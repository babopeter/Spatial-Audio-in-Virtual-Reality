/*
	Created by Carl Emil Carlsen.
	Copyright 2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;

namespace OscSimpl.Examples
{
	public class QuaternionBlob : MonoBehaviour
	{
		OscOut _oscOut;
		OscIn _oscIn;

		OscMessage _message;

		const string address = "/test/quaternion";


		void Start()
		{
			// Set up OscIn and OscOut objects for a local test.
			_oscOut = gameObject.AddComponent<OscOut>();
			_oscIn = gameObject.AddComponent<OscIn>(); 
			_oscOut.Open( 7000 );
			_oscIn.Open( 7000 );

			// Request messages with 'address' to be send to 'OnMessageReceived'.
			_oscIn.Map( address, OnMessageReceived );

			// Always cache messages when possible.
			_message = new OscMessage( address );
		}


		void Update()
		{
			// Create a test value.
			Quaternion value = Random.rotation;

			// Use SetAsBlob to write a Quaternion value in a byte blob.
			_message.SetBlob( 0, value );

			// Send.
			_oscOut.Send( _message  );

			// Log.
			Debug.Log( "Sending: " + value + "\n" );
		}


		void OnMessageReceived( OscMessage message )
		{
			// Use TryGetFromBlob to read a Quaternion value from a byte blob.
			Quaternion value;
			if( message.TryGetBlob( 0, out value ) ){
				Debug.Log( "Receiving: " + value + "\n" );
			}

			// Always recycle received messages when used.
			OscPool.Recycle( message );
		}
	}
}