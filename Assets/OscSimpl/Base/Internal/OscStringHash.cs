/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2019 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using System;
using UnityEngine;

namespace OscSimpl
{
	/// <summary>
	/// OscStringHash is used for creating (almost) unique indexes for OSC address.
	/// strings. The reason we need it is:
	/// 
	/// 1) To avoid generating garbage when reading a message address string from 
	/// bytes. If we can grab an identifier from bytes and use it find a message 
	/// from the pool that is likely to hold the same address string, then it makes
	/// sense to compare while reading and omit creating a new string if they match,
	/// saving ton of garbage.
	///  
	/// 2) To avoid generating garbage when reusing messages from the pool. If we can 
	/// find messages in the pool with the same byte count we avoid resizing the 
	/// capacity of the lists that hold the arguments, saving garbage as well.
	/// Messages with the same address are likely to contain the same sort of data.
	///
	/// 3) To speed up pool lookups. Using an interger to look up a message from the 
	/// pool (using Dictionary) is faster than using a string.
	/// https://stackoverflow.com/questions/1539476/does-the-length-of-key-affect-dictionary-performance
	/// 
	/// 4) To speed up OscMapping lookups. Though, because the value of Lossy Hash 
	/// is not guaranteed to be unique for all addresses, must do an addtional lookup
	/// on the string. The lookup table is therefore stored in a dictionary of type
	/// Dictionary<int,Dictionary<string<OscMapping>>>.
	///
	/// OscStringHash packs and unpacks an signed 32 bit integer that contains the 
	/// following, in order:
	///
	///		Data               | Bits  | Bit index | Max Value (inclussive)
	///		------------------ | ----- | --------- | ---------
	///		Address Length     | 16    | 0         | 65534
	///		Address Sum        | 16    | 8         | 65534
	///
	/// Messages with address "/a/b" and "/b/a" will produce the same hash and a new 
	/// string will in that case be generated when receving bytes.
	/// </summary>
	public static class OscStringHash
	{
		const int lossyStringHashLengthMask = 65535; //  1111 1111 1111 1111 0000 0000 0000 0000
		const int lossyStringHashSumMask = -65536; //  0000 0000 0000 0000 1111 1111 1111 1111
		const int lossyStringHashSumButIndex = 16;


		public static int Pack( string text )
		{
			// Validate.
			if( text.Length < 1 || (byte) text[0] != OscConst.addressPrefixByte ) return 0;

			// Compute.
			int hash = text.Length-1;
			ushort sum = 0;
			for( int i = 1; i < text.Length; i++ ) sum += (byte) text[i]; // Skipping address prefix.
			hash = (hash & ~(0xf << lossyStringHashSumButIndex)) | (sum << lossyStringHashSumButIndex);
			//Debug.Log( "Pack string: " + hash );
			return hash;
		}


		public static int Pack( byte[] data, int index )
		{
			// Validate address and skip address prefix.
			if( data.Length < 1 || data[index++] != OscConst.addressPrefixByte ) return 0;

			// Read address length and sum.
			int hash = 0;
			ushort sum = 0;
			while( index < data.Length ) {
				byte b = data[index++];
				if( b == 0 ) break;
				hash++;
				sum += b;
			}
			hash = ( hash & ~(0xf << lossyStringHashSumButIndex)) | (sum << lossyStringHashSumButIndex);
			//Debug.Log( "Pack data: " + hash );
			return hash;
		}


		public static void Unpack( int hash, out ushort stringLength, out ushort stringSum )
		{
			stringLength = (ushort) (hash & lossyStringHashLengthMask);
			stringSum = (ushort) ((hash & lossyStringHashSumMask) >> lossyStringHashSumButIndex);
		}
	}
}