/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2019 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;

namespace OscSimpl.Examples
{
	public class Optimisations : MonoBehaviour
	{
		// We will be sinding this string as an example.
		public string outgoingText = "Hello";

		// Always store outgoing messages that contain more than one argument.
		OscMessage _outgoingMessage;

		// Always cache incoming strings and blobs so only generate garbage
		// when strings change content and blobs change length.
		string _incomingText;

		// Always store strings that never change in constants.
		const string address = "/test";

		OscOut _oscOut;
		OscIn _oscIn;


		void Start()
		{
			// Set up OscIn and OscOut for local testing.
			_oscOut = gameObject.AddComponent<OscOut>();
			_oscIn = gameObject.AddComponent<OscIn>(); 
			_oscOut.Open( 7000 );
			_oscIn.Open( 7000 );

			// OPTIMISATION #1
			// Ensure that outgoing messages will be bundled automatically.
			// You can also bundle messages yourself; check the Bundles example.
			_oscOut.bundleMessagesOnEndOfFrame = true;

			// OPTIMISATION #2
			// Instantiate outgoing messages once and cache them locally.
			// If you are sending a single argument that is neither a string
			// nor a blob, then you can use the optimised method
			// Send( address, value ) instead.
			_outgoingMessage = new OscMessage( address );

			// OPTIMISATION #3
			// When receving strings and blobs then handle the message yourself.
			// Otherwise new strings and byte arrays will be generated continously.
			_oscIn.Map( address, OnMessageReceived );
		}


		void Update()
		{
			// Update the message.
			_outgoingMessage.Set( 0, outgoingText );

			// Send the message, knowing that because oscOut.bundleMessagesOnEndOfFrame
			// is set to true, it will be bundles with other messages and send in one go.
			_oscOut.Send( _outgoingMessage );
		}


		void OnMessageReceived( OscMessage incomingMessage )
		{
			// Try to get the string from the message at arg index 0.
			if( incomingMessage.TryGet( 0, ref _incomingText ) )
			{
				// We have now received a string that will only be
				// recreated (generate garbage) if it changes.

				// However, this Debug.Log call will generate garbage. Lots of it ;)
				Debug.Log( _incomingText );
			}

			// OPTIMISATION #4
			// Always recycle messages when you handle them yourself.
			OscPool.Recycle( incomingMessage );
		}
	}
}