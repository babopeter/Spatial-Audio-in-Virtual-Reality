/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

// Note that the simplest way to use bundles is to enable 'bundleMessagesOnEndOfFrame' on OscOut.
// But you you need to go manual, you can. Here is how:

using UnityEngine;

namespace OscSimpl.Examples
{
	public class UsingBundles : MonoBehaviour
	{
		OscIn _oscIn;
		OscOut _oscOut;

		// Always cache bundles and messages that are handled
		// in the update loop.
		OscBundle _bundle;
		OscMessage _message1, _message2;

		const string address1 = "/test1";
		const string address2 = "/test2";


		void Start()
		{
			// Set up OscIn and OscOut for testing locally
			_oscOut = gameObject.AddComponent<OscOut>();
			_oscIn = gameObject.AddComponent<OscIn>();
			_oscOut.Open( 7000 );
			_oscIn.Open( 7000 );

			// Forward received messages with addresses to methods.
			_oscIn.Map( address1, OnReceived1 );
			_oscIn.Map( address2, OnReceived2 );

			// Create a bundle and two messages.
			_bundle = new OscBundle();
			_message1 = new OscMessage( address1 );
			_message2 = new OscMessage( address2 );

			// Add the two messages to the bundle.
			_bundle.Add( _message1 );
			_bundle.Add( _message2 );

			// Messages contained in incoming bundles are automatically unpacked,
			// and so incoming bundle objects are never exposed.
			// If you need to access time tags from incoming bundles you can set a
			// flag that will add the a time tag from contained messages when unpacking.
			//_oscIn.addTimeTagsToBundledMessages = true;
		}


		void Update()
		{
			// Update the bundle time tag if you want to.
			_bundle.timeTag = new OscTimeTag( System.DateTime.Now );

			// Update message arguments.
			_message1.Set( 0, Random.value );
			_message2.Set( 0, Random.value );

			// Send the bundle, containing the two messages.
			_oscOut.Send( _bundle );

			// Log.
			Debug.Log( "Send as bundle\n" + _message1 + " AND " + _message2 );
		}


		void OnReceived1( OscMessage message )
		{
			// Log.
			Debug.Log( "Received\n" + message );

			// Always recycle incoming messages when used.
			OscPool.Recycle( message );
		}


		void OnReceived2( OscMessage message )
		{
			// Log.
			Debug.Log( "Received\n" + message );

			// Always recycle incoming messages when used.
			OscPool.Recycle( message );
		}
	}
}