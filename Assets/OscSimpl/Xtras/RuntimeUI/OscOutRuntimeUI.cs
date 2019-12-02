/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;

namespace OscSimpl
{
	[RequireComponent(typeof(RectTransform))]
	public class OscOutRuntimeUI : MonoBehaviour
	{
		public OscOut oscOut;
		public Toggle openToggle;
		public InputField portInputField;
		public Dropdown modeDropdown;
		public InputField ipAddressInputField;
		public Text messageBufferText;
		public Toggle messagesToggle;
		public Text messagesToggleLabel;
		public GameObject messageContainer;

		//StringBuilder _consoleText = new StringBuilder();
		StringBuilder _sb = new StringBuilder();
		string[] _messageStringBuffer = new string[messageBufferCapacity];
		Queue<string> _messageStringQueue = new Queue<string>( messageBufferCapacity );
		//Queue<int> _messageCharCountQueue = new Queue<int>( messageBufferCapacity );

		int _port;
		string _ipAddress;

		const int messageBufferCapacity = 10;
		const string messageToggleTextEnabled = "Hide Messages";
		const string messageToggleTextDisabled = "Show Messages";

		string portPrefKey { get { return "OscOut Port -" + name; } }
		string ipAddressPrefKey { get { return "OscOut IP Address -" + name; } }
		string messagesVisibilityPrefKey { get { return "OscOut Messages Visibility -" + name; } }


		void OnEnable()
		{
			// Load settings and apply.
			if( PlayerPrefs.HasKey( portPrefKey ) && PlayerPrefs.HasKey( ipAddressPrefKey ) && PlayerPrefs.HasKey( messagesVisibilityPrefKey ) ){
				int tempPort = PlayerPrefs.GetInt( portPrefKey );
				string tempIpAddress = PlayerPrefs.GetString( ipAddressPrefKey );
				oscOut.Open( tempPort, tempIpAddress );
				modeDropdown.value = (int) oscOut.mode; // avoid onChanged call
				messagesToggle.isOn = PlayerPrefs.GetInt( messagesVisibilityPrefKey ) == 1 ? true : false;
				OnMessageVisibilityChanged( messagesToggle.isOn );
			}

			// Subcribe to UI events.
			openToggle.onValueChanged.AddListener( OnOpenChanged );
			portInputField.onEndEdit.AddListener( OnPortEndEdit );
			modeDropdown.onValueChanged.AddListener( OnModeChanged );
			ipAddressInputField.onEndEdit.AddListener( OnIpAddressEndEdit );
			messagesToggle.onValueChanged.AddListener( OnMessageVisibilityChanged );
		}


		void OnDisable()
		{
			_messageStringQueue.Clear();

			// Unsubcribe to UI events.
			openToggle.onValueChanged.RemoveListener( OnOpenChanged );
			portInputField.onEndEdit.RemoveListener( OnPortEndEdit );
			modeDropdown.onValueChanged.RemoveListener( OnModeChanged );
			ipAddressInputField.onEndEdit.RemoveListener( OnIpAddressEndEdit );
			messagesToggle.onValueChanged.RemoveListener( OnMessageVisibilityChanged );

			// Save settings.
			PlayerPrefs.SetInt( portPrefKey, oscOut.port );
			PlayerPrefs.SetString( ipAddressPrefKey, oscOut.remoteIpAddress );
			PlayerPrefs.SetInt( messagesVisibilityPrefKey, messagesToggle.isOn ? 1 : 0 );
		}



		void Update()
		{
			if( oscOut == null ){
				Destroy( this );
				return;
			}

			// Update UI.
			if( oscOut.isOpen != openToggle.isOn ) openToggle.isOn = oscOut.isOpen;
			if( oscOut.port != _port ){
				_port = oscOut.port;
				portInputField.text = _port.ToString();
			}
			if( oscOut.mode != (OscSendMode) modeDropdown.value ){
				modeDropdown.value = (int) oscOut.mode;
			}
			if( oscOut.remoteIpAddress != _ipAddress ){
				_ipAddress = oscOut.remoteIpAddress;
				ipAddressInputField.text = _ipAddress;
			}

			if( messagesToggle.isOn ){
				_sb.Clear();
				_messageStringQueue.CopyTo( _messageStringBuffer, 0 ); // Copy to array so we can iterate backswards.
				for( int i = _messageStringBuffer.Length-1; i >= 0; i-- ) _sb.AppendLine( _messageStringBuffer[i] );
				messageBufferText.text = _sb.ToString();
			}
		}


		void OnAnyMessage( OscMessage message )
		{
			if( _messageStringQueue.Count >= messageBufferCapacity ) _messageStringQueue.Dequeue();
			_messageStringQueue.Enqueue( message.ToString() );

			// Always recycle received messages, also when received from OscOut.
			OscPool.Recycle( message );
		}


		void OnOpenChanged( bool on )
		{
			if( on ) oscOut.Open( _port, _ipAddress );
			else oscOut.Close();
		}


		void OnPortEndEdit( string portString )
		{
			if( string.IsNullOrEmpty( portString ) ){
				portInputField.text = oscOut.port.ToString();
				return;
			}
			_port = int.Parse( portString );
			oscOut.Open( _port, _ipAddress );
		}


		void OnModeChanged( int modeInt )
		{
			switch( (OscSendMode) modeInt ){
			case OscSendMode.UnicastToSelf: 	oscOut.Open( oscOut.port ); break;
			case OscSendMode.Unicast: 			oscOut.Open( oscOut.port, OscConst.unicastAddressDefault ); break;
			case OscSendMode.Multicast:			oscOut.Open( oscOut.port, OscConst.multicastAddressDefault ); break;
			case OscSendMode.Broadcast:			oscOut.Open( oscOut.port, System.Net.IPAddress.Broadcast.ToString() ); break;
			}
		}


		void OnIpAddressEndEdit( string ipAddress )
		{
			_ipAddress = ipAddress;
			oscOut.Open( _port, ipAddress );
		}


		void OnMessageVisibilityChanged( bool visible )
		{
			messageContainer.SetActive( visible );
			if( visible ){
				oscOut.MapAnyMessage( OnAnyMessage );
				messagesToggleLabel.text = messageToggleTextEnabled;
			} else {
				oscOut.UnmapAnyMessage( OnAnyMessage );
				messagesToggleLabel.text = messageToggleTextDisabled;
				_messageStringQueue.Clear();
			}
		}
	}
}