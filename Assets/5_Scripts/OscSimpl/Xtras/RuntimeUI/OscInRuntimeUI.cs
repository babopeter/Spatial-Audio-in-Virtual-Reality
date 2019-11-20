/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace OscSimpl
{
	[RequireComponent(typeof(RectTransform))]
	public class OscInRuntimeUI : MonoBehaviour
	{
		public OscIn oscIn;
		public Toggle openToggle;
		public InputField portInputField;
		public Dropdown modeDropdown;
		public Text localIpAddressLabel;
		public InputField multicastAddressInputField;
		public Text messageBufferText;
		public Toggle messagesToggle;
		public Text messagesToggleLabel;
		public GameObject messageContainer;

		RectTransform _componentRect;

		StringBuilder _sb = new StringBuilder();
		string[] _messageStringBuffer = new string[messageBufferCapacity];
		Queue<string> _messageStringQueue = new Queue<string>( messageBufferCapacity );

		int _port;
		string _multicastAddress;
		OscReceiveMode _mode;

		const int messageBufferCapacity = 10;
		const string messageToggleTextEnabled = "Hide Messages";
		const string messageToggleTextDisabled = "Show Messages";
		const int componentHeightNonMulticast = 163;
		const int componentHeightMulticast = 193;

		string portPrefKey { get { return "OscIn Port -" + name; } }
		string modePrefKey { get { return "OscIn Mode -" + name; } }
		string multicastAddressPrefKey { get { return "OscIn Multicast Address -" + name; } }
		string messagesVisibilityPrefKey { get { return "OscIn Messages Visibility -" + name; } }


		void Awake()
		{
			_componentRect = gameObject.GetComponent<RectTransform>();
		}


		void OnEnable()
		{
			// Load settings and apply
			if(
				PlayerPrefs.HasKey( portPrefKey ) && 
				PlayerPrefs.HasKey( modePrefKey ) &&
				PlayerPrefs.HasKey( multicastAddressPrefKey ) && 
				PlayerPrefs.HasKey( messagesVisibilityPrefKey )
			){
				int tempPort = PlayerPrefs.GetInt( portPrefKey );
				OscReceiveMode tempMode = (OscReceiveMode) PlayerPrefs.GetInt( modePrefKey );
				string tempMulticastAddress = PlayerPrefs.GetString( multicastAddressPrefKey );
				modeDropdown.value = (int) tempMode; // avoid onChanged call
				Open( tempPort, tempMode, tempMulticastAddress );
				messagesToggle.isOn = PlayerPrefs.GetInt( messagesVisibilityPrefKey ) == 1 ? true : false;
				OnMessageVisibilityChanged( messagesToggle.isOn );
			}

			// Subcribe to UI events
			openToggle.onValueChanged.AddListener( OnOpenChanged );
			portInputField.onEndEdit.AddListener( OnPortEndEdit );
			modeDropdown.onValueChanged.AddListener( OnModeChanged );
			multicastAddressInputField.onEndEdit.AddListener( OnIpAddressEndEdit );
			messagesToggle.onValueChanged.AddListener( OnMessageVisibilityChanged );
		}


		void OnDisable()
		{
			_messageStringQueue.Clear();

			// Unsubcribe to UI events
			openToggle.onValueChanged.RemoveListener( OnOpenChanged );
			portInputField.onEndEdit.RemoveListener( OnPortEndEdit );
			modeDropdown.onValueChanged.RemoveListener( OnModeChanged );
			multicastAddressInputField.onEndEdit.RemoveListener( OnIpAddressEndEdit );
			messagesToggle.onValueChanged.RemoveListener( OnMessageVisibilityChanged );

			// Save settings
			PlayerPrefs.SetInt( portPrefKey, oscIn.port );
			PlayerPrefs.SetInt( modePrefKey, (int) oscIn.mode );
			PlayerPrefs.SetString( multicastAddressPrefKey, oscIn.multicastAddress );
			PlayerPrefs.SetInt( messagesVisibilityPrefKey, messagesToggle.isOn ? 1 : 0 );
		}


		void Update()
		{
			if( oscIn == null ){
				Destroy( this );
				return;
			}

			// Update UI
			if( oscIn.isOpen != openToggle.isOn ) openToggle.isOn = oscIn.isOpen;
			if( oscIn.port != _port ){
				_port = oscIn.port;
				portInputField.text = _port.ToString();
			}
			if( oscIn.mode != (OscReceiveMode) modeDropdown.value ){
				_mode = oscIn.mode;
				modeDropdown.value = (int) oscIn.mode;
			}
			if( OscIn.localIpAddress != localIpAddressLabel.text ){
				if( string.IsNullOrEmpty( OscIn.localIpAddress ) ) localIpAddressLabel.text = "Local IP Not found";
				else localIpAddressLabel.text = OscIn.localIpAddress;
			}
			if( oscIn.multicastAddress != _multicastAddress ){
				_multicastAddress = oscIn.multicastAddress;
				multicastAddressInputField.text = _multicastAddress;
			}

			if( messagesToggle.isOn ){
				_sb.Clear();
				_messageStringQueue.CopyTo( _messageStringBuffer, 0 ); // Copy to array so we can iterate backswards.
				for( int i = _messageStringBuffer.Length-1; i >= 0; i-- ) _sb.AppendLine( _messageStringBuffer[i] );
				messageBufferText.text = _sb.ToString();
			}
		}


		void Open( int port, OscReceiveMode mode, string multicastAddress )
		{
			switch( mode ){
			case OscReceiveMode.UnicastBroadcast:
				oscIn.Open( port );
				_componentRect.sizeDelta = new Vector2( _componentRect.sizeDelta.x, componentHeightNonMulticast );
				break;
			case OscReceiveMode.UnicastBroadcastMulticast:
				oscIn.Open( port, multicastAddress );
				_componentRect.sizeDelta = new Vector2( _componentRect.sizeDelta.x, componentHeightMulticast );
				break;
			}
		}


		void OnAnyMessage( OscMessage message )
		{
			if( _messageStringQueue.Count >= messageBufferCapacity ) _messageStringQueue.Dequeue();
			_messageStringQueue.Enqueue( message.ToString() );

			// Always recycle received messages.
			OscPool.Recycle( message );
		}


		void OnOpenChanged( bool on )
		{
			if( on ) Open( _port, _mode, _multicastAddress );
			else oscIn.Close();
		}


		void OnPortEndEdit( string portString )
		{
			if( string.IsNullOrEmpty( portString ) ){
				portInputField.text = oscIn.port.ToString();
				return;
			}
			_port = int.Parse( portString );
			Open( _port, _mode, _multicastAddress );
		}


		void OnModeChanged( int modeInt )
		{
			_mode = (OscReceiveMode) modeInt;
			Open( _port, _mode, _multicastAddress );
		}


		void OnIpAddressEndEdit( string multicastAddress )
		{
			_multicastAddress = multicastAddress;
			Open( _port, _mode, multicastAddress );
		}


		void OnMessageVisibilityChanged( bool visible )
		{
			messageContainer.SetActive( visible );
			if( visible ){
				oscIn.MapAnyMessage( OnAnyMessage );
				messagesToggleLabel.text = messageToggleTextEnabled;
			} else {
				oscIn.UnmapAnyMessage( OnAnyMessage );
				messagesToggleLabel.text = messageToggleTextDisabled;
				_messageStringQueue.Clear();
			}
		}
	}
}