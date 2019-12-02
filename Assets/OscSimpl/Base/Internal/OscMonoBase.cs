/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2019 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;

namespace OscSimpl
{
	public class OscMonoBase : MonoBehaviour
	{
		[SerializeField] protected bool _openOnAwake = false;
		[SerializeField] protected int _udpBufferSize = OscConst.udpBufferSizeDefault;

		protected OscMessageEvent _onAnyMessage = new OscMessageEvent();
		protected int _onAnyMessageListenerCount = 0;
		protected int _messageCount = 0;

#if UNITY_EDITOR
		// Accessed by inspector through reflection.
		[SerializeField] protected OscMessageEvent _inspectorMessageEvent = new OscMessageEvent();
#endif
	}
}

