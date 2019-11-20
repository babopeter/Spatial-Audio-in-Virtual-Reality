/*
	Created by Carl Emil Carlsen.
	Copyright 2019 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk

	OSC 1.0
		"4 byte MIDI message. Bytes from MSB to LSB are: port id, status byte, data1, data2"
		http://opensoundcontrol.org/spec-1_0
			
	Status byte and data meaning
		http://www.opensound.com/pguide/midi/midi5.html
*/

using System;
using System.Runtime.InteropServices;

/// <summary>
/// Osc MIDI Message.
/// </summary>
[Serializable, StructLayout( LayoutKind.Explicit )]
public struct OscMidiMessage
{
	[FieldOffset( 0 )] byte _port;
	[FieldOffset( 1 )] byte _status; // Contains 4 bit status code and 4 bit channel numnber (if not system message).
	[FieldOffset( 2 )] byte _data1;
	[FieldOffset( 3 )] byte _data2;

	/// <summary>
	/// Gets or sets the port id number.
	/// </summary>
	public byte port
	{
		get { return _port; }
		set { _port = value; }
	}


	/// <summary>
	/// Gets or sets the raw status byte.
	/// Use methods GetTypeAndChannel and SetTypeAndChannel for your convenience.
	/// </summary>
	public byte status
	{
		get { return _status; }
		set { _status = value; }
	}


	/// <summary>
	/// Gets or sets the first data part of the message.
	/// Depending on the the status byte (OscMidiMessage.Type) this
	/// can contain key, channel number, pressure and other data.
	/// </summary>
	public byte data1
	{
		get { return _data1; }
		set { _data1 = value; }
	}


	/// <summary>
	/// Gets or sets the second data part of the message.
	/// Depending on the the status byte (OscMidiMessage.Type) this
	/// can contain velocity, pressure and other data.
	/// </summary>
	public byte data2
	{
		get { return _data2; }
		set { _data2 = value; }
	}


	/// <summary>
	/// Create a Osc MIDI Messsages and set raw byte values.
	/// </summary>
	public OscMidiMessage( byte port, byte status, byte data1, byte data2 )
	{
		_port = port;
		_status = status;
		_data1 = data1;
		_data2 = data2;
	}


	/// <summary>
	/// Creates a note off message.
	/// </summary>
	public static OscMidiMessage NoteOff( Channel channel, byte key )
	{
		OscMidiMessage message = new OscMidiMessage();
		message.SetTypeAndChannel( Type.NoteOff, channel );
		message.data1 = key;
		return message;
	}


	/// <summary>
	/// Creates a note on message.
	/// </summary>
	public static OscMidiMessage NoteOn( Channel channel, byte key, byte velocity )
	{
		OscMidiMessage message = new OscMidiMessage();
		message.SetTypeAndChannel( Type.NoteOn, channel );
		message.data1 = key;
		message.data2 = velocity;
		return message;
	}


	/// <summary>
	/// Creates a control change (CC) message.
	/// </summary>
	public static OscMidiMessage ControlChange( Channel channel, byte number, byte value )
	{
		OscMidiMessage message = new OscMidiMessage();
		message.SetTypeAndChannel( Type.ControlChange, channel );
		message.data1 = number;
		message.data2 = value;
		return message;
	}


	/// <summary>
	/// Creates a program change message.
	/// </summary>
	public static OscMidiMessage ProgramChange( Channel channel, byte number )
	{
		OscMidiMessage message = new OscMidiMessage();
		message.SetTypeAndChannel( Type.ProgramChange, channel );
		message.data1 = number;
		return message;
	}



	/// <summary>
	/// Gets the message type and channel from the status byte.
	/// </summary>
	public void GetTypeAndChannel( out Type type, out Channel channel )
	{
		if( _status < 128 ){
			type = Type.Unknown;
			channel = Channel.NotApplicable;
			return;
		}
		if( _status > 239 ) {
			type = Type.System;
			channel = Channel.NotApplicable;
			return;
		}

		switch( _status )
		{
			case 128: type = Type.NoteOff; channel = Channel.Ch1; break;
			case 129: type = Type.NoteOff; channel = Channel.Ch2; break;
			case 130: type = Type.NoteOff; channel = Channel.Ch3; break;
			case 131: type = Type.NoteOff; channel = Channel.Ch4; break;
			case 132: type = Type.NoteOff; channel = Channel.Ch5; break;
			case 133: type = Type.NoteOff; channel = Channel.Ch6; break;
			case 134: type = Type.NoteOff; channel = Channel.Ch7; break;
			case 135: type = Type.NoteOff; channel = Channel.Ch8; break;
			case 136: type = Type.NoteOff; channel = Channel.Ch9; break;
			case 137: type = Type.NoteOff; channel = Channel.Ch10; break;
			case 138: type = Type.NoteOff; channel = Channel.Ch11; break;
			case 139: type = Type.NoteOff; channel = Channel.Ch12; break;
			case 140: type = Type.NoteOff; channel = Channel.Ch13; break;
			case 141: type = Type.NoteOff; channel = Channel.Ch14; break;
			case 142: type = Type.NoteOff; channel = Channel.Ch15; break;
			case 143: type = Type.NoteOff; channel = Channel.Ch16; break;

			case 144: type = Type.NoteOn; channel = Channel.Ch1; break;
			case 145: type = Type.NoteOn; channel = Channel.Ch2; break;
			case 146: type = Type.NoteOn; channel = Channel.Ch3; break;
			case 147: type = Type.NoteOn; channel = Channel.Ch4; break;
			case 148: type = Type.NoteOn; channel = Channel.Ch5; break;
			case 149: type = Type.NoteOn; channel = Channel.Ch6; break;
			case 150: type = Type.NoteOn; channel = Channel.Ch7; break;
			case 151: type = Type.NoteOn; channel = Channel.Ch8; break;
			case 152: type = Type.NoteOn; channel = Channel.Ch9; break;
			case 153: type = Type.NoteOn; channel = Channel.Ch10; break;
			case 154: type = Type.NoteOn; channel = Channel.Ch11; break;
			case 155: type = Type.NoteOn; channel = Channel.Ch12; break;
			case 156: type = Type.NoteOn; channel = Channel.Ch13; break;
			case 157: type = Type.NoteOn; channel = Channel.Ch14; break;
			case 158: type = Type.NoteOn; channel = Channel.Ch15; break;
			case 159: type = Type.NoteOn; channel = Channel.Ch16; break;

			case 160: type = Type.PolyAfterTouch; channel = Channel.Ch1; break;
			case 161: type = Type.PolyAfterTouch; channel = Channel.Ch2; break;
			case 162: type = Type.PolyAfterTouch; channel = Channel.Ch3; break;
			case 163: type = Type.PolyAfterTouch; channel = Channel.Ch4; break;
			case 164: type = Type.PolyAfterTouch; channel = Channel.Ch5; break;
			case 165: type = Type.PolyAfterTouch; channel = Channel.Ch6; break;
			case 166: type = Type.PolyAfterTouch; channel = Channel.Ch7; break;
			case 167: type = Type.PolyAfterTouch; channel = Channel.Ch8; break;
			case 168: type = Type.PolyAfterTouch; channel = Channel.Ch9; break;
			case 169: type = Type.PolyAfterTouch; channel = Channel.Ch10; break;
			case 170: type = Type.PolyAfterTouch; channel = Channel.Ch11; break;
			case 171: type = Type.PolyAfterTouch; channel = Channel.Ch12; break;
			case 172: type = Type.PolyAfterTouch; channel = Channel.Ch13; break;
			case 173: type = Type.PolyAfterTouch; channel = Channel.Ch14; break;
			case 174: type = Type.PolyAfterTouch; channel = Channel.Ch15; break;
			case 175: type = Type.PolyAfterTouch; channel = Channel.Ch16; break;

			case 176: type = Type.ControlChange; channel = Channel.Ch1; break;
			case 177: type = Type.ControlChange; channel = Channel.Ch2; break;
			case 178: type = Type.ControlChange; channel = Channel.Ch3; break;
			case 179: type = Type.ControlChange; channel = Channel.Ch4; break;
			case 180: type = Type.ControlChange; channel = Channel.Ch5; break;
			case 181: type = Type.ControlChange; channel = Channel.Ch6; break;
			case 182: type = Type.ControlChange; channel = Channel.Ch7; break;
			case 183: type = Type.ControlChange; channel = Channel.Ch8; break;
			case 184: type = Type.ControlChange; channel = Channel.Ch9; break;
			case 185: type = Type.ControlChange; channel = Channel.Ch10; break;
			case 186: type = Type.ControlChange; channel = Channel.Ch11; break;
			case 187: type = Type.ControlChange; channel = Channel.Ch12; break;
			case 188: type = Type.ControlChange; channel = Channel.Ch13; break;
			case 189: type = Type.ControlChange; channel = Channel.Ch14; break;
			case 190: type = Type.ControlChange; channel = Channel.Ch15; break;
			case 191: type = Type.ControlChange; channel = Channel.Ch16; break;

			case 192: type = Type.ProgramChange; channel = Channel.Ch1; break;
			case 193: type = Type.ProgramChange; channel = Channel.Ch2; break;
			case 194: type = Type.ProgramChange; channel = Channel.Ch3; break;
			case 195: type = Type.ProgramChange; channel = Channel.Ch4; break;
			case 196: type = Type.ProgramChange; channel = Channel.Ch5; break;
			case 197: type = Type.ProgramChange; channel = Channel.Ch6; break;
			case 198: type = Type.ProgramChange; channel = Channel.Ch7; break;
			case 199: type = Type.ProgramChange; channel = Channel.Ch8; break;
			case 200: type = Type.ProgramChange; channel = Channel.Ch9; break;
			case 201: type = Type.ProgramChange; channel = Channel.Ch10; break;
			case 202: type = Type.ProgramChange; channel = Channel.Ch11; break;
			case 203: type = Type.ProgramChange; channel = Channel.Ch12; break;
			case 204: type = Type.ProgramChange; channel = Channel.Ch13; break;
			case 205: type = Type.ProgramChange; channel = Channel.Ch14; break;
			case 206: type = Type.ProgramChange; channel = Channel.Ch15; break;
			case 207: type = Type.ProgramChange; channel = Channel.Ch16; break;

			case 208: type = Type.ChannelAfterTouch; channel = Channel.Ch1; break;
			case 209: type = Type.ChannelAfterTouch; channel = Channel.Ch2; break;
			case 210: type = Type.ChannelAfterTouch; channel = Channel.Ch3; break;
			case 211: type = Type.ChannelAfterTouch; channel = Channel.Ch4; break;
			case 212: type = Type.ChannelAfterTouch; channel = Channel.Ch5; break;
			case 213: type = Type.ChannelAfterTouch; channel = Channel.Ch6; break;
			case 214: type = Type.ChannelAfterTouch; channel = Channel.Ch7; break;
			case 215: type = Type.ChannelAfterTouch; channel = Channel.Ch8; break;
			case 216: type = Type.ChannelAfterTouch; channel = Channel.Ch9; break;
			case 217: type = Type.ChannelAfterTouch; channel = Channel.Ch10; break;
			case 218: type = Type.ChannelAfterTouch; channel = Channel.Ch11; break;
			case 219: type = Type.ChannelAfterTouch; channel = Channel.Ch12; break;
			case 220: type = Type.ChannelAfterTouch; channel = Channel.Ch13; break;
			case 221: type = Type.ChannelAfterTouch; channel = Channel.Ch14; break;
			case 222: type = Type.ChannelAfterTouch; channel = Channel.Ch15; break;
			case 223: type = Type.ChannelAfterTouch; channel = Channel.Ch16; break;

			case 224: type = Type.PitchWheel; channel = Channel.Ch1; break;
			case 225: type = Type.PitchWheel; channel = Channel.Ch2; break;
			case 226: type = Type.PitchWheel; channel = Channel.Ch3; break;
			case 227: type = Type.PitchWheel; channel = Channel.Ch4; break;
			case 228: type = Type.PitchWheel; channel = Channel.Ch5; break;
			case 229: type = Type.PitchWheel; channel = Channel.Ch6; break;
			case 230: type = Type.PitchWheel; channel = Channel.Ch7; break;
			case 231: type = Type.PitchWheel; channel = Channel.Ch8; break;
			case 232: type = Type.PitchWheel; channel = Channel.Ch9; break;
			case 233: type = Type.PitchWheel; channel = Channel.Ch10; break;
			case 234: type = Type.PitchWheel; channel = Channel.Ch11; break;
			case 235: type = Type.PitchWheel; channel = Channel.Ch12; break;
			case 236: type = Type.PitchWheel; channel = Channel.Ch13; break;
			case 237: type = Type.PitchWheel; channel = Channel.Ch14; break;
			case 238: type = Type.PitchWheel; channel = Channel.Ch15; break;
			case 239: type = Type.PitchWheel; channel = Channel.Ch16; break;

			default:	type = Type.Unknown;	channel = Channel.NotApplicable; break;
		}
	}


	/// <summary>
	/// Sets the message type and channel, overwriting the status byte.
	/// </summary>
	public void SetTypeAndChannel( Type type, Channel channel )
	{
		switch( type )
		{
			case Type.NoteOff: 				_status = (byte) (128 + (int) channel ); return;
			case Type.NoteOn: 				_status = (byte) (144 + (int) channel); return;
			case Type.PolyAfterTouch: 		_status = (byte) (160 + (int) channel); return;
			case Type.ControlChange: 		_status = (byte) (176 + (int) channel); return;
			case Type.ProgramChange: 		_status = (byte) (192 + (int) channel); return;
			case Type.ChannelAfterTouch:	_status = (byte) (208 + (int) channel); return;
			case Type.PitchWheel: 			_status = (byte) (224 + (int) channel); return;
			case Type.System: 				_status = (byte) (240 + (int) channel); return;
		}
		_status = 0; // Unkown.
	}


	public override string ToString()
	{
		return "(" + _port + "," + _status + "," + _data1 + "," + _data2 + ")";
	}


	[Serializable]
	public enum Type
	{
		NoteOff,
		NoteOn,
		PolyAfterTouch,
		ControlChange,
		ProgramChange,
		ChannelAfterTouch,
		PitchWheel,
		System,
		Unknown,
	}


	[Serializable]
	public enum Channel
	{
		Ch1,
		Ch2,
		Ch3,
		Ch4,
		Ch5,
		Ch6,
		Ch7,
		Ch8,
		Ch9,
		Ch10,
		Ch11,
		Ch12,
		Ch13,
		Ch14,
		Ch15,
		Ch16,
		NotApplicable,
	}
}