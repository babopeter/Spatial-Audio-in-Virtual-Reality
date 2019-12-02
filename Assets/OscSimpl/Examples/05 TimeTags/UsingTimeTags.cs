/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2019 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;

namespace OscSimpl.Examples
{
	public class UsingTimeTags : MonoBehaviour
	{
		OscOut _oscOut;
		OscIn _oscIn;

		const string address = "/test";


		void Start()
		{
			// Set up OscIn and OscOut for local testing.
			_oscOut = gameObject.AddComponent<OscOut>();
			_oscIn = gameObject.AddComponent<OscIn>(); 
			_oscOut.Open( 7000 );
			_oscIn.Open( 7000 );

			// Forward received messages with address to method.
			_oscIn.MapTimeTag( address, OnReceived );
		}


		void Update()
		{
			// Create a timetag with the current time.
			OscTimeTag timetag = new OscTimeTag( System.DateTime.Now );

			// Make it 1 milisecond into the future.
			timetag.time = timetag.time.AddMilliseconds( 1 );

			// Send it off.
			_oscOut.Send( address, timetag );
		}


		void OnReceived( OscTimeTag timeTag )
		{
			Debug.Log( timeTag + "\n" );
		}
	}
}