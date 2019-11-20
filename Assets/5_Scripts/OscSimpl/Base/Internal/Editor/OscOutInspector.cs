/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace OscSimpl
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(OscOut))]
	public class OscOutInspector : Editor
	{
		OscOut _oscOut;
		
		SerializedProperty _openOnAwake;
		SerializedProperty _remoteIpAddress;
		SerializedProperty _port;
		SerializedProperty _multicastLoopback;
		SerializedProperty _bundleMessagesOnEndOfFrame;
		SerializedProperty _splitBundlesAvoidingBufferOverflow;
		SerializedProperty _udpBufferSize;
		SerializedProperty _settingsFoldout;
		SerializedProperty _messagesFoldout;
		
		readonly GUIContent _portLabel = new GUIContent( "Port", "Port for target (remote) device." );
		readonly GUIContent _modeLabel = new GUIContent( "Send Mode", "Transmission mode." );
		readonly GUIContent _remoteIpAddressLabel = new GUIContent( "Remote IP Address", "IP Address for target (remote) device. LED shows ping status; gray for pingning, yellow for fail, green for success." );
		readonly GUIContent _isOpenLabel = new GUIContent( "Is Open", "Indicates whether this OscOut object is open and ready to send. In Edit Mode OSC objects are opened and closed automatically by their inspectors." );
		readonly GUIContent _openOnAwakeLabel = new GUIContent( "Open On Awake", "Open this OscOut object automatically when Awake is invoked by Unity (at runtime). The setting is only accessible using the inspector in Edit Mode." );
		readonly GUIContent _multicastLoopbackLabel = new GUIContent( "Multicast Loopback", "Whether outgoing multicast messages are delivered to the sending application." );
		readonly GUIContent _bundleMessagesOnEndOfFrameLabel = new GUIContent( "Bundle Messages On End Of Frame", "Bundle all messages and send automatically at the end of the frame." );
		readonly GUIContent _splitBundlesAvoidingBufferOverflowLabel = new GUIContent( "Split Bundles Avoiding Buffer Overflow", "Ensure that bundles sizes never exceed Udp buffer size" );
		readonly GUIContent _udpBufferSizeLabel = new GUIContent( "Udp Buffer Size" );
		readonly GUIContent _settingsFoldLabel = new GUIContent( "Settings" );

		StringBuilder _sb = new StringBuilder();
		string[] _messageStringBuffer = new string[messageBufferCapacity];
		Queue<string> _messageStringQueue = new Queue<string>( messageBufferCapacity );

		string _prevControlName;
		
		string _tempIPAddress;
		int _tempPort;
		int _tempBufferSize;
		
		OscRemoteStatus _statusInEditMode = OscRemoteStatus.Unknown;

		IEnumerator _pingEnumerator;
		DateTime _lastPingTime;
		
		// We use reflection to access _inspectorMessageEventObject, to ceil it from users.
		object _inspectorMessageEventObject;

		const string ipAddressControlName = "OscOut IP Address";
		const string portControlName = "OscOut Port";
		const string bufferSizeControlName = "OscOut Buffer Size";
		const int messageBufferCapacity = 10;

		const float pingInterval = 1.0f; // Seconds
		const int executionOrderNum = -5000;



		void OnEnable()
		{
			_oscOut = target as OscOut;
			
			// Get serialized properties.
			_openOnAwake = serializedObject.FindProperty("_openOnAwake");
			_remoteIpAddress = serializedObject.FindProperty( "_remoteIpAddress" );
			_port = serializedObject.FindProperty("_port");
			_multicastLoopback = serializedObject.FindProperty("_multicastLoopback");
			_bundleMessagesOnEndOfFrame = serializedObject.FindProperty("_bundleMessagesOnEndOfFrame");
			_splitBundlesAvoidingBufferOverflow = serializedObject.FindProperty( "_splitBundlesAvoidingBufferOverflow" );
			_udpBufferSize = serializedObject.FindProperty( "_udpBufferSize" );
			_settingsFoldout = serializedObject.FindProperty("_settingsFoldout");
			_messagesFoldout = serializedObject.FindProperty("_messagesFoldout");
			
			// Store socket info for change check workaround.
			_tempIPAddress = _remoteIpAddress.stringValue;
			_tempPort = _port.intValue;
			
			// Ensure that OscOut scripts will be executed early, so that if Open On Awake is enabled the socket will open before other scripts are called.
			MonoScript script = MonoScript.FromMonoBehaviour( target as MonoBehaviour );
			if( MonoImporter.GetExecutionOrder( script ) != executionOrderNum ) MonoImporter.SetExecutionOrder( script, executionOrderNum );
			
			// When object is selected in Edit Mode then we start listening.
			if( _oscOut.enabled && !Application.isPlaying && !_oscOut.isOpen ){
				_oscOut.Open( _oscOut.port, _oscOut.remoteIpAddress );
				_statusInEditMode = _oscOut.mode == OscSendMode.UnicastToSelf ? OscRemoteStatus.Connected : OscRemoteStatus.Unknown;
			}

			// Subscribe to OSC messages
			OscEditorUI.AddInspectorMessageListener( _oscOut, OnOSCMessage, ref _inspectorMessageEventObject );

			// If in Edit Mode, then start a coroutine that will update the connection status. Unity can't start coroutines in Runtime.
			if( !Application.isPlaying && _oscOut.mode == OscSendMode.Unicast ){
				_pingEnumerator = OscHelper.StartCoroutineInEditMode( PingCoroutine(), ref _lastPingTime );
			}
		}
		
		
		void OnDisable()
		{
			// When object is deselected in Edit Mode then we stop listening.
			if( !Application.isPlaying && _oscOut.isOpen ) _oscOut.Close();

			// Unsubscribe from messsages.
			OscEditorUI.RemoveInspectorMessageListener( _oscOut, OnOSCMessage, ref _inspectorMessageEventObject );
		}
		
		
		public override void OnInspectorGUI()
		{
			string currentControlName;
			bool deselect;

			// Check for key down before drawing any fields because they might consume the event.
			bool enterKeyDown = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
			
			// Load serialized object.
			serializedObject.Update();

			// Port field.
			EditorGUI.BeginChangeCheck();
			GUI.SetNextControlName( portControlName );
			int newPort = EditorGUILayout.IntField( _portLabel, _oscOut.port );
			if( EditorGUI.EndChangeCheck() ){
				_port.intValue = newPort;
				if( _oscOut.isOpen ) _oscOut.Close(); // Close socket while editing
			}
			currentControlName = GUI.GetNameOfFocusedControl();
			bool enterKeyDownPort = enterKeyDown && currentControlName == portControlName;
			if( enterKeyDownPort ) UnfocusAndUpdateUI();
			deselect = _prevControlName == portControlName && currentControlName != portControlName;
			if( ( deselect || enterKeyDownPort ) && !_oscOut.isOpen ){
				if( _oscOut.Open( _port.intValue, _remoteIpAddress.stringValue ) ){
					_tempPort = _port.intValue;
				} else {
					_port.intValue = _tempPort; // Undo
					_oscOut.Open( _port.intValue, _remoteIpAddress.stringValue );
				}
			}

			// Mode field.
			EditorGUI.BeginChangeCheck();
			OscSendMode newMode = (OscSendMode) EditorGUILayout.EnumPopup( _modeLabel, _oscOut.mode );
			if( EditorGUI.EndChangeCheck() && newMode != _oscOut.mode ){
				switch( newMode ){
				case OscSendMode.UnicastToSelf: 	_oscOut.Open( _oscOut.port ); break;
				case OscSendMode.Unicast: 			_oscOut.Open( _oscOut.port, OscConst.unicastAddressDefault ); break;
				case OscSendMode.Multicast:			_oscOut.Open( _oscOut.port, OscConst.multicastAddressDefault ); break;
				case OscSendMode.Broadcast:			_oscOut.Open( _oscOut.port, IPAddress.Broadcast.ToString() ); break;
				}
				UpdateStatusInEditMode();
			}

			// IP Address field.
			EditorGUI.BeginChangeCheck();
			GUI.SetNextControlName( ipAddressControlName );
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel( _remoteIpAddressLabel );
			string newIp = EditorGUILayout.TextField( _oscOut.remoteIpAddress ); // Field
			if( EditorGUI.EndChangeCheck() ){
				IPAddress ip;
				if( IPAddress.TryParse( newIp, out ip ) ) _remoteIpAddress.stringValue = newIp; // Accept only valid ip addresses
				if( _oscOut.isOpen ) _oscOut.Close(); // Close socket while editing
				if( !Application.isPlaying ){
					if( _pingEnumerator != null ) _pingEnumerator = null; // Don't update ping coroutine while editing
					if( _statusInEditMode != OscRemoteStatus.Unknown ) _statusInEditMode = OscRemoteStatus.Unknown;
				}
			}
			GUILayout.FlexibleSpace();
			Rect rect = GUILayoutUtility.GetRect( 16, 5 );
			rect.width = 5;
			rect.x += 3;
			rect.y += 7;
			OscRemoteStatus status = Application.isPlaying ? _oscOut.remoteStatus : _statusInEditMode;
			EditorGUI.DrawRect( rect, StatusToColor( status ) );
			EditorGUILayout.EndHorizontal();
			GUILayoutUtility.GetRect( 1, 2 ); // vertical spacing
			currentControlName = GUI.GetNameOfFocusedControl();
			bool enterKeyDownIp = enterKeyDown && currentControlName == ipAddressControlName;
			if( enterKeyDownIp ) UnfocusAndUpdateUI();
			deselect = _prevControlName == ipAddressControlName && currentControlName != ipAddressControlName;
			if( ( deselect || enterKeyDownIp ) && !_oscOut.isOpen ){ // All this mess to check for end edit, OMG!!! Not cool.
				if( _oscOut.Open( _port.intValue, _remoteIpAddress.stringValue ) ){
					_tempIPAddress = _remoteIpAddress.stringValue;
					UpdateStatusInEditMode();
				} else {
					_remoteIpAddress.stringValue = _tempIPAddress; // Undo
				}
			}
			
			// Is Open field.
			EditorGUI.BeginDisabledGroup( true );
			EditorGUILayout.Toggle( _isOpenLabel, _oscOut.isOpen );
			EditorGUI.EndDisabledGroup();

			// Open On Awake field.
			EditorGUI.BeginDisabledGroup( Application.isPlaying );
			EditorGUILayout.PropertyField( _openOnAwake, _openOnAwakeLabel );
			EditorGUI.EndDisabledGroup();

			EditorGUI.indentLevel++;

			// Settings ...
			_settingsFoldout.boolValue = EditorGUILayout.Foldout( _settingsFoldout.boolValue, _settingsFoldLabel );
			if( _settingsFoldout.boolValue )
			{
				// Multicast loopback field.
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField( _multicastLoopbackLabel, GUILayout.Width( 150 ) );
				GUILayout.FlexibleSpace();
				EditorGUI.BeginChangeCheck();
				_multicastLoopback.boolValue = EditorGUILayout.Toggle( _multicastLoopback.boolValue, GUILayout.Width( 30 ) );
				if( EditorGUI.EndChangeCheck() && _oscOut.mode == OscSendMode.Multicast ) _oscOut.multicastLoopback = _multicastLoopback.boolValue;
				EditorGUILayout.EndHorizontal();

				// Bundle Messages On End Of Frame field.
				BoolSettingsField( _bundleMessagesOnEndOfFrame, _bundleMessagesOnEndOfFrameLabel );

				// Split Bundles Avoiding Buffer Overflow field.
				BoolSettingsField( _splitBundlesAvoidingBufferOverflow, _splitBundlesAvoidingBufferOverflowLabel );

				// Udp Buffer Size field. (UI horror get get a end changed event).
				GUI.SetNextControlName( bufferSizeControlName );
				EditorGUI.BeginChangeCheck();
				int newBufferSize = EditorGUILayout.IntField( _udpBufferSizeLabel, _udpBufferSize.intValue );
				if( EditorGUI.EndChangeCheck() ) {
					_tempBufferSize = Mathf.Clamp( newBufferSize, OscConst.udpBufferSizeMin, OscConst.udpBufferSizeMax );
				}
				currentControlName = GUI.GetNameOfFocusedControl();
				bool enterKeyDownBufferSize = enterKeyDown && currentControlName == bufferSizeControlName;
				if( enterKeyDownBufferSize ) UnfocusAndUpdateUI();
				deselect = _prevControlName == bufferSizeControlName && currentControlName != bufferSizeControlName;
				if( enterKeyDownBufferSize || deselect ){
					if( _tempBufferSize != _udpBufferSize.intValue ) {
						_udpBufferSize.intValue = _tempBufferSize;
						_oscOut.udpBufferSize = _tempBufferSize; // This will reopen OscOut
					}
				}
			}

			// Messages ...
			EditorGUI.BeginDisabledGroup( !_oscOut.isOpen );
			GUIContent messagesFoldContent = new GUIContent( "Messages (" + _oscOut.messageCount + ")", "Messages received since last update" );
			_messagesFoldout.boolValue = EditorGUILayout.Foldout( _messagesFoldout.boolValue, messagesFoldContent );
			if( _messagesFoldout.boolValue ){
				_sb.Clear();
				_messageStringQueue.CopyTo( _messageStringBuffer, 0 ); // Copy to array so we can iterate backswards.
				for( int i = _messageStringBuffer.Length-1; i >= 0; i-- ) _sb.AppendLine( _messageStringBuffer[i] );
				EditorGUILayout.HelpBox( _sb.ToString(), MessageType.None );
			}
			EditorGUI.EndDisabledGroup();

			EditorGUI.indentLevel--;

			// Apply
			serializedObject.ApplyModifiedProperties();
			
			// Request OnInspectorGUI to be called every frame as long as inspector is active
			EditorUtility.SetDirty( target );
			
			// Update ping coroutine manually in Edit Mode. (Unity does not run coroutines in Edit Mode)
			if( !Application.isPlaying && _pingEnumerator != null ) OscHelper.UpdateCoroutineInEditMode( _pingEnumerator, ref _lastPingTime );

			// Store name of focused control to detect unfocus events
			_prevControlName = GUI.GetNameOfFocusedControl();
		}


		void UpdateStatusInEditMode()
		{
			switch( _oscOut.mode ){
			case OscSendMode.UnicastToSelf:
				_statusInEditMode = OscRemoteStatus.Connected;
				_pingEnumerator = null;
				break;
			case OscSendMode.Unicast:
				_statusInEditMode = OscRemoteStatus.Unknown;
				if( !Application.isPlaying ) _pingEnumerator = OscHelper.StartCoroutineInEditMode( PingCoroutine(), ref _lastPingTime );
				break;
			case OscSendMode.Multicast:
				_statusInEditMode = OscRemoteStatus.Unknown;
				_pingEnumerator = null;
				break;
			case OscSendMode.Broadcast:
				_statusInEditMode = OscRemoteStatus.Unknown;
				_pingEnumerator = null;
				break;
			}
		}


		void BoolSettingsField( SerializedProperty prop, GUIContent label )
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField( label, GUILayout.Width( 220 ) );
			GUILayout.FlexibleSpace();
			prop.boolValue = EditorGUILayout.Toggle( prop.boolValue, GUILayout.Width( 30 ) );
			EditorGUILayout.EndHorizontal();
		}


		void UnfocusAndUpdateUI()
		{
			GUI.FocusControl("");
			EditorUtility.SetDirty( target );
		}
		
		
		void OnOSCMessage( OscMessage message )
		{
			if( _messageStringQueue.Count >= messageBufferCapacity ) _messageStringQueue.Dequeue();
			_messageStringQueue.Enqueue( message.ToString() );
		}


		Color StatusToColor( OscRemoteStatus status )
		{
			switch( status )
			{
			case OscRemoteStatus.Unknown: return Color.yellow;
			case OscRemoteStatus.Connected: return Color.green;
			case OscRemoteStatus.Disconnected: return Color.red;
			default: return Color.gray;
			}
		}
		

		// This coroutine is only run in Edit Mode.
		IEnumerator PingCoroutine()
		{
			while( true )
			{
				Ping ping = new Ping( _oscOut.remoteIpAddress );
				yield return new WaitForSeconds( pingInterval );
				//Debug.Log( "Ping time " + ping.time );
				_statusInEditMode = ( ping.isDone && ping.time >= 0 ) ? OscRemoteStatus.Connected : OscRemoteStatus.Disconnected;
			}
		}
	}
}