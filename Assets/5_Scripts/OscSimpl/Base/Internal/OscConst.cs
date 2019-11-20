/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2019 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using System.Net;

namespace OscSimpl
{
	public static class OscConst
	{
		const string bundlePrefix = "#bundle";
		public const byte bundlePrefixByte = (byte) '#';
		public const char addressPrefix = '/';

		public const int bundleHeaderSize = 16; // Prefix + time tag.

		public static readonly byte[] bundlePrefixBytes; // Including trailing zero. Eight bytes in all.
		public const byte addressPrefixByte = 	(byte) addressPrefix;
		public const byte tagPrefixByte = 		(byte) ',';
		public const byte tagUnsupportedByte = 	(byte) '?';

		// OSC Type Tags (15).
		public const byte tagNullByte = 		(byte) 'N'; // Zero bytes.
		public const byte tagImpulseByte = 		(byte) 'I';
		public const byte tagTrueByte = 		(byte) 'T';
		public const byte tagFalseByte = 		(byte) 'F';
		public const byte tagFloatByte = 		(byte) 'f'; // Four bytes.
		public const byte tagIntByte = 			(byte) 'i';
		public const byte tagCharByte = 		(byte) 'c';
		public const byte tagColorByte = 		(byte) 'r';
		public const byte tagMidiByte = 		(byte) 'm'; 
		public const byte tagDoubleByte = 		(byte) 'd'; // Eight bytes.
		public const byte tagLongByte = 		(byte) 'h';
		public const byte tagTimetagByte = 		(byte) 't';
		public const byte tagStringByte = 		(byte) 's'; // Variable byte count.
		public const byte tagSymbolByte = 		(byte) 'S';
		public const byte tagBlobByte = 		(byte) 'b';
		
		// Networking
		public static readonly string hostName = Dns.GetHostName();
		public const string unicastAddressDefault = "192.168.1.1";
		public const string multicastAddressDefault = "224.1.1.1";
		public static readonly string loopbackAddress = IPAddress.Loopback.ToString();
		public const float localIpUpdateInterval = 5; // Sec
		public static readonly string[] loopbackAddressArray = { IPAddress.Loopback.ToString() }; // Deprecated

		// ASCII.
		public const byte asciiUnknownByte = (byte) '?'; // 63
		public const byte asciiMaxByte = 127; // Exclusive

		// By Andrew Cheong http://stackoverflow.com/questions/13145397/regex-for-multicast-ip-address
		public const string multicastAddressPattern = "2(?:2[4-9]|3\\d)(?:\\.(?:25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d?|0)){3}";

		public const int timeToLive = 255;

		// https://msdn.microsoft.com/en-us/library/tst0kwb1(v=vs.110).aspx
		public const int portMin = 1;
		public const int portMax = 65535;

		// Udp buffer size.
		public const int udpBufferSizeDefault = 8192;	// // Tests on MacOs 10.14 shows that messages larger than 8192 send locally are truncated (Dec 16th 2018).
		public const int udpBufferSizeMin = 512;		// http://stackoverflow.com/questions/1098897/what-is-the-largest-safe-udp-packet-size-on-the-internet
		public const int udpBufferSizeMax = 65507;		// http://stackoverflow.com/questions/1098897/what-is-the-largest-safe-udp-packet-size-on-the-internet


		static OscConst()
		{
			bundlePrefixBytes = new byte[ StringOscData.EvaluateByteCount( bundlePrefix ) ];
			int index = 0;
			StringOscData.TryWriteTo( bundlePrefix, bundlePrefixBytes, ref index );
		}
	}
}