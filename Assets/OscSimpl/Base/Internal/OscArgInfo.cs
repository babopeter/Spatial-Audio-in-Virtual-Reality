/*
	Created by Carl Emil Carlsen.
	Copyright 2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk

	IPEndPoint.Serialize generates garbage. It called by Socket.SendTo (which is used by UdpClient.Send).
	This is a workaround that caches the resulting SocketAddress to avoid successive called to Serialize.
*/

using UnityEngine;
using System.Text;
using System;

namespace OscSimpl
{
	[Serializable]
	struct OscArgInfo
	{
		public byte tagByte;
		public int size;

		public static readonly OscArgInfo nullInfo = new OscArgInfo( OscConst.tagNullByte, 0 );
		public static readonly OscArgInfo impulseInfo = new OscArgInfo( OscConst.tagImpulseByte, 0 );
		public static readonly OscArgInfo boolTrueInfo = new OscArgInfo( OscConst.tagTrueByte, 0 );
		public static readonly OscArgInfo boolFalseInfo = new OscArgInfo( OscConst.tagFalseByte, 0 );
		public static readonly OscArgInfo floatInfo = new OscArgInfo( OscConst.tagFloatByte, 4 );
		public static readonly OscArgInfo intInfo = new OscArgInfo( OscConst.tagIntByte, 4 );
		public static readonly OscArgInfo charInfo = new OscArgInfo( OscConst.tagCharByte, 4 );
		public static readonly OscArgInfo colorInfo = new OscArgInfo( OscConst.tagColorByte, 4 );
		public static readonly OscArgInfo midiInfo = new OscArgInfo( OscConst.tagMidiByte, 4 );
		public static readonly OscArgInfo doubleInfo = new OscArgInfo( OscConst.tagDoubleByte, 8 );
		public static readonly OscArgInfo longInfo = new OscArgInfo( OscConst.tagLongByte, 8 );
		public static readonly OscArgInfo timeTagInfo = new OscArgInfo( OscConst.tagTimetagByte, 8 );
		public static readonly OscArgInfo eightByteBlobInfo = new OscArgInfo( OscConst.tagBlobByte, 4+8 );
		public static readonly OscArgInfo twelveByteBlobInfo = new OscArgInfo( OscConst.tagBlobByte, 4+12 );
		public static readonly OscArgInfo sixteenByteBlobInfo = new OscArgInfo( OscConst.tagBlobByte, 4+16 );
		public static readonly OscArgInfo sixtyfourByteBlobInfo = new OscArgInfo( OscConst.tagBlobByte, 4+64 );
		public static readonly OscArgInfo undefinedInfo = new OscArgInfo( OscConst.tagUnsupportedByte, 0 );

		public OscArgInfo( byte tagByte, int size )
		{
			this.tagByte = tagByte;
			this.size = size;
		}


		public override string ToString()
		{
			return "(" + ((char) tagByte) + ", " + size + ")";
		}
	}

}