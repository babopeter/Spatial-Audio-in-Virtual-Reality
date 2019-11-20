/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using OscSimpl;

/// <summary>
/// Class representing an OSC bundle. Bundles have a OscTimeTag and can contain OscMessage
/// and OscBundle objects.
/// </summary>
public sealed class OscBundle : OscPacket
{
	OscTimeTag _timeTag;
	List<OscPacket> _packets;


	/// <summary>
	/// Gets or sets the timetag for this bundle.
	/// </summary>
	public OscTimeTag timeTag
	{
		get { return _timeTag; }
		set { _timeTag = value; }
	}

	/// <summary>
	/// Gets the list of OscMessage and OscBundle objects.
	/// </summary>
	public List<OscPacket> packets { get { return _packets; } }

	/// <summary>
	/// Constructor for creating a bundle with a timetag containing the current time.
	/// </summary>
	public OscBundle() : this( new OscTimeTag( DateTime.Now ) ) { }

	/// <summary>
	/// Constructor for creating a bundle with specified timetag.
	/// </summary>
	public OscBundle( OscTimeTag timeTag )
	{
		_timeTag = timeTag;
		_packets = new List<OscPacket>();
	}

	/// <summary>
	/// Add a OscMessage or OscBundle to this bundle. Shorthand for bundle.packets.Add.
	/// </summary>
	public void Add( OscPacket packet )
	{
		_packets.Add( packet );
	}

	/// <summary>
	/// Remove all OscMessage and OscBundle object in this bundle, and
	/// do so recursively for all contained bundles.
	/// </summary>
	public void Clear()
	{
		foreach( OscPacket packet in _packets ) {
			if( packet is OscBundle ) (packet as OscBundle).Clear();
		}
		_packets.Clear();
	}

	// Undocumented on purpose.
	public override bool TryWriteTo( byte[] data, ref int index )
	{
		// Null check.
		if( data == null ) {
			StringBuilder sb = OscDebug.BuildText( this );
			sb.Append( "Write failed. Buffer cannot be null.\n" );
			Debug.LogWarning( sb.ToString() );
			return false;
		}

		// Capacity check.
		int size = Size();
		if( index + size > data.Length ) {
			StringBuilder sb = OscDebug.BuildText( this );
			sb.Append( "Write failed. Buffer capacity insufficient.\n" );
			Debug.LogWarning( sb.ToString() );
			return false;
		}

		// Write header.
		TryWriteHeader( _timeTag, data, ref index );

		// Write packets.
		foreach( OscPacket packet in _packets ) {
			if( packet == null ) continue;

			// Silently ensure that nested bundle's timetag is >= than parent bundle's timetag.
			if( packet is OscBundle && (packet as OscBundle).timeTag < _timeTag ) {
				(packet as OscBundle).timeTag = _timeTag;
			}

			// Reserve space for writing package length.
			int packetSizeIndex = index;
			index += 4;

			// Write packet to bytes.
			int packetContentIndex = index;
			if( !packet.TryWriteTo( data, ref index ) ) return false;
			int packetByteCount = index - packetContentIndex;

			// Write packet byte size (before packet data).
			if( !new FourByteOscData( packetByteCount ).TryWriteTo( data, ref packetSizeIndex ) ) {
				Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
				return false;
			}
		}

		return true;
	}


	public static bool TryWriteHeader( OscTimeTag timeTag, byte[] data, ref int index )
	{
		// Check capacity.
		if( index + OscConst.bundleHeaderSize > data.Length ) return false;

		// Add prefix.
		Array.Copy( OscConst.bundlePrefixBytes, 0, data, index, OscConst.bundlePrefixBytes.Length );
		index += OscConst.bundlePrefixBytes.Length;

		// Add timetag.
		if( !new EightByteOscData( timeTag ).TryWriteTo( data, ref index ) ) return false;

		return true;
	}


	public static bool TryReadFrom( byte[] data, ref int index, int byteCount, out OscBundle bundle )
	{
		// Check header size (prefix + timetag).
		if( data.Length - index < OscConst.bundleHeaderSize ) {
			Debug.LogWarning( "[OscParser] OscBundle with invalid header was ignored." + Environment.NewLine ); // TODO make warnings optional
			bundle = null;
			return false;
		}

		// Check prefix.
		if( !ReadAndValidatePrefix( data, ref index ) ) {
			Debug.LogWarning( "[OscParser] OscBundle with invalid header was ignored." + Environment.NewLine );  // TODO make warnings optional
			bundle = null;
			return false;
		}

		// Try get recycled bundle from the pool, otherwise create new.
		bundle = OscPool.GetBundle();

		// Get timetag osc ntp.
		EightByteOscData timeTagDataValue;
		EightByteOscData.TryReadFrom( data, ref index, out timeTagDataValue );
		bundle.timeTag = timeTagDataValue.timeTagValue;

		// Fill Bundle.
		while( index < byteCount )
		{
			FourByteOscData packetSizeDataValue;
			if( !FourByteOscData.TryReadFrom( data, ref index, out packetSizeDataValue ) ) break;
			if( index + packetSizeDataValue.intValue > data.Length ) {
				Debug.LogWarning( "[OscParser] Incomplete OscBundle was ignored.\nPerhaps your UDP buffer size is too small.\n" );  // TODO make warnings optional
				bundle = null;
				return false;
			}

			OscPacket subPacket;
			if( TryReadFrom( data, ref index, byteCount, out subPacket ) ){
				bundle.Add( subPacket );
			} else {
				Debug.Log( "Failed to read packet." ); // TODO make warnings optional
			}
		}

		// Done.
		return true;
	}


	/// <summary>
	/// Returns the byte size of the bundle.
	/// </summary>
	public override int Size()
	{
		int size = OscConst.bundleHeaderSize;
		foreach( OscPacket packet in _packets ) {
			if( packet  == null ) continue;
			size += 4; // size header.
			size += packet.Size();
		}
		return size;
	}


	static bool ReadAndValidatePrefix( byte[] data, ref int index )
	{
		for( int i = 0; i < OscConst.bundlePrefixBytes.Length; i++ ) if( data[index++] != OscConst.bundlePrefixBytes[i] ) return false;
		return true;
	}
}