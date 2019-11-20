/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2019 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace OscSimpl
{
	[Serializable]
	public class OscMapping
	{
		[FormerlySerializedAs("address")] // Update 1.3 -> 2.0
		[SerializeField] string _address = "/";

		public OscMessageType type = OscMessageType.OscMessage;
		
		public OscMessageEvent OscMessageHandler = new OscMessageEvent();
		public OscFloatEvent FloatHandler = new OscFloatEvent();
		public OscDoubleEvent DoubleHandler = new OscDoubleEvent();
		public OscIntEvent IntHandler = new OscIntEvent();
		public OscLongEvent LongHandler = new OscLongEvent();
		public OscStringEvent StringHandler = new OscStringEvent();
		public OscCharEvent CharHandler = new OscCharEvent();
		public OscBoolEvent BoolHandler = new OscBoolEvent();
		public OscColorEvent ColorHandler = new OscColorEvent();
		public OscBlobEvent BlobHandler = new OscBlobEvent();
		public OscTimeTagEvent TimeTagHandler = new OscTimeTagEvent();
		public OscMidiEvent MidiHandler = new OscMidiEvent();
		public OscEvent ImpulseNullEmptyHandler = new OscEvent();

		[NonSerialized] bool _hasSpecialPattern;
		[NonSerialized] bool _hasCheckedForSpecialPattern;


		public string address
		{
			get { return _address; }
			set {
				OscAddress.Sanitize( ref value );
				_address = value;
				_hasCheckedForSpecialPattern = false;
			}
		}

		public bool hasSpecialPattern {
			get {
				if( _hasCheckedForSpecialPattern ) return _hasSpecialPattern;
				_hasSpecialPattern = OscAddress.HasAnySpecialPatternCharacter( _address );
				return _hasSpecialPattern;
			}
		}
		
		
		public OscMapping( string address, OscMessageType type )
		{
			this.address = address;
			this.type = type;
		}


		public bool IsMatching( string address )
		{
			if( hasSpecialPattern ) return OscAddress.IsMatching( address, _address );
			return string.Compare( address, _address ) == 0;
		}	


		public void Clear()
		{
			OscMessageHandler.RemoveAllListeners();
			FloatHandler.RemoveAllListeners();
			IntHandler.RemoveAllListeners();
			StringHandler.RemoveAllListeners();
			BoolHandler.RemoveAllListeners();
			ColorHandler.RemoveAllListeners();
			CharHandler.RemoveAllListeners();
			DoubleHandler.RemoveAllListeners();
			LongHandler.RemoveAllListeners();
			TimeTagHandler.RemoveAllListeners();
			MidiHandler.RemoveAllListeners();
			BlobHandler.RemoveAllListeners();
			ImpulseNullEmptyHandler.RemoveAllListeners();
		}
	}
}