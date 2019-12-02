/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2019 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text;

namespace OscSimpl
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(OscIn))]
	public class OscInInspector : Editor
	{
		OscIn _oscIn;
		
		SerializedProperty _openOnAwake;
		SerializedProperty _port;
		SerializedProperty _mode;
		SerializedProperty _multicastAddress;
		SerializedProperty _filterDuplicates;
		SerializedProperty _addTimeTagsToBundledMessages;
		SerializedProperty _mappings;
		SerializedProperty _dirtyMappings;
		SerializedProperty _udpBufferSize;
		SerializedProperty _settingsFoldout;
		SerializedProperty _mappingsFoldout;
		SerializedProperty _messagesFoldout;

		string _prevControlName;
		
		int _tempPort;
		string _tempMulticastAddress;
		int _tempBufferSize;

		StringBuilder _sb = new StringBuilder();
		string[] _messageStringBuffer = new string[messageBufferCapacity];
		Queue<string> _messageStringQueue = new Queue<string>( messageBufferCapacity );
		
		// We use reflection to access _inspectorMessageEventObject, to ceil it from users.
		object _inspectorMessageEventObject;

		readonly GUIContent _portLabel = new GUIContent( "Port", "Receiving Port for this computer." );
		readonly GUIContent _modeLabel = new GUIContent( "Receive Mode", "Transmission mode." );
		readonly GUIContent _localIpAddressLabel = new GUIContent( "Local IP Address", "The primary IP address of this device." );
        readonly GUIContent _localIpAddressAlternativesLabel = new GUIContent( "Local IP Alternatives", "Alternative IP addresses of this device (when you have multiple network adapters)." );
		readonly GUIContent _multicastIpAddressLabel = new GUIContent( "Multicast Address", "Multicast group address. Valid range 224.0.0.0 to 239.255.255.255." );
		readonly GUIContent _isOpenLabel = new GUIContent( "Is Open", "Indicates whether this OscIn object is open and ready to receive. In Edit Mode OSC objects are opened and closed automatically by their inspectors" );
		readonly GUIContent _openOnAwakeLabel = new GUIContent( "Open On Awake", "Open this Oscin object automatically when Awake is invoked by Unity (at runtime). The setting is only accessible using the inspector in Edit Mode." );
		readonly GUIContent _filterDuplicatesLabel = new GUIContent( "Filter Duplicates", "Forward only one message per OSC address every Update call. Use the last message received." );
		readonly GUIContent _addTimeTagsToBundledMessagesLabel = new GUIContent( "Add Time Tags To Bundled Messages", "When enabled, timetags from bundles are added to contained messages as last argument." );
		readonly GUIContent _addMappingButtonLabel = new GUIContent( "Add" );
		readonly GUIContent _removeMappingButtonLabel = new GUIContent( "X" );
		readonly GUIContent _udpBufferSizeLabel = new GUIContent( "Udp Buffer Size" );
		readonly GUIContent _settingsFoldLabel = new GUIContent( "Settings" );
		
		const string portControlName = "OscIn Port";
		const string multicastAddressControlName = "OscIn Multicast Ip Address";
		const string bufferSizeControlName = "OscIn Buffer Size";
		const int messageBufferCapacity = 10;

		const string mappingAddressFieldName = "_address";


		void OnEnable()
		{
			_oscIn = target as OscIn;
			
			_openOnAwake = serializedObject.FindProperty("_openOnAwake");
			_port = serializedObject.FindProperty("_port");
			_mode = serializedObject.FindProperty("_mode");
			_multicastAddress = serializedObject.FindProperty("_multicastAddress");
			_filterDuplicates = serializedObject.FindProperty("_filterDuplicates");
			_addTimeTagsToBundledMessages = serializedObject.FindProperty("_addTimeTagsToBundledMessages");
			_mappings = serializedObject.FindProperty("_mappings");
			_dirtyMappings = serializedObject.FindProperty("_dirtyMappings");
			_udpBufferSize = serializedObject.FindProperty( "_udpBufferSize" );
			_settingsFoldout = serializedObject.FindProperty("_settingsFoldout");
			_mappingsFoldout = serializedObject.FindProperty("_mappingsFoldout");
			_messagesFoldout = serializedObject.FindProperty("_messagesFoldout");
			
			// Store socket info for change check workaround.
			_tempPort = _port.intValue;
			_tempMulticastAddress = _multicastAddress.stringValue;
			
			// Ensure that OscIn scripts will be executed early, so it can deliver messages before we compute anything.
			MonoScript script = MonoScript.FromMonoBehaviour( target as MonoBehaviour );
			if( MonoImporter.GetExecutionOrder( script ) != -5000 ) MonoImporter.SetExecutionOrder( script, -5000 );

			// When object is selected in Edit Mode then we start listening.
			if( _oscIn.enabled && !Application.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode && !_oscIn.isOpen ){
				if( _oscIn.mode == OscReceiveMode.UnicastBroadcast ) _oscIn.Open( _oscIn.port );
				else _oscIn.Open( _oscIn.port, _oscIn.multicastAddress );
			}

			// Subscribe to messages.
			OscEditorUI.AddInspectorMessageListener( _oscIn, OnOSCMessage, ref _inspectorMessageEventObject );
		}
		
		
		void OnDisable()
		{
			// When object is deselected in Edit Mode then we stop listening.
			if( !Application.isPlaying && _oscIn.isOpen ) _oscIn.Close();

			// Unsubscribe from messsages.
			OscEditorUI.RemoveInspectorMessageListener( _oscIn, OnOSCMessage, ref _inspectorMessageEventObject );
		}





		public override void OnInspectorGUI()
		{
			Rect rect;
			string currentControlName;

			// Check for key down before drawing any fields because they might consume the event.
			bool enterKeyDown = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
			
			// Load serialized object
			serializedObject.Update();

			// Port field
			EditorGUI.BeginChangeCheck();
			GUI.SetNextControlName( portControlName );
			int newPort = EditorGUILayout.IntField( _portLabel, _oscIn.port );
			if( EditorGUI.EndChangeCheck() ){
				_port.intValue = newPort;
				if( _oscIn.isOpen ) _oscIn.Close(); // Close UDPReceiver while editing
			}
			currentControlName = GUI.GetNameOfFocusedControl();
			bool enterKeyDownPort = enterKeyDown && currentControlName == portControlName;
			if( enterKeyDownPort ) UnfocusAndUpdateUI();
			bool deselect = _prevControlName == portControlName && currentControlName != portControlName;
			if( ( deselect || enterKeyDownPort ) && !_oscIn.isOpen ){
				if( _oscIn.Open( _port.intValue ) ){
					_tempPort = _port.intValue;
				} else {
					_port.intValue = _tempPort; // undo
					_oscIn.Open( _port.intValue );
				}
			}

			// Mode field
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( _mode, _modeLabel );
			if( EditorGUI.EndChangeCheck() ){
				switch( (OscReceiveMode) _mode.enumValueIndex ){
					case OscReceiveMode.UnicastBroadcast:
						_oscIn.Open( _oscIn.port, string.Empty );
						_multicastAddress.stringValue = string.Empty;
						break;
					case OscReceiveMode.UnicastBroadcastMulticast:
						_oscIn.Open( _oscIn.port, OscConst.multicastAddressDefault );
						_multicastAddress.stringValue = OscConst.multicastAddressDefault;
						break;
				}
			}

			// Multicast field
			if( _oscIn.mode == OscReceiveMode.UnicastBroadcastMulticast )
			{
				EditorGUI.BeginChangeCheck();
				GUI.SetNextControlName( multicastAddressControlName );
				EditorGUILayout.PropertyField( _multicastAddress, _multicastIpAddressLabel );
				if( EditorGUI.EndChangeCheck() ){
					if( _oscIn.isOpen ) _oscIn.Close(); // Close socket while editing
				}
				currentControlName = GUI.GetNameOfFocusedControl();
				bool enterKeyDownMulticastIpAddress = enterKeyDown && currentControlName == multicastAddressControlName;
				if( enterKeyDownMulticastIpAddress ) UnfocusAndUpdateUI();
				deselect = _prevControlName == multicastAddressControlName && currentControlName != multicastAddressControlName;
				if( ( deselect || enterKeyDownMulticastIpAddress ) && !_oscIn.isOpen ){
					if( _oscIn.Open( _port.intValue, _multicastAddress.stringValue ) ){
						_tempMulticastAddress = _multicastAddress.stringValue;
					} else {
						_multicastAddress.stringValue = _tempMulticastAddress; // undo
						_oscIn.Open( _port.intValue, _multicastAddress.stringValue );
					}
				}
			}

			// IP Address field.
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel( _localIpAddressLabel );
			EditorGUILayout.LabelField( " " );
			rect = GUILayoutUtility.GetLastRect(); // UI voodoo to position the selectable label perfectly
			EditorGUI.SelectableLabel( rect, OscIn.localIpAddress );
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

            // Alternative IP Addresses field.
            if( OscIn.localIpAddressAlternatives.Count > 0 ) {
                int i = 0;
                foreach( string ip in OscIn.localIpAddressAlternatives ) {
                    EditorGUILayout.BeginVertical();
			        EditorGUILayout.BeginHorizontal();
			        EditorGUILayout.PrefixLabel( new GUIContent( _localIpAddressAlternativesLabel.text + "[" + i + "]", _localIpAddressAlternativesLabel.tooltip ) );
			        EditorGUILayout.LabelField( " " );
			        rect = GUILayoutUtility.GetLastRect(); // UI voodoo to position the selectable label perfectly
			        EditorGUI.SelectableLabel( rect, ip );
			        EditorGUILayout.EndHorizontal();
			        EditorGUILayout.EndVertical();
                    i++;
                }

            }

			// Is Open field
			EditorGUI.BeginDisabledGroup( true );
			EditorGUILayout.Toggle( _isOpenLabel, _oscIn.isOpen );
			EditorGUI.EndDisabledGroup();

			// Open On Awake field
			EditorGUI.BeginDisabledGroup( Application.isPlaying );
			EditorGUILayout.PropertyField( _openOnAwake, _openOnAwakeLabel );
			EditorGUI.EndDisabledGroup();

			EditorGUI.indentLevel++;

			// Settings ...
			_settingsFoldout.boolValue = EditorGUILayout.Foldout( _settingsFoldout.boolValue, _settingsFoldLabel );

			if( _settingsFoldout.boolValue )
			{
				// Filter Duplicates field
				BoolSettingsField( _filterDuplicates, _filterDuplicatesLabel );

				// Add Time Tags To Bundled Messages field.
				BoolSettingsField( _addTimeTagsToBundledMessages, _addTimeTagsToBundledMessagesLabel );

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
				if( enterKeyDownBufferSize || deselect ) {
					if( _tempBufferSize != _udpBufferSize.intValue ) {
						_udpBufferSize.intValue = _tempBufferSize;
						_oscIn.udpBufferSize = _tempBufferSize; // // This will reopen OscIn
					}
				}
			}

			// Mappings ...
			string mappingsFoldLabel = "Mappings (" + _mappings.arraySize + ")";
			_mappingsFoldout.boolValue = EditorGUILayout.Foldout( _mappingsFoldout.boolValue, mappingsFoldLabel );
			if( _mappingsFoldout.boolValue )
			{
				// Mapping elements ..
				int removeIndexRequsted = -1;

				for( int m=0; m<_mappings.arraySize; m++ )
				{
					SerializedProperty mapping = _mappings.GetArrayElementAtIndex( m );

					// Mapping field (using costum property drawer)
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( mapping, true );
					if( EditorGUI.EndChangeCheck() ){
						SerializedProperty address = mapping.FindPropertyRelative( mappingAddressFieldName );
						address.stringValue = GetSanitizedAndUniqueAddress( _mappings, m, address.stringValue );
						_dirtyMappings.boolValue = true;
					}
					
					// Remove mapping button
					rect = GUILayoutUtility.GetLastRect();
					rect = EditorGUI.IndentedRect( GUILayoutUtility.GetLastRect() );
					rect.x = rect.x + rect.width - OscMappingDrawer.removeButtonWidth - 7;
					rect.y += 6;
					rect.width = OscMappingDrawer.removeButtonWidth;
					rect.height = OscMappingDrawer.removeButtonHeight;
					if( GUI.Button( rect, _removeMappingButtonLabel ) ) removeIndexRequsted = m;
				}
				
				// Handle mapping removal ..
				if( removeIndexRequsted != -1 ){
					_mappings.DeleteArrayElementAtIndex( removeIndexRequsted );
					_dirtyMappings.boolValue = true;
				}
				
				// Add mapping button
				rect = EditorGUI.IndentedRect( GUILayoutUtility.GetRect( 20, 30 ) );
				if( GUI.Button( rect, _addMappingButtonLabel ) ){
					_mappings.InsertArrayElementAtIndex( _mappings.arraySize );
					SerializedProperty mapping = _mappings.GetArrayElementAtIndex( _mappings.arraySize-1 );
					SerializedProperty address = mapping.FindPropertyRelative( mappingAddressFieldName );
					address.stringValue = GetSanitizedAndUniqueAddress( _mappings, -1, address.stringValue );
					_dirtyMappings.boolValue = true;
				}
				EditorGUILayout.Space();
			}

			// Messages foldout
			_sb.Length = 0;
			_sb.Append( "Messages (" ); _sb.Append( _oscIn.messageCount ); _sb.Append( ")" );
			GUIContent messagesFoldContent = new GUIContent( _sb.ToString(), "Messages received since last update" );
			_messagesFoldout.boolValue = EditorGUILayout.Foldout( _messagesFoldout.boolValue, messagesFoldContent );
			if( _messagesFoldout.boolValue ){
				_sb.Clear();
				_messageStringQueue.CopyTo( _messageStringBuffer, 0 ); // Copy to array so we can iterate backswards.
				for( int i = _messageStringBuffer.Length-1; i >= 0; i-- ) _sb.AppendLine( _messageStringBuffer[i] );
				EditorGUILayout.HelpBox( _sb.ToString(), MessageType.None );
			}
			
			EditorGUI.indentLevel--;

			// Apply
			serializedObject.ApplyModifiedProperties();
			
			// Request OnInspectorGUI to be called every frame as long as inspector is active
			EditorUtility.SetDirty( target );

			// Store name of focused control to detect unfocus events
			_prevControlName = GUI.GetNameOfFocusedControl();
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
			GUI.FocusControl ("");
			EditorUtility.SetDirty( target );
		}
		
		
		void OnOSCMessage( OscMessage message )
		{
			if( _messageStringQueue.Count >= messageBufferCapacity ) _messageStringQueue.Dequeue();
			_messageStringQueue.Enqueue( message.ToString() );
		}


		string GetSanitizedAndUniqueAddress( SerializedProperty mappingsProp, int mappingIndex, string address )
		{
			// Sanitize
			OscAddress.Sanitize( ref address );

			// Gather all addresses, excluding the one from the mapping we are messing with (if any).
			List<string> addresses = new List<string>();
			for( int m = 0; m < mappingsProp.arraySize; m++ ){
				if( m != mappingIndex ){
					addresses.Add( mappingsProp.GetArrayElementAtIndex( m ).FindPropertyRelative( mappingAddressFieldName ).stringValue );
				}
			}
				
			// Make unique
			if( !addresses.Contains( address ) ) return address;
			string addressWithoutNumber = address.TrimEnd( new char[]{ '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' } );
			for( int i=1; ; i++ )
			{
				string candidateAddress = addressWithoutNumber + i;
				if( !addresses.Contains( candidateAddress ) ) return candidateAddress;
			}
		}
	}
}