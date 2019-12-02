/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;

namespace OscSimpl.Examples
{
	public class GettingStartedReceiving : MonoBehaviour
	{
		[SerializeField] OscIn _oscIn;

		const string address1 = "/test1";
		const string address2 = "/test2";


		void Start()
		{
			// Ensure that we have a OscIn component and start receiving on port 7000.
			if( !_oscIn ) _oscIn = gameObject.AddComponent<OscIn>();
			_oscIn.Open( 7000 );
		}


		void OnEnable()
		{
			// You can "map" messages to methods in two ways:

			// 1) For messages with a single argument, route the value using the type specific map methods.
			_oscIn.MapFloat( address1, OnTest1 );

			// 2) For messages with multiple arguments, route the message using the Map method.
			_oscIn.Map( address2, OnTest2 );
		}


		void OnDisable()
		{
			// If you want to stop receiving messages you have to "unmap".
			_oscIn.UnmapFloat( OnTest1 );
			_oscIn.Unmap( OnTest2 );
		}

		
		void OnTest1( float value )
		{
			Debug.Log( "Received test1\n" + value + "\n" );
		}


		void OnTest2( OscMessage message )
		{
			// Get arguments from index 0, 1 and 2 safely.
			int frameCount;
			float time;
			float random;
			if(
				message.TryGet( 0, out frameCount ) && 
				message.TryGet( 1, out time ) &&
				message.TryGet( 2, out random )
			) {
				Debug.Log( "Received test2\n" + frameCount + " " + time + " " + random + "\n" );
			}

			// If you don't know what type of arguments to expect, then you 
			// can check the type of each argument and get the ones you need.
			for( int i = 0; i < message.Count(); i++ )
			{
				OscArgType argType;
				if( !message.TryGetArgType( i, out argType ) ) continue;

				switch( argType )
				{
					case OscArgType.Float:
						float floatValue;
						message.TryGet( i, out floatValue );
						// Do something with floatValue here...
						break;
					case OscArgType.Int:
						int intValue;
						message.TryGet( i, out intValue );
						// Do something with intValue here...
						break;
				}
			}

			// Always recycle incoming messages when used.
			OscPool.Recycle( message );
		}
	}
}