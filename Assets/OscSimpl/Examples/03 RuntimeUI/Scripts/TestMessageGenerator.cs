/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;

namespace OscSimpl.Examples
{
	public class TestMessageGenerator : MonoBehaviour
	{
		[SerializeField] OscOut _oscOut = null;

		const string _address = "/test";


		void Update()
		{
			_oscOut.Send( _address, Random.value );
		}
	}
}