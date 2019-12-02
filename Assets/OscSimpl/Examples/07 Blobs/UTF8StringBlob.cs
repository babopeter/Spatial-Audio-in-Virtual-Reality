/*
	Created by Carl Emil Carlsen.
	Copyright 2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;

namespace OscSimpl.Examples
{
	public class UTF8StringBlob : MonoBehaviour
	{
		OscOut _oscOut;
		OscIn _oscIn;

		OscMessage _message;
		string _sendText = "Rødgrød med fløde.";

		const string address = "/test/utf8string";


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
			// Update test text content.
			int tempI = Random.Range( 0, _sendText.Length );
			char tempChar = _sendText[tempI];
			_sendText = _sendText.Remove( tempI, 1 );
			_sendText = _sendText.Insert( Random.Range( 0, _sendText.Length ), tempChar.ToString() );

			// Use SetAsBlob to write a utf-8 string to a byte blob.
			_message.SetBlob( 0, System.Text.Encoding.UTF8, _sendText );

			// Send.
			_oscOut.Send( _message  );

			// Log.
			Debug.Log( "Sending: " + _sendText + "\n" );
		}


		void OnMessageReceived( OscMessage message )
		{
			// Use TryGetFromBlob to read a Vector2 value from a byte blob.
			string receviedText;
			if( message.TryGetBlob( 0, System.Text.Encoding.UTF8, out receviedText ) ){
				Debug.Log( "Receiving: " + receviedText + "\n" );
			}

			// Always recycle received messages when used.
			OscPool.Recycle( message );
		}
	}
}