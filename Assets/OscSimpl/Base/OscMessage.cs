/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2019 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using OscSimpl;

/// <summary>
/// Class representing an OSC message. Messages have an OSC address an a number of OSC arguments.
/// </summary>
[Serializable]
public class OscMessage : OscPacket
{
	string _address;
	List<byte> _argData;
	List<OscArgInfo> _argInfo;

	bool _dirtySize = true;
	int _cachedSize;

	bool _dirtyAddressHash = true;
	int _addressHash;


	// We need two string builders because we may want to build for logging while building for ToString.
	static StringBuilder _toStringSB; // Used for ToString


	/// <summary>
	/// Gets or sets the OSC Address Pattern of the message. Must start with '/'.
	/// </summary>
	public string address {
		get { return _address; }
		set {
			OscAddress.Sanitize( ref value );
			_address = value;
			_dirtySize = true;
		}
	}


	/// <summary>
	/// Creates a new OSC Message.
	/// </summary>
	public OscMessage()
	{
		_argData = new List<byte>();
		_argInfo = new List<OscArgInfo>();
	}


	/// <summary>
	/// Creates a new OSC Message with address.
	/// </summary>
	public OscMessage( string address ) : this()
	{
		_address = address;
	}


	/// <summary>
	/// Clears all arguments.
	/// </summary>
	public void Clear()
	{
		_argData.Clear();
		_argInfo.Clear();
		_dirtySize = true;
		_dirtyAddressHash = true;
	}


	/// <summary>
	/// Returns the number of arguments.
	/// </summary>
	public int Count()
	{
		return _argInfo.Count;
	}


	/// <summary>
	/// Removes argument at index, shifting the subsequent arguments one index down.
	/// </summary>
	public void RemoveAt( int index )
	{
		// Bounds check.
		if( index < 0 || index >= _argInfo.Count ) return;

		// Check if arg contains data.
		int dataSize = _argInfo[index].size;
		if( dataSize > 0 ) {
			// Remove data.
			int dataStartIndex = GetDataIndex( index );
			_argData.RemoveRange( dataStartIndex, dataSize );
		}

		// Remove info.
		_argInfo.RemoveAt( index );

		// Request update byte count.
		_dirtySize = true;
	}


	/// <summary>
	/// Returns the argument type at index.
	/// </summary>
	public bool TryGetArgType( int index, out OscArgType type )
	{
		if( index < 0 || index >= _argInfo.Count ) {
			type = OscArgType.Unsupported;
			return false;
		}
		type = OscConverter.ToArgType( _argInfo[index].tagByte );
		return true;
	}


	/// <summary>
	/// Tries to get argument tag at index. Returns success status.
	/// </summary>
	public bool TryGetArgTag( int index, out char tag )
	{
		// Arg bounds.
		if( index < 0 || index >= _argInfo.Count ) {
			tag = (char) OscConst.tagUnsupportedByte;
			return false;
		}

		// Get.
		tag = (char) _argInfo[index].tagByte;
		return true;
	}


	/// <summary>
	/// Tries to get argument byte size at index. Returns success status.
	/// </summary>
	bool TryGetArgSize( int index, out int size )
	{
		// Arg bounds.
		if( index < 0 || index >= _argInfo.Count ) {
			size = 0;
			return false;
		}

		// Get.
		size = _argInfo[index].size;
		return true;
	}


	/// <summary>
	/// Tries to get argument at index of type float. Returns success status.
	/// </summary>
	public bool TryGet( int index, out float value )
	{
		if( !ValidateTryGet( index, OscArgType.Float ) ) {
			value = 0;
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		FourByteOscData dataValue;
		if( !FourByteOscData.TryReadFrom( _argData, ref dataStartIndex, out dataValue ) ) {
			value = 0;
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		value = dataValue.floatValue;
		return true;
	}


	/// <summary>
	/// Tries to get argument at index of type double. Returns success status.
	/// </summary>
	public bool TryGet( int index, out double value )
	{
		if( !ValidateTryGet( index, OscArgType.Double ) ) {
			value = 0;
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		EightByteOscData dataValue;
		if( !EightByteOscData.TryReadFrom( _argData, ref dataStartIndex, out dataValue ) ) {
			value = 0;
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		value = dataValue.doubleValue;
		return true;
	}


	/// <summary>
	/// Tries to get argument at index of type int. Returns success status.
	/// </summary>
	public bool TryGet( int index, out int value )
	{
		if( !ValidateTryGet( index, OscArgType.Int ) ) {
			value = 0;
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		FourByteOscData dataValue;
		if( !FourByteOscData.TryReadFrom( _argData, ref dataStartIndex, out dataValue ) ) {
			value = 0;
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		value = dataValue.intValue;
		return true;
	}


	/// <summary>
	/// Tries to get argument at index of type long. Returns success status.
	/// </summary>
	public bool TryGet( int index, out long value )
	{
		if( !ValidateTryGet( index, OscArgType.Long ) ) {
			value = 0;
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		EightByteOscData dataValue;
		if( !EightByteOscData.TryReadFrom( _argData, ref dataStartIndex, out dataValue ) ) {
			value = 0;
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		value = dataValue.longValue;
		return true;
	}


	/// <summary>
	/// Tries to get argument at index of type string. Returns success status.
	/// </summary>
	public bool TryGet( int index, ref string value )
	{
		if( !ValidateTryGet( index, OscArgType.String ) ) {
			value = string.Empty;
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		if( !StringOscData.TryReadFrom( _argData, ref dataStartIndex, ref value ) ) {
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		return true;
	}



	/// <summary>
	/// Tries to get argument at index of type char. Returns success status.
	/// </summary>
	public bool TryGet( int index, out char value )
	{
		if( !ValidateTryGet( index, OscArgType.Char ) ) {
			value = (char) OscConst.tagUnsupportedByte;
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		FourByteOscData dataValue;
		if( !FourByteOscData.TryReadFrom( _argData, ref dataStartIndex, out dataValue ) ) {
			value = ' ';
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		value = dataValue.charValue;
		return true;
	}


	/// <summary>
	/// Tries to get argument at index of type bool. Returns success status.
	/// </summary>
	public bool TryGet( int index, out bool value )
	{
		if( !ValidateTryGet( index, OscArgType.Bool ) ) {
			value = false;
			return false;
		}

		// Get.
		value = _argInfo[index].tagByte == OscConst.tagTrueByte;
		return true;
	}


	/// <summary>
	/// Tries to get argument at index of type color. Returns success status.
	/// </summary>
	public bool TryGet( int index, out Color32 value )
	{
		if( !ValidateTryGet( index, OscArgType.Color ) ) {
			value = new Color32();
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		FourByteOscData dataValue;
		if( !FourByteOscData.TryReadFrom( _argData, ref dataStartIndex, out dataValue ) ) {
			value = new Color32();
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		value = dataValue.colorValue;
		return true;
	}


	/// <summary>
	/// Tries to get argument at index of type blob (byte[]). Returns success status.
	/// </summary>
	public bool TryGet( int index, ref byte[] value )
	{
		if( !ValidateTryGet( index, OscArgType.Blob ) ) {
			value = null;
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		if( !BlobOscData.TryReadFrom( _argData, ref dataStartIndex, ref value ) ) {
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		return true;
	}


	/// <summary>
	/// Tries to get argument at index of type time tag. Returns success status.
	/// </summary>
	public bool TryGet( int index, out OscTimeTag value )
	{
		if( !ValidateTryGet( index, OscArgType.TimeTag ) ) {
			value = new OscTimeTag();
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		EightByteOscData dataValue;
		if( !EightByteOscData.TryReadFrom( _argData, ref dataStartIndex, out dataValue ) ) {
			value = new OscTimeTag();
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		value = dataValue.timeTagValue;
		return true;
	}


	/// <summary>
	/// Tries to get argument at index of midi message. Returns success status.
	/// </summary>
	public bool TryGet( int index, out OscMidiMessage value )
	{
		if( !ValidateTryGet( index, OscArgType.Midi ) ) {
			value = new OscMidiMessage();
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		FourByteOscData dataValue;
		if( !FourByteOscData.TryReadFrom( _argData, ref dataStartIndex, out dataValue ) ) {
			value = new OscMidiMessage();
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		value = dataValue.midiMessage;
		return true;
	}


	/// <summary>
	/// Tries to get argument at index of type null. Returns success status.
	/// </summary>
	public bool TryGet( int index, OscNull nothing )
	{
		return ValidateTryGet( index, OscArgType.Null );
	}


	/// <summary>
	/// Tries to get argument at index of type impulse. Returns success status.
	/// </summary>
	public bool TryGet( int index, OscImpulse impulse )
	{
		return ValidateTryGet( index, OscArgType.Impulse );
	}


	/// <summary>
	/// Set argument at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage Set( int index, float value )
	{
		int dataStartIndex = AdaptiveSet( index, OscArgInfo.floatInfo );
		if( !new FourByteOscData( value ).TryWriteTo( _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Set argument at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage Set( int index, double value )
	{
		int dataStartIndex = AdaptiveSet( index, OscArgInfo.doubleInfo );
		if( !new EightByteOscData( value ).TryWriteTo( _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Set argument at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage Set( int index, int value )
	{
		int dataStartIndex = AdaptiveSet( index, OscArgInfo.intInfo );
		if( !new FourByteOscData( value ).TryWriteTo( _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Set argument at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage Set( int index, long value )
	{
		int dataStartIndex = AdaptiveSet( index, OscArgInfo.longInfo );
		if( !new EightByteOscData( value ).TryWriteTo( _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Set argument at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage Set( int index, string value )
	{
		OscArgInfo info = new OscArgInfo( OscConst.tagStringByte, StringOscData.EvaluateByteCount( value ) );
		int dataStartIndex = AdaptiveSet( index, info );
		if( !StringOscData.TryWriteTo( value, _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Set argument at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage Set( int index, char value )
	{
		int dataStartIndex = AdaptiveSet( index, OscArgInfo.charInfo );
		if( !new FourByteOscData( value ).TryWriteTo( _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Set argument at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage Set( int index, bool value )
	{
		AdaptiveSet( index, value ? OscArgInfo.boolTrueInfo : OscArgInfo.boolFalseInfo );
		return this;
	}


	/// <summary>
	/// Set argument at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage Set( int index, Color32 value )
	{
		int dataStartIndex = AdaptiveSet( index, OscArgInfo.colorInfo );
		if( !new FourByteOscData( value ).TryWriteTo( _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Set argument at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage Set( int index, byte[] value )
	{
		OscArgInfo info = new OscArgInfo( OscConst.tagBlobByte, BlobOscData.EvaluateByteCount( value ) );
		int dataStartIndex = AdaptiveSet( index, info );
		if( !BlobOscData.TryWriteTo( value, _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}

	
	/// <summary>
	/// Set argument at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage Set( int index, OscTimeTag value )
	{
		int dataStartIndex = AdaptiveSet( index, OscArgInfo.timeTagInfo );
		if( !new EightByteOscData( value ).TryWriteTo( _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Set argument at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage Set( int index, OscMidiMessage value )
	{
		int dataStartIndex = AdaptiveSet( index, OscArgInfo.midiInfo );
		if( !new FourByteOscData( value ).TryWriteTo( _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Set argument at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage Set( int index, OscNull nothing )
	{
		AdaptiveSet( index, OscArgInfo.nullInfo );
		return this;
	}


	/// <summary>
	/// Set argument at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage Set( int index, OscImpulse impulse )
	{
		AdaptiveSet( index, OscArgInfo.impulseInfo );
		return this;
	}


	/// <summary>
	/// Adds argument.
	/// </summary>
	public OscMessage Add( float value ){ Set( _argInfo.Count, value ); return this; }


	/// <summary>
	/// Adds argument.
	/// </summary>
	public OscMessage Add( double value ) { Set( _argInfo.Count, value ); return this; }
	

	/// <summary>
	/// Adds argument.
	/// </summary>
	public OscMessage Add( int value ){ Set( _argInfo.Count, value ); return this; }


	/// <summary>
	/// Adds argument.
	/// </summary>
	public OscMessage Add( long value ) { Set( _argInfo.Count, value ); return this; }


	/// <summary>
	/// Adds argument.
	/// </summary>
	public OscMessage Add( string value ) { Set( _argInfo.Count, value ); return this; }


	/// <summary>
	/// Adds argument.
	/// </summary>
	public OscMessage Add( char value ) { Set( _argInfo.Count, value ); return this; }


	/// <summary>
	/// Adds argument.
	/// </summary>
	public OscMessage Add( bool value ) { Set( _argInfo.Count, value ); return this; }
	
	
	/// <summary>
	/// Adds argument.
	/// </summary>
	public OscMessage Add( Color32 value ){ Set( _argInfo.Count, value ); return this; }


	/// <summary>
	/// Adds argument.
	/// </summary>
	public OscMessage Add( byte[] value ) { Set( _argInfo.Count, value ); return this; }


	/// <summary>
	/// Adds argument.
	/// </summary>
	public OscMessage Add( OscTimeTag value ){ Set( _argInfo.Count, value ); return this; }
	
	
	/// <summary>
	/// Adds argument.
	/// </summary>
	public OscMessage Add( OscMidiMessage value ){ Set( _argInfo.Count, value ); return this; }


	/// <summary>
	/// Adds argument.
	/// </summary>
	public OscMessage Add( OscNull nothing ) { Set( _argInfo.Count, nothing ); return this; }


	/// <summary>
	/// Adds argument.
	/// </summary>
	public OscMessage Add( OscImpulse impulse ) { Set( _argInfo.Count, impulse ); return this; }


	/// <summary>
	/// Tries to get value at index of from a byte blob. Returns success status.
	/// </summary>
	public bool TryGetBlob( int index, out Vector2 value )
	{
		if( !ValidateTryGet( index, OscArgType.Blob ) ) {
			value = Vector2.zero;
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		if( !BlobOscData.TryReadFrom( _argData, ref dataStartIndex, out value ) ) {
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		return true;
	}


	/// <summary>
	/// Tries to get value at index of from a byte blob. Returns success status.
	/// </summary>
	public bool TryGetBlob( int index, out Vector3 value )
	{
		if( !ValidateTryGet( index, OscArgType.Blob ) ) {
			value = Vector3.zero;
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		if( !BlobOscData.TryReadFrom( _argData, ref dataStartIndex, out value ) ) {
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		return true;
	}


	/// <summary>
	/// Tries to get value at index of from a byte blob. Returns success status.
	/// </summary>
	public bool TryGetBlob( int index, out Vector4 value )
	{
		if( !ValidateTryGet( index, OscArgType.Blob ) ) {
			value = Vector4.zero;
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		if( !BlobOscData.TryReadFrom( _argData, ref dataStartIndex, out value ) ) {
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		return true;
	}


	/// <summary>
	/// Tries to get value at index of from a byte blob. Returns success status.
	/// </summary>
	public bool TryGetBlob( int index, out Quaternion value )
	{
		if( !ValidateTryGet( index, OscArgType.Blob ) ) {
			value = Quaternion.identity;
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		if( !BlobOscData.TryReadFrom( _argData, ref dataStartIndex, out value ) ) {
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		return true;
	}


	/// <summary>
	/// Tries to get value at index of from a byte blob. Returns success status.
	/// </summary>
	public bool TryGetBlob( int index, out Matrix4x4 value )
	{
		if( !ValidateTryGet( index, OscArgType.Blob ) ) {
			value = Matrix4x4.identity;
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		if( !BlobOscData.TryReadFrom( _argData, ref dataStartIndex, out value ) ) {
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		return true;
	}


	/// <summary>
	/// Tries to read a list of values from a byte blob at argument index. Returns success status.
	/// </summary>
	public bool TryGetBlob( int index, ref List<float> values )
	{
		if( !ValidateTryGet( index, OscArgType.Blob ) ) return false;

		// Get.
		int dataStartIndex = GetDataIndex( index );
		if( !BlobOscData.TryReadFrom( _argData, ref dataStartIndex, ref values ) ) {
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		return true;
	}


	/// <summary>
	/// Tries to read a list of values from a byte blob at argument index. Returns success status.
	/// </summary>
	public bool TryGetBlob( int index, ref List<int> values )
	{
		if( !ValidateTryGet( index, OscArgType.Blob ) ) return false;

		// Get.
		int dataStartIndex = GetDataIndex( index );
		if( !BlobOscData.TryReadFrom( _argData, ref dataStartIndex, ref values ) ) {
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		return true;
	}


	/// <summary>
	/// Tries to read string with a given encoding from a byte blob at argument index. Returns success status.
	/// </summary>
	public bool TryGetBlob( int index, Encoding encoding, out string value )
	{
		if( !ValidateTryGet( index, OscArgType.Blob ) ) {
			value = string.Empty;
			return false;
		}

		// Get.
		int dataStartIndex = GetDataIndex( index );
		if( !BlobOscData.TryReadFrom( _argData, ref dataStartIndex, encoding, out value ) ) {
			Debug.Log( OscDebug.FailedReadingBytesWarning( this ) );
			return false;
		}
		return true;
	}


	/// <summary>
	/// Set a value as a byte blob at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage SetBlob( int index, Vector2 value )
	{
		int dataStartIndex = AdaptiveSet( index, OscArgInfo.eightByteBlobInfo );
		if( !BlobOscData.TryWriteTo( value, _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Set a value as a byte blob at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage SetBlob( int index, Vector3 value )
	{
		int dataStartIndex = AdaptiveSet( index, OscArgInfo.twelveByteBlobInfo );
		if( !BlobOscData.TryWriteTo( value, _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Set a value as a byte blob at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage SetBlob( int index, Vector4 value )
	{
		int dataStartIndex = AdaptiveSet( index, OscArgInfo.sixteenByteBlobInfo );
		if( !BlobOscData.TryWriteTo( value, _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Set a value as a byte blob at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage SetBlob( int index, Quaternion value )
	{
		int dataStartIndex = AdaptiveSet( index, OscArgInfo.sixteenByteBlobInfo );
		if( !BlobOscData.TryWriteTo( value, _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Set a value as a byte blob at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage SetBlob( int index, Matrix4x4 value )
	{
		int dataStartIndex = AdaptiveSet( index, OscArgInfo.sixtyfourByteBlobInfo );
		if( !BlobOscData.TryWriteTo( value, _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Writes a list of values to a byte blob at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage SetBlob( int index, IList<float> values )
	{
		OscArgInfo info = new OscArgInfo( OscConst.tagBlobByte, (1+values.Count) * FourByteOscData.byteCount );
		int dataStartIndex = AdaptiveSet( index, info );
		if( !BlobOscData.TryWriteTo( values, _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Writes a list of values to a byte blob at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage SetBlob( int index, IList<int> values )
	{
		OscArgInfo info = new OscArgInfo( OscConst.tagBlobByte, (1+values.Count) * FourByteOscData.byteCount );
		int dataStartIndex = AdaptiveSet( index, info );
		if( !BlobOscData.TryWriteTo( values, _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Writes a string with a given encoding to a byte blob at specified index, expanding message capacity if necessary.
	/// </summary>
	public OscMessage SetBlob( int index, Encoding encoding, string value )
	{
		OscArgInfo info = new OscArgInfo( OscConst.tagBlobByte, BlobOscData.EvaluateByteCount( value, encoding ) );
		int dataStartIndex = AdaptiveSet( index, info );
		if( !BlobOscData.TryWriteTo( value, encoding, _argData, ref dataStartIndex ) ) {
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
		}
		return this;
	}


	/// <summary>
	/// Returns the byte size of the message.
	/// </summary>
	public override int Size()
	{
		if( !_dirtySize ) return _cachedSize;

		_cachedSize = 0;

		// Address.
		_cachedSize += StringOscData.EvaluateByteCount( _address );

		// Tags.
		_cachedSize++;                    // Prefix;
		_cachedSize += _argInfo.Count;    // ASCII char tags.
		_cachedSize += 4 - (_cachedSize % 4);   // Followed by at least one trailing zero, multiple of four bytes.

		// Data, already multiple of four bytes.
		int argCount = _argInfo.Count;
		for( int i = 0; i < argCount; i++ ) _cachedSize += _argInfo[i].size;

		return _cachedSize;
	}


	// Undocumented on purpose.
	public override bool TryWriteTo( byte[] data, ref int index )
	{
		// Null check.
		if( data == null ){
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

		// Address.
		if( !StringOscData.TryWriteTo( _address, data, ref index ) ){
			Debug.Log( OscDebug.FailedWritingBytesWarning( this ) );
			return false;
		}

		// Tag prefix.
		data[index++] = OscConst.tagPrefixByte;

		// Argument tags.
		int argCount = _argInfo.Count;
		for( int i = 0; i < argCount; i++ ) data[index++] = _argInfo[i].tagByte;

		// Followed by at least one trailing zero, multiple of four bytes.
		int trailingNullCount = 4 - (index % 4);
		for( int i = 0; i < trailingNullCount; i++ ) data[index++] = 0;

		//Debug.Log( "WRITE: Args data start index: " + index );

		// Argument data.
		_argData.CopyTo( data, index );
		index += _argData.Count;

		// Cache byte count.
		_cachedSize = size;
		_dirtySize = false;

		return true;
	}


	// Undocumented on purpose.
	public static bool TryReadFrom( byte[] data, ref int index, int size, ref OscMessage message )
	{
		int beginIndex = index;

		// If we are not provided with a message, then read the lossy hash and try reuse from the pool.
		if( message == null ){
			int hash = OscStringHash.Pack( data, index );
			message = OscPool.GetMessage( hash );
		} else {
			if( message._argInfo.Count > 0 ) message.Clear(); // Ensure that arguments are cleared.
		}

		// Address.
		string address = message._address;
		if( !StringOscData.TryReadFrom( data, ref index, ref address ) ) {
			Debug.Log( OscDebug.FailedReadingBytesWarning( message ) );
			return false;
		}
		message._address = address;

		// Tag prefix.
		if( data[index] != OscConst.tagPrefixByte ){
			StringBuilder sb = OscDebug.BuildText( message );
			sb.Append( "Read failed. Tag prefix missing.\n" );
			Debug.LogWarning( sb.ToString() );
			return false;
		}
		index++;

		// Argument tags.
		for( int i = index; i < data.Length && data[i] != 0; i++ ){
			message._argInfo.Add( new OscArgInfo( data[i], 0 ) );
		}
		index += message._argInfo.Count;

		// Followed by at least one trailing zero, multiple of four bytes.
		index += 4 - (index % 4);

		//Debug.Log( "READ: Args data start index: " + index );

		// Argument data info.
		int argDataByteCount = 0;
		for( int i = 0; i < message._argInfo.Count; i++ )
		{
			byte tagByte = message._argInfo[i].tagByte;
			int argByteCount = 0;
			switch( tagByte )
			{
				case OscConst.tagNullByte:
				case OscConst.tagImpulseByte:
				case OscConst.tagTrueByte:
				case OscConst.tagFalseByte:
					break;
				case OscConst.tagFloatByte:
				case OscConst.tagIntByte:
				case OscConst.tagCharByte:
				case OscConst.tagColorByte:
				case OscConst.tagMidiByte:
					argByteCount = 4;
					break;
				case OscConst.tagDoubleByte:
				case OscConst.tagLongByte:
				case OscConst.tagTimetagByte:
					argByteCount = 8;
					break;
				case OscConst.tagStringByte:
				case OscConst.tagSymbolByte:
					argByteCount = StringOscData.EvaluateByteCount( data, index + argDataByteCount );
					break;
				case OscConst.tagBlobByte:
					BlobOscData.TryEvaluateByteCount( data, index + argDataByteCount, out argByteCount );
					break;
				default:
					StringBuilder sb = OscDebug.BuildText( message );
					sb.Append( "Read failed. Tag '" ); sb.Append( (char) tagByte ); sb.Append( "' is not supported\n" );
					Debug.LogWarning( sb.ToString() );
					return false;
			}
			message._argInfo[i] = new OscArgInfo( tagByte, argByteCount );
			//Debug.Log( "i; " + i + ", info: " + message._argInfo[i] );
			argDataByteCount += argByteCount;
		}

		// AdaptiveSet data list.
		if( message._argData.Capacity < argDataByteCount ) message._argData.Capacity = argDataByteCount;

		// Read data.
		for( int i = 0; i < argDataByteCount; i++ ) message._argData.Add( data[index++] );

		// Cache byte count.
		message._cachedSize = index - beginIndex;
		message._dirtySize = false;

		return true;
	}


	/// <summary>
	/// Alternative ToString() method that appends to a StringBuilder to avoid creating a new string.
	/// Set optional argument appendNewLineAtEnd append a new line at the end.
	/// Set optional argument insertIndex to Insert instead of Append.
	/// </summary>
	public void ToString( StringBuilder sb, bool appendNewLineAtEnd = false, int insertIndex = -1 )
	{
		StringBuilder tsb; // Temp String Builder
		if( insertIndex >= 0 ){
			if( _toStringSB == null ) _toStringSB = new StringBuilder();
			else _toStringSB.Clear();
			tsb = _toStringSB;
		}  else {
			tsb = sb;
		}

		tsb.Append( _address );
		for( int i = 0; i < _argInfo.Count; i++ )
		{
			tsb.Append( ' ' );

			OscArgType type;
			if( !TryGetArgType( i, out type ) ) continue;

			switch( type ) {
				case OscArgType.Null:
					tsb.Append( "Null" ); break;
				case OscArgType.Impulse:
					tsb.Append( "Impulse" ); break;
				case OscArgType.Bool:
					bool boolValue;
					if( TryGet( i, out boolValue ) ) tsb.Append( boolValue );
					else tsb.Append( "ERROR" );
					break;
				case OscArgType.Float:
					float floatValue;
					if( TryGet( i, out floatValue ) ) tsb.Append( floatValue );
					else tsb.Append( "ERROR" );
					break;
				case OscArgType.Int:
					int intValue;
					if( TryGet( i, out intValue ) ) tsb.Append( intValue );
					else tsb.Append( "ERROR" );
					break;
				case OscArgType.Char:
					char charValue;
					if( TryGet( i, out charValue ) ) { tsb.Append( "'" ); tsb.Append( charValue ); tsb.Append( "'" ); } else { tsb.Append( "ERROR" ); }
					break;
				case OscArgType.Color:
					Color32 colorValue;
					if( TryGet( i, out colorValue ) ) tsb.Append( colorValue );
					else tsb.Append( "ERROR" );
					break;
				case OscArgType.Double:
					double doubleValue;
					if( TryGet( i, out doubleValue ) ) tsb.Append( doubleValue );
					else tsb.Append( "ERROR" );
					break;
				case OscArgType.Long:
					long longValue;
					if( TryGet( i, out longValue ) ) tsb.Append( longValue );
					else tsb.Append( "ERROR" );
					break;
				case OscArgType.TimeTag:
					OscTimeTag timeTagValue;
					if( TryGet( i, out timeTagValue ) ) tsb.Append( timeTagValue );
					else tsb.Append( "ERROR" );
					break;
				case OscArgType.String:
					string stringValue = string.Empty;
					if( TryGet( i, ref stringValue ) ) { tsb.Append( "\"" ); tsb.Append( stringValue ); tsb.Append( "\"" ); } else { tsb.Append( "ERROR" ); }
					break;
				case OscArgType.Blob:
					byte[] blobValue = null;
					if( TryGet( i, ref blobValue ) ) { tsb.Append( "Blob[" ); tsb.Append( blobValue.Length ); tsb.Append( "]" ); } else { tsb.Append( "ERROR" ); }
					break;
			}
		}

		if( appendNewLineAtEnd ) tsb.AppendLine();

		if( insertIndex >= 0 ){
			sb.EnsureCapacity( sb.Length + tsb.Length );
			// It seems to produce a bit less garbage if we insert each char instead of the string.
			for( int i = 0; i < tsb.Length; i++ ) sb.Insert( insertIndex++, tsb[i] );
		}
	}


	// Undocumented on purpose.
	public override string ToString()
	{
		if( _toStringSB == null ) _toStringSB = new StringBuilder();
		else _toStringSB.Clear();
		ToString( _toStringSB );
		return _toStringSB.ToString();	
	}


	// Undocumented on purpose.
	public int GetAddressHash()
	{
		// Return cached when possible.
		if( !_dirtyAddressHash ) return _addressHash;

		// Compute and return.
		_addressHash = OscStringHash.Pack( _address );
		_dirtyAddressHash = false;
		return _addressHash;
	}


	int GetDataIndex( int argIndex )
	{
		int dataStartIndex = 0;
		for( int i = 0; i < argIndex; i++ ) dataStartIndex += _argInfo[i].size;
		return dataStartIndex;
	}



	int AdaptiveSet( int index, OscArgInfo info )
	{
		// Check for change.
		bool shouldAddArg = index >= _argInfo.Count;
		OscArgInfo oldInfo = shouldAddArg ? OscArgInfo.undefinedInfo : _argInfo[index];
		if( info.tagByte == oldInfo.tagByte && info.size == oldInfo.size ){
			// No adaptation needed.
			return GetDataIndex( index );
		}

		// Adapt info list.
		if( shouldAddArg ){
			int requiredArgCount = index+1;
			if( requiredArgCount > _argInfo.Capacity ) _argInfo.Capacity = requiredArgCount;
			for( int i = _argInfo.Count; i < requiredArgCount; i++ ) _argInfo.Add( OscArgInfo.nullInfo );
		}

		// Get start index and old byte count for data.
		int oldDataSize = 0;
		int dataStartIndex = 0;
		for( int i = 0; i < _argInfo.Count; i++ )
		{
			if( i == index ) dataStartIndex = oldDataSize;
			oldDataSize += _argInfo[i].size;
		}

		// Adapt data list.
		int byteDelta = info.size - oldInfo.size;
		if( byteDelta > 0 ){
			int newDataByteCount = oldDataSize + byteDelta;
			if( newDataByteCount > _argData.Capacity ) _argData.Capacity = newDataByteCount;
			// If last argument then add to end, otherwise insert to preserve existing data.
			if( index == _argInfo.Count-1 ) {
				for( int i = 0; i < byteDelta; i++ ) _argData.Add( 0 );
			} else {
				for( int i = 0; i < byteDelta; i++ ) _argData.Insert( dataStartIndex, 0 );
			}
		} else if( byteDelta < 0 ) {
			_argData.RemoveRange( dataStartIndex, -byteDelta );
		}

		// Overwrite info.
		_argInfo[index] = info;

		// Request byte count update.
		_dirtySize = true;

		return dataStartIndex;
	}


	bool ValidateTryGet( int index, OscArgType requestedType )
	{
		// Arg bounds.
		if( index < 0 || index >= _argInfo.Count ){
			StringBuilder sb = StartBuildingInvalidTryGetString( OscDebug.BuildText( this ) );
			sb.Append( "Requested argument index " ); sb.Append( index );
			sb.Append( " is out of bounds. Message has " ); sb.Append( _argInfo.Count );
			sb.Append( " arguments.\n" );
			Debug.LogWarning( sb.ToString() );
			return false;
		}

		// Arg type.
		OscArgInfo info = _argInfo[index];
		OscArgType type = OscConverter.ToArgType( info.tagByte );
		if( requestedType != type ){
			StringBuilder sb = StartBuildingInvalidTryGetString( OscDebug.BuildText( this ) );
			sb.Append( "Argument at index " ); sb.Append( index );
			sb.Append( " is not type " ); sb.Append( requestedType );
			sb.Append( " ('" ); sb.Append( (char) OscConverter.ToTagByte( requestedType ) ); sb.Append( "')");
			sb.Append( ", it is " ); sb.Append( type );
			sb.Append( " ('" ); sb.Append( (char) info.tagByte );
			sb.Append( "').\n" );
			Debug.LogWarning( sb.ToString() );
			return false;
		}

		// Data capacity.
		if( index + info.size > _argData.Count ){
			StringBuilder sb = StartBuildingInvalidTryGetString( OscDebug.BuildText( this ) );
			sb.Append( "Argument at index " ); sb.Append( index );
			sb.Append( " has incomplete data\n" );
			Debug.LogWarning( sb.ToString() );
			return false;
		}

		return true;
	}


	StringBuilder StartBuildingInvalidTryGetString( StringBuilder sb )
	{
		sb.Append( "TryGet failed for message with address \"" ); sb.Append( _address ); sb.Append( "\".\n" );
		return sb;
	}



	[Obsolete( "Use new OscMessage(address) instead and call AddArg or SetArg subsequently to set arguments." )]
	public OscMessage( string address, params object[] args ) : this( address )
	{
		Add( args  );
	}


	[Obsolete( "Use TryGet( int index, OscNull nothing ) instead." )]
	public bool TryGetNull( int index )
	{
		return ValidateTryGet( index, OscArgType.Null );
	}


	[Obsolete( "Use TryGet( int index, OscImpulse impulse ) instead." )]
	public bool TryGetImpulse( int index )
	{
		return ValidateTryGet( index, OscArgType.Impulse );
	}


	[Obsolete( "Use Set( int index, OscNull nothing ) instead." )]
	public void SetNull( int index )
	{
		AdaptiveSet( index, OscArgInfo.nullInfo );
	}


	[Obsolete( "Use Set( int index, OscImpulse impulse ) instead." )]
	public void SetImpulse( int index )
	{
		AdaptiveSet( index, OscArgInfo.impulseInfo );
	}


	[Obsolete( "Please use Add or Set." )]
	public void Add( params object[] args )
	{
		// Adaptive Set info capacity.
		int infoStartIndex = _argInfo.Count;
		int newArgCount = infoStartIndex + args.Length;
		if( newArgCount > _argInfo.Capacity ) _argInfo.Capacity = newArgCount;

		// Get info and evaluate data byte count.
		int newArgsByteCount = 0;
		foreach( object arg in args )
		{
			byte tagByte = OscConverter.ToTagByte( arg );

			int argByteCount = 0;
			switch( tagByte ) {
				case OscConst.tagFloatByte:
				case OscConst.tagIntByte:
				case OscConst.tagCharByte:
				case OscConst.tagColorByte:
				case OscConst.tagMidiByte:
					argByteCount = 4;
					break;
				case OscConst.tagDoubleByte:
				case OscConst.tagLongByte:
				case OscConst.tagTimetagByte:
					argByteCount = 8;
					break;
				case OscConst.tagStringByte:
					argByteCount = StringOscData.EvaluateByteCount( (string) arg );
					break;
				case OscConst.tagBlobByte:
					argByteCount = BlobOscData.EvaluateByteCount( (byte[]) arg );
					break;
			}

			_argInfo.Add( new OscArgInfo( tagByte, argByteCount ) );
			newArgsByteCount += argByteCount;
		}

		// AdaptiveSet data capacity.
		int totalArgsByteCount = _argData.Count + newArgsByteCount;
		if( totalArgsByteCount > _argData.Capacity ) _argData.Capacity = totalArgsByteCount;

		// Store arguments directly as bytes.
		int i = infoStartIndex;
		foreach( object arg in args ) {
			switch( _argInfo[i++].tagByte ) {
				case OscConst.tagFloatByte: new FourByteOscData( (float) arg ).AddTo( _argData ); break;
				case OscConst.tagIntByte: new FourByteOscData( (int) arg ).AddTo( _argData ); break;
				case OscConst.tagCharByte: new FourByteOscData( (char) arg ).AddTo( _argData ); break;
				case OscConst.tagColorByte: new FourByteOscData( (Color32) arg ).AddTo( _argData ); break;
				case OscConst.tagMidiByte: new FourByteOscData( (OscMidiMessage) arg ).AddTo( _argData ); break;
				case OscConst.tagDoubleByte: new EightByteOscData( (double) arg ).AddTo( _argData ); break;
				case OscConst.tagLongByte: new EightByteOscData( (long) arg ).AddTo( _argData ); break;
				case OscConst.tagTimetagByte: new EightByteOscData( (OscTimeTag) arg ).AddTo( _argData ); break;
				case OscConst.tagStringByte: StringOscData.AddTo( (string) arg, _argData ); break;
				case OscConst.tagBlobByte: BlobOscData.AddTo( (byte[]) arg, _argData ); break;
				case OscConst.tagUnsupportedByte:
					// For unsupported tags, we don't attemt to store any data. But we warn the user.
					Debug.LogWarning( "Type " + arg.GetType() + " is not supported.\n" ); // TODO warnings should be optional.
					break;
			}
		}
	}

}