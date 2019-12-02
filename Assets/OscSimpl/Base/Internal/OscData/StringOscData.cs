/*
	Created by Carl Emil Carlsen.
	Copyright 2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk

	A sequence of non-null ASCII characters, followed by a null, followed by 0-3 additional null characters to make the total number of bytes a multiple of 4.
*/


// TODO When .NET 4.0 becomes available use StringBuilder.Clear() instead of StringBuilder.Length = 0.
// https://stackoverflow.com/questions/1998080/how-to-reuse-stringbuilder-obj

using System.Text;
using System.Collections.Generic;

namespace OscSimpl
{
	public static class StringOscData
	{
		static byte[] _cache;
		static StringBuilder _sb = new StringBuilder();


		public static int EvaluateByteCount( string text )
		{
			return text.Length + 4 - (text.Length % 4);   // Single byte ASCII, multiple of four bytes with at least one trailing null.
		}


		public static int EvaluateByteCount( byte[] data, int index )
		{
			int count = 0;
			for( int i = index; i < data.Length && data[i] != 0; i++ ) {
				count++;
				//UnityEngine.Debug.Log( (char) data[i] );
			}
			count += 4 - (count % 4);
			return count;
		}


		// TODO When .Net 4.5 becomes available in Unity: Use IReadOnlyList<T> instead of having two identical methods here.


		public static bool TryReadFrom( IList<byte> data, ref int index, ref string text )
		{
			// Count and validate.
			int textByteCount = 0;
			for( int i = index; i < data.Count && data[i] != 0; i++ ) textByteCount++;
			int trailingNullCount = 4 - (textByteCount % 4); // Including a minimum of one trailing null.
			if( index + textByteCount + trailingNullCount > data.Count ) {
				text = string.Empty;
				return false;
			}

			// We use a StringBuilder because ASCII.GetString don't accept Lists.
			_sb.Length = 0;
			if( _sb.Capacity < textByteCount ) _sb.Capacity = textByteCount;
			bool change = !(!string.IsNullOrEmpty( text ) && text.Length == textByteCount );
			for( int i = 0; i < textByteCount; i++ ){
				byte b = data[index++];
				if( b > OscConst.asciiMaxByte ) b = OscConst.asciiUnknownByte; // Mimic Encoding.ASCII.GetString behaviour, but without garbage.
				char c = (char) b;
				if( !change && c != text[i] ) change = true;
				_sb.Append( c );
			}
			if( change ) text = _sb.ToString(); // Only create string if it changed from the original.
			index += trailingNullCount;

			return true;
		}



		public static bool TryWriteTo( string text, byte[] data, ref int index )
		{
			// Count and validate.
			int textByteCount = text.Length; // ASCII takes one byte per charater.
			int trailingNullCount = 4 - (textByteCount % 4); // Including a minimum of one trailing null.
			if( index + textByteCount + trailingNullCount > data.Length ) return false;

			// Write.
			Encoding.ASCII.GetBytes( text, 0, text.Length, data, index );
			index += textByteCount;
			for( int i = 0; i < trailingNullCount; i++ ) data[index++] = 0;

			return true;
		}


		public static bool TryWriteTo( string text, List<byte> data, ref int index )
		{
			// Count and validate.
			int textByteCount = text.Length; // ASCII takes one byte per charater.
			int trailingNullCount = 4 - (textByteCount % 4);
			if( index + textByteCount + trailingNullCount > data.Count ) return false;

			// Encode to cache.
			if( _cache == null || _cache.Length < textByteCount ) _cache = new byte[textByteCount];
			Encoding.ASCII.GetBytes( text, 0, textByteCount, _cache, 0 );

			// Write.
			for( int i = 0; i < textByteCount; i++ ) data[index++] = _cache[i];
			for( int i = 0; i < trailingNullCount; i++ ) data[index++] = 0;

			return true;
		}


		public static int AddTo( string text, List<byte> data )
		{
			// Encode to cache.
			int textByteCount = text.Length;
			if( _cache == null || _cache.Length < textByteCount ) _cache = new byte[textByteCount];
			Encoding.ASCII.GetBytes( text, 0, textByteCount, _cache, 0 );

			// Add to list with trailing nulls.
			for( int i = 0; i < textByteCount; i++ ) data.Add( _cache[i] );
			int trailingNullCount = 4 - (textByteCount % 4);
			for( int i = 0; i < trailingNullCount; i++ ) data.Add( 0 );
			return textByteCount + trailingNullCount;
		}
	}
}