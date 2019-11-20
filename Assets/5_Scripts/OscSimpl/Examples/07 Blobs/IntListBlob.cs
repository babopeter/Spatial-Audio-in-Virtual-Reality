/*
	Created by Carl Emil Carlsen.
	Copyright 2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace OscSimpl.Examples
{
	public class IntListBlob : MonoBehaviour
	{
		OscOut _oscOut;
		OscIn _oscIn;

		OscMessage _message;
		List<int> _sendInts;
		List<int> _receivedInts;

		const string address = "/test/ints";


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
			_sendInts = new List<int>();
			_receivedInts = new List<int>();
		}


		void Update()
		{
			// Update test list with new content.
			_sendInts.Clear();
			int randomCount = Random.Range( 1, 5 );
			for( int i = 0; i < randomCount; i++ ) _sendInts.Add( Random.Range( 0, 100 ) );

			// Use SetAsBlob to write list of floats to a byte blob.
			_message.SetBlob( 0, _sendInts );

			// Send.
			_oscOut.Send( _message  );

			// Log.
			Debug.Log( "Sending: " + ListToString( _sendInts ) + "\n" );
		}


		void OnMessageReceived( OscMessage message )
		{
			// Use TryGetFromBlob to read a list of floats from a byte blob.
			if( message.TryGetBlob( 0, ref _receivedInts ) ){
				Debug.Log( "Receiving: " + ListToString( _receivedInts ) + "\n" );
			}

			// Always recycle received messages when used.
			OscPool.Recycle( message );
		}


		static string ListToString<T>( List<T> floats )
		{
			StringBuilder sb = new StringBuilder();
			for( int i = 0; i < floats.Count; i++ ){
				if( i > 0 ) sb.Append( ", " );
				sb.Append( floats[i] );
			}
			return sb.ToString();
		}
	}
}