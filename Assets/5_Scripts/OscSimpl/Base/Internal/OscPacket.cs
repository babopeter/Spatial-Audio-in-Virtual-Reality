/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using System;

namespace OscSimpl
{
	[Serializable]
	public abstract class OscPacket
	{
		public abstract int Size();

		public abstract bool TryWriteTo( byte[] data, ref int index );

		public static bool TryReadFrom( byte[] data, ref int index, int byteCount, out OscPacket packet )
		{
			if( index >= byteCount ) {
				packet = null;
				return false;
			}

			packet = null;

			byte prefix = data[index];
			if( prefix == OscConst.bundlePrefixByte ){
				OscBundle bundle;
				if( OscBundle.TryReadFrom( data, ref index, byteCount, out bundle ) ) packet = bundle;
			} else if( prefix == OscConst.addressPrefixByte ){
				OscMessage message = null;
				if( OscMessage.TryReadFrom( data, ref index, byteCount, ref message ) ) packet = message;
			}

			return packet != null;
		}
	}
}