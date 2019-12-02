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
	public struct SixtyfourByteOscData
	{
		[FieldOffset( 0 )] Matrix4x4 _matrix4x4;
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
		[FieldOffset( 16 )] byte _b16;
		[FieldOffset( 17 )] byte _b17;
		[FieldOffset( 18 )] byte _b18;
		[FieldOffset( 19 )] byte _b19;
		[FieldOffset( 20 )] byte _b20;
		[FieldOffset( 21 )] byte _b21;
		[FieldOffset( 22 )] byte _b22;
		[FieldOffset( 23 )] byte _b23;
		[FieldOffset( 24 )] byte _b24;
		[FieldOffset( 25 )] byte _b25;
		[FieldOffset( 26 )] byte _b26;
		[FieldOffset( 27 )] byte _b27;
		[FieldOffset( 28 )] byte _b28;
		[FieldOffset( 29 )] byte _b29;
		[FieldOffset( 30 )] byte _b30;
		[FieldOffset( 31 )] byte _b31;
		[FieldOffset( 32 )] byte _b32;
		[FieldOffset( 33 )] byte _b33;
		[FieldOffset( 34 )] byte _b34;
		[FieldOffset( 35 )] byte _b35;
		[FieldOffset( 36 )] byte _b36;
		[FieldOffset( 37 )] byte _b37;
		[FieldOffset( 38 )] byte _b38;
		[FieldOffset( 39 )] byte _b39;
		[FieldOffset( 40 )] byte _b40;
		[FieldOffset( 41 )] byte _b41;
		[FieldOffset( 42 )] byte _b42;
		[FieldOffset( 43 )] byte _b43;
		[FieldOffset( 44 )] byte _b44;
		[FieldOffset( 45 )] byte _b45;
		[FieldOffset( 46 )] byte _b46;
		[FieldOffset( 47 )] byte _b47;
		[FieldOffset( 48 )] byte _b48;
		[FieldOffset( 49 )] byte _b49;
		[FieldOffset( 50 )] byte _b50;
		[FieldOffset( 51 )] byte _b51;
		[FieldOffset( 52 )] byte _b52;
		[FieldOffset( 53 )] byte _b53;
		[FieldOffset( 54 )] byte _b54;
		[FieldOffset( 55 )] byte _b55;
		[FieldOffset( 56 )] byte _b56;
		[FieldOffset( 57 )] byte _b57;
		[FieldOffset( 58 )] byte _b58;
		[FieldOffset( 59 )] byte _b59;
		[FieldOffset( 60 )] byte _b60;
		[FieldOffset( 61 )] byte _b61;
		[FieldOffset( 62 )] byte _b62;
		[FieldOffset( 63 )] byte _b63;

		public const int byteCount = 64;

		public Matrix4x4 matrix4x4Value { get { return _matrix4x4; } }


		public SixtyfourByteOscData
		(
			byte b0, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8, byte b9, byte b10, byte b11, byte b12, byte b13, byte b14, byte b15,
			byte b16, byte b17, byte b18, byte b19, byte b20, byte b21, byte b22, byte b23, byte b24, byte b25, byte b26, byte b27, byte b28, byte b29, byte b30, byte b31,
			byte b32, byte b33, byte b34, byte b35, byte b36, byte b37, byte b38, byte b39, byte b40, byte b41, byte b42, byte b43, byte b44, byte b45, byte b46, byte b47,
			byte b48, byte b49, byte b50, byte b51, byte b52, byte b53, byte b54, byte b55, byte b56, byte b57, byte b58, byte b59, byte b60, byte b61, byte b62, byte b63
		) : this() {
			if( BitConverter.IsLittleEndian ) {
				_b0 = b63;
				_b1 = b62;
				_b2 = b61;
				_b3 = b60;
				_b4 = b59;
				_b5 = b58;
				_b6 = b57;
				_b7 = b56;
				_b8 = b55;
				_b9 = b54;
				_b10 = b53;
				_b11 = b52;
				_b12 = b51;
				_b13 = b50;
				_b14 = b49;
				_b15 = b48;
				_b16 = b47;
				_b17 = b46;
				_b18 = b45;
				_b19 = b44;
				_b20 = b43;
				_b21 = b42;
				_b22 = b41;
				_b23 = b40;
				_b24 = b39;
				_b25 = b38;
				_b26 = b37;
				_b27 = b36;
				_b28 = b35;
				_b29 = b34;
				_b30 = b33;
				_b31 = b32;
				_b32 = b31;
				_b33 = b30;
				_b34 = b29;
				_b35 = b28;
				_b36 = b27;
				_b37 = b26;
				_b38 = b25;
				_b39 = b24;
				_b40 = b23;
				_b41 = b22;
				_b42 = b21;
				_b43 = b20;
				_b44 = b19;
				_b45 = b18;
				_b46 = b17;
				_b47 = b16;
				_b48 = b15;
				_b49 = b14;
				_b50 = b13;
				_b51 = b12;
				_b52 = b11;
				_b53 = b10;
				_b54 = b9;
				_b55 = b8;
				_b56 = b7;
				_b57 = b6;
				_b58 = b5;
				_b59 = b4;
				_b60 = b3;
				_b61 = b2;
				_b62 = b1;
				_b63 = b0;
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
				_b16 = b16;
				_b17 = b17;
				_b18 = b18;
				_b19 = b19;
				_b20 = b20;
				_b21 = b21;
				_b22 = b22;
				_b23 = b23;
				_b24 = b24;
				_b25 = b25;
				_b26 = b26;
				_b27 = b27;
				_b28 = b28;
				_b29 = b29;
				_b30 = b30;
				_b31 = b31;
				_b32 = b32;
				_b33 = b33;
				_b34 = b34;
				_b35 = b35;
				_b36 = b36;
				_b37 = b37;
				_b38 = b38;
				_b39 = b39;
				_b40 = b40;
				_b41 = b41;
				_b42 = b42;
				_b43 = b43;
				_b44 = b44;
				_b45 = b45;
				_b46 = b46;
				_b47 = b47;
				_b48 = b48;
				_b49 = b49;
				_b50 = b50;
				_b51 = b51;
				_b52 = b52;
				_b53 = b53;
				_b54 = b54;
				_b55 = b55;
				_b56 = b56;
				_b57 = b57;
				_b58 = b58;
				_b59 = b59;
				_b60 = b60;
				_b61 = b61;
				_b62 = b62;
				_b63 = b63;
			}
		}


		public SixtyfourByteOscData( Matrix4x4 value ) : this() { _matrix4x4 = value; }


		// TODO When .Net 4.5 becomes available in Unity: Replace IList<T> with IReadOnlyList<T> since we want to pass Array where number and order of list elements is read-only.
		public static bool TryReadFrom( IList<byte> data, ref int index, out SixtyfourByteOscData value )
		{
			if( index + byteCount > data.Count ) {
				value =new SixtyfourByteOscData();
				return false;
			}
			value = new SixtyfourByteOscData(
				data[index++], data[index++], data[index++], data[index++],
				data[index++], data[index++], data[index++], data[index++],
				data[index++], data[index++], data[index++], data[index++],
				data[index++], data[index++], data[index++], data[index++],
				data[index++], data[index++], data[index++], data[index++],
				data[index++], data[index++], data[index++], data[index++],
				data[index++], data[index++], data[index++], data[index++],
				data[index++], data[index++], data[index++], data[index++],
				data[index++], data[index++], data[index++], data[index++],
				data[index++], data[index++], data[index++], data[index++],
				data[index++], data[index++], data[index++], data[index++],
				data[index++], data[index++], data[index++], data[index++],
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

			if( BitConverter.IsLittleEndian ){
				data[index++] = _b63;
				data[index++] = _b62;
				data[index++] = _b61;
				data[index++] = _b60;
				data[index++] = _b59;
				data[index++] = _b58;
				data[index++] = _b57;
				data[index++] = _b56;
				data[index++] = _b55;
				data[index++] = _b54;
				data[index++] = _b53;
				data[index++] = _b52;
				data[index++] = _b51;
				data[index++] = _b50;
				data[index++] = _b49;
				data[index++] = _b48;
				data[index++] = _b47;
				data[index++] = _b46;
				data[index++] = _b45;
				data[index++] = _b44;
				data[index++] = _b43;
				data[index++] = _b42;
				data[index++] = _b41;
				data[index++] = _b40;
				data[index++] = _b39;
				data[index++] = _b38;
				data[index++] = _b37;
				data[index++] = _b36;
				data[index++] = _b35;
				data[index++] = _b34;
				data[index++] = _b33;
				data[index++] = _b32;
				data[index++] = _b31;
				data[index++] = _b30;
				data[index++] = _b29;
				data[index++] = _b28;
				data[index++] = _b27;
				data[index++] = _b26;
				data[index++] = _b25;
				data[index++] = _b24;
				data[index++] = _b23;
				data[index++] = _b22;
				data[index++] = _b21;
				data[index++] = _b20;
				data[index++] = _b19;
				data[index++] = _b18;
				data[index++] = _b17;
				data[index++] = _b16;
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
				data[index++] = _b16;
				data[index++] = _b17;
				data[index++] = _b18;
				data[index++] = _b19;
				data[index++] = _b20;
				data[index++] = _b21;
				data[index++] = _b22;
				data[index++] = _b23;
				data[index++] = _b24;
				data[index++] = _b25;
				data[index++] = _b26;
				data[index++] = _b27;
				data[index++] = _b28;
				data[index++] = _b29;
				data[index++] = _b30;
				data[index++] = _b31;
				data[index++] = _b32;
				data[index++] = _b33;
				data[index++] = _b34;
				data[index++] = _b35;
				data[index++] = _b36;
				data[index++] = _b37;
				data[index++] = _b38;
				data[index++] = _b39;
				data[index++] = _b40;
				data[index++] = _b41;
				data[index++] = _b42;
				data[index++] = _b43;
				data[index++] = _b44;
				data[index++] = _b45;
				data[index++] = _b46;
				data[index++] = _b47;
				data[index++] = _b48;
				data[index++] = _b49;
				data[index++] = _b50;
				data[index++] = _b51;
				data[index++] = _b52;
				data[index++] = _b53;
				data[index++] = _b54;
				data[index++] = _b55;
				data[index++] = _b56;
				data[index++] = _b57;
				data[index++] = _b58;
				data[index++] = _b59;
				data[index++] = _b60;
				data[index++] = _b61;
				data[index++] = _b62;
				data[index++] = _b63;
			}

			return true;
		}


		public void AddTo( List<byte> data )
		{
			if( data.Capacity < data.Count + byteCount ) data.Capacity = data.Count + byteCount;

			if( BitConverter.IsLittleEndian ){
				data.Add( _b63 );
				data.Add( _b62 );
				data.Add( _b61 );
				data.Add( _b60 );
				data.Add( _b59 );
				data.Add( _b58 );
				data.Add( _b57 );
				data.Add( _b56 );
				data.Add( _b55 );
				data.Add( _b54 );
				data.Add( _b53 );
				data.Add( _b52 );
				data.Add( _b51 );
				data.Add( _b50 );
				data.Add( _b49 );
				data.Add( _b48 );
				data.Add( _b47 );
				data.Add( _b46 );
				data.Add( _b45 );
				data.Add( _b44 );
				data.Add( _b43 );
				data.Add( _b42 );
				data.Add( _b41 );
				data.Add( _b40 );
				data.Add( _b39 );
				data.Add( _b38 );
				data.Add( _b37 );
				data.Add( _b36 );
				data.Add( _b35 );
				data.Add( _b34 );
				data.Add( _b33 );
				data.Add( _b32 );
				data.Add( _b31 );
				data.Add( _b30 );
				data.Add( _b29 );
				data.Add( _b28 );
				data.Add( _b27 );
				data.Add( _b26 );
				data.Add( _b25 );
				data.Add( _b24 );
				data.Add( _b23 );
				data.Add( _b22 );
				data.Add( _b21 );
				data.Add( _b20 );
				data.Add( _b19 );
				data.Add( _b18 );
				data.Add( _b17 );
				data.Add( _b16 );
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
				data.Add( _b16 );
				data.Add( _b17 );
				data.Add( _b18 );
				data.Add( _b19 );
				data.Add( _b20 );
				data.Add( _b21 );
				data.Add( _b22 );
				data.Add( _b23 );
				data.Add( _b24 );
				data.Add( _b25 );
				data.Add( _b26 );
				data.Add( _b27 );
				data.Add( _b28 );
				data.Add( _b29 );
				data.Add( _b30 );
				data.Add( _b31 );
				data.Add( _b32 );
				data.Add( _b33 );
				data.Add( _b34 );
				data.Add( _b35 );
				data.Add( _b36 );
				data.Add( _b37 );
				data.Add( _b38 );
				data.Add( _b39 );
				data.Add( _b40 );
				data.Add( _b41 );
				data.Add( _b42 );
				data.Add( _b43 );
				data.Add( _b44 );
				data.Add( _b45 );
				data.Add( _b46 );
				data.Add( _b47 );
				data.Add( _b48 );
				data.Add( _b49 );
				data.Add( _b50 );
				data.Add( _b51 );
				data.Add( _b52 );
				data.Add( _b53 );
				data.Add( _b54 );
				data.Add( _b55 );
				data.Add( _b56 );
				data.Add( _b57 );
				data.Add( _b58 );
				data.Add( _b59 );
				data.Add( _b60 );
				data.Add( _b61 );
				data.Add( _b62 );
				data.Add( _b63 );
			}
		}
	}
}