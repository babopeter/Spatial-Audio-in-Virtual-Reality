/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

/// <summary>
/// Enum representing the argument types supported by OSC simpl.
/// </summary>
public enum OscArgType
{
	Null,
	Impulse,
	Bool,

	Float,
	Int,
	Char,
	Color,
	Midi,

	Double,
	Long,
	TimeTag,

	String,
	Blob,

	Unsupported
}