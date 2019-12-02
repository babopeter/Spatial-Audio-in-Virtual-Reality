/*
	Created by Carl Emil Carlsen.
	Copyright 2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk

	In this class, we use StructLayout Explicit to convert from and to bytes,
	without generating garbage.
*/


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace OscSimpl
{
	[Serializable, StructLayout( LayoutKind.Explicit )]
	public struct SixteenByteOscData
	{
		[FieldOffset( 0 )] Vector4 _vector4Value;
		[FieldOffset( 0 )] Quaternion _quaternionValue;
		[FieldOffset( 0 )] byte _b0;
		[FieldOffset( 1 )] byte _b1;
		[FieldOffset( 2 )] byte _b2;
		[FieldOffset( 3 )] byte _b3;
		[FieldOffset( 4 )] byte _b4;
		[FieldOffset( 5 )] byte _b5;
		[FieldOffset( 6 )] byte _b6;
		[FieldOffset( 7 )] byte _b7;
		[FieldOffset( 8 )] byte _b8;
		[FieldOffset( 9 )] byte _b9;
		[FieldOffset( 10 )] byte _b10;
		[FieldOffset( 11 )] byte _b11;
		[FieldOffset( 12 )] byte _b12;
		[FieldOffset( 13 )] byte _b13;
		[FieldOffset( 14 )] byte _b14;
		[FieldOffset( 15 )] byte _b15;

		public const int byteCount = 16;

		public Vector4 vector4Value { get { return _vector4Value; } }
		public Quaternion quaternionValue { get { return _quaternionValue; } }


		public SixteenByteOscData
		(
			byte b0, byte b1, byte b2, byte b3, 
			byte b4, byte b5, byte b6, byte b7, 
			byte b8, byte b9, byte b10, byte b11,
			byte b12, byte b13, byte b14, byte b15
		) : this() {
			if( BitConverter.IsLittleEndian ) {
				_b0 = b15;
				_b1 = b14;
				_b2 = b13;
				_b3 = b12;
				_b4 = b11;
				_b5 = b10;
				_b6 = b9;
				_b7 = b8;
				_b8 = b7;
				_b9 = b6;
				_b10 = b5;
				_b11 = b4;
				_b12 = b3;
				_b13 = b2;
				_b14 = b1;
				_b15 = b0;
			} else {
				_b0 = b0;
				_b1 = b1;
				_b2 = b2;
				_b3 = b3;
				_b4 = b4;
				_b5 = b5;
				_b6 = b6;
				_b7 = b7;
				_b8 = b8;
				_b9 = b9;
				_b10 = b10;
				_b11 = b11;
				_b12 = b12;
				_b13 = b13;
				_b14 = b14;
				_b15 = b15;
			}
		}


		public SixteenByteOscData( Vector4 value ) : this() { _vector4Value = value; }
		public SixteenByteOscData( Quaternion value ) : this() { _quaternionValue = value; }


		// TODO When .Net 4.5 becomes available in Unity: Replace IList<T> with IReadOnlyList<T> since we want to pass Array where number and order of list elements is read-only.
		public static bool TryReadFrom( IList<byte> data, ref int index, out SixteenByteOscData value )
		{
			if( index + byteCount > data.Count ) {
				value =new SixteenByteOscData();
				return false;
			}
			value = new SixteenByteOscData(
				data[index++], data[index++], data[index++], data[index++], 
				data[index++], data[index++], data[index++], data[index++],
				data[index++], data[index++], data[index++], data[index++],
				data[index++], data[index++], data[index++], data[index++]
			);
			return true;
		}


		// TODO When .Net 4.5 becomes available in Unity: Replace IList<T> with IReadOnlyList<T> since we want to pass Array where number and order of list elements is read-only.
		public bool TryWriteTo( IList<byte> data, ref int index )
		{
			if( index + byteCount > data.Count ) return false;

			if( BitConverter.IsLittleEndian ) {
				data[index++] = _b15;
				data[index++] = _b14;
				data[index++] = _b13;
				data[index++] = _b12;
				data[index++] = _b11;
				data[index++] = _b10;
				data[index++] = _b9;
				data[index++] = _b8;
				data[index++] = _b7;
				data[index++] = _b6;
				data[index++] = _b5;
				data[index++] = _b4;
				data[index++] = _b3;
				data[index++] = _b2;
				data[index++] = _b1;
				data[index++] = _b0;
			} else {
				data[index++] = _b0;
				data[index++] = _b1;
				data[index++] = _b2;
				data[index++] = _b3;
				data[index++] = _b4;
				data[index++] = _b5;
				data[index++] = _b6;
				data[index++] = _b7;
				data[index++] = _b8;
				data[index++] = _b9;
				data[index++] = _b10;
				data[index++] = _b11;
				data[index++] = _b12;
				data[index++] = _b13;
				data[index++] = _b14;
				data[index++] = _b15;
			}

			return true;
		}


		public void AddTo( List<byte> data )
		{
			if( data.Capacity < data.Count + byteCount ) data.Capacity = data.Count + byteCount;

			if( BitConverter.IsLittleEndian ){
				data.Add( _b15 );
				data.Add( _b14 );
				data.Add( _b13 );
				data.Add( _b12 );
				data.Add( _b11 );
				data.Add( _b10 );
				data.Add( _b9 );
				data.Add( _b8 );
				data.Add( _b7 );
				data.Add( _b6 );
				data.Add( _b5 );
				data.Add( _b4 );
				data.Add( _b3 );
				data.Add( _b2 );
				data.Add( _b1 );
				data.Add( _b0 );
			} else {
				data.Add( _b0 );
				data.Add( _b1 );
				data.Add( _b2 );
				data.Add( _b3 );
				data.Add( _b4 );
				data.Add( _b5 );
				data.Add( _b6 );
				data.Add( _b7 );
				data.Add( _b8 );
				data.Add( _b9 );
				data.Add( _b10 );
				data.Add( _b11 );
				data.Add( _b12 );
				data.Add( _b13 );
				data.Add( _b14 );
				data.Add( _b15 );
			}
		}
	}
}