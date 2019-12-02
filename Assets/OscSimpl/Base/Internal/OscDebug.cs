/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk

	IPEndPoint.Serialize generates garbage. It called by Socket.SendTo (which is used by UdpClient.Send).
	This is a workaround that caches the resulting SocketAddress to avoid successive called to Serialize.
*/

using System;
using System.Text;
using System.Collections;

namespace OscSimpl
{
	public static class OscDebug
	{
		static StringBuilder _sb = new StringBuilder();

		const string failedWritingBytesText = "Failed to write bytes.";
		const string failedReadingBytesText = "Failed to read bytes.";

		const string defaultPrefix = "<b>[OSC]</b> ";


		public static StringBuilder BuildText( object o = null )
		{
			_sb.Clear();
			if( o == null ) {
				_sb.Append( defaultPrefix );
			} else {
				_sb.Append( "<b>[" ); _sb.Append( o.GetType().Name ); _sb.Append( "]</b> " );
			}
			return _sb;
		}

		
		public static string FailedWritingBytesWarning( object o )
		{
			StartBuilding( o );
			_sb.Append( failedWritingBytesText );
			_sb.AppendLine();
			return _sb.ToString();
		}


		public static string FailedReadingBytesWarning( object o )
		{
			StartBuilding( o );
			_sb.Append( failedReadingBytesText );
			if( o is OscMessage && !string.IsNullOrEmpty( (o as OscMessage ).address ) ){
				_sb.Append( " Address " );
				_sb.Append( (o as OscMessage).address );
				_sb.Append( "." );
			}
			_sb.AppendLine();
			return _sb.ToString();
		}


		public static string Bits( int value )
		{
			BitArray bits = new BitArray( BitConverter.GetBytes( value ) );
			int i = 0;
			_sb.Clear();
			foreach( bool b in bits ) {
				if( i++ % 4 == 0 ) _sb.Append( ' ' );
				_sb.Append( b ? 1 : 0 );
			}
			return _sb.ToString();
		}


		static void StartBuilding( object o )
		{
			_sb.Clear();
			_sb.Append( "<b>[" ); _sb.Append( o.GetType().Name ); _sb.Append( "]</b> " );
		}
	}
}