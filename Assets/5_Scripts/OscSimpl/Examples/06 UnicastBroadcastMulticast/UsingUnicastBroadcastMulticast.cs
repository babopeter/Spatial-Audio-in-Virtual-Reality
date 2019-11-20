/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2019 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;

namespace OscSimpl.Examples
{
	public class UsingUnicastBroadcastMulticast : MonoBehaviour
	{
		OscOut _oscOut;
		OscIn _oscIn;

		const string address = "/test";


		void Start()
		{
			// Create objects for sending and receiving.
			_oscOut = gameObject.AddComponent<OscOut>();
			_oscIn = gameObject.AddComponent<OscIn>();

			// If the concepts of unicast, broadcast and multicast makes no sense
			// then please read the Underlying Concepts section in the manual.

			// SEND UNICAST
			// To unicast locally on this device, just provide a port.
			_oscOut.Open( 7000 );
			// To unicast to a specific remote target, provide an IP address
			//_oscOut.Open( 7000, "192.168.1.60" );

			// SEND BROADCAST
			// To broadcast, provide the broadcast IP. You can hardcode it like
			// here, or obtain it from System.Net.IPAddress.Broadcast.ToString().
			//_oscOut.Open( 7000, "255.255.255.255" );

			// SEND MULTICAST
			// To multicast, provide a multicast IP. Multicast addresses must be 
			// between 224.0.0.0 to 239.255.255.255, but addresses 224.0.0.0 to 
			// 224.0.0.255 are reserved for routing info so you should really 
			// only use 224.0.1.0 to 239.255.255.255.
			//_oscOut.Open( 7000, "224.1.2.3" );

			// RECEIVE UNICAST AND BROADCAST
			// To receive unicast and broadcast messages send locally on this device
			// and from remote transmitters, just provide a port. Note that if you 
			// are offline, you won't receive broadcasted messages.
			_oscIn.Open( 7000 );

			// RECEIVE MULTICAST
			// Provide a multicast address.
			//_oscIn.Open( 7000, "224.1.2.3" );

			// Forward recived messages with address to method.
			_oscIn.MapFloat( address, OnReceived );
		}


		void Update()
		{
			_oscOut.Send( address, Random.value );
		}


		void OnReceived( float value )
		{
			Debug.Log( value + "\n" );
		}
	}
}