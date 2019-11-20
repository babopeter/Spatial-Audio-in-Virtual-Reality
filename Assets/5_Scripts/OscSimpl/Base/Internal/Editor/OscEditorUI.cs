/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2019 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;


namespace OscSimpl
{
	public static class OscEditorUI
	{
		public static Color boxColor = EditorGUIUtility.isProSkin ? new Color( 0.26f, 0.26f, 0.26f, 1 ) : new Color( 0.65f, 0.65f, 0.65f, 1 );
		public static Color32 eventHandlerHeaderColor = EditorGUIUtility.isProSkin ? new Color32( 93, 93, 93, 255 ) : new Color32( 95, 95, 95, 255 );

		// All this reflection stuf is to avoid exposing _inspectorMessageEvent to the user.
		static FieldInfo _inspectorMessageEventInfo;
		static MethodInfo _addListenerInfo;
		static MethodInfo _removeListenerInfo;


		public static void AddInspectorMessageListener( OscMonoBase oscBase, UnityAction<OscMessage> method, ref object inspectorMessageEventObject )
		{

			GetReflectionAccessForInspector( oscBase, method, ref inspectorMessageEventObject );
			_addListenerInfo.Invoke( inspectorMessageEventObject, new object[] { method.Target, method.Method } );
		}


		public static void RemoveInspectorMessageListener( OscMonoBase oscBase, UnityAction<OscMessage> method, ref object inspectorMessageEventObject )
		{
			GetReflectionAccessForInspector( oscBase, method, ref inspectorMessageEventObject );
			_removeListenerInfo.Invoke( inspectorMessageEventObject, new object[] { method.Target, method.Method } );

		}

		static void GetReflectionAccessForInspector( OscMonoBase oscBase, UnityAction<OscMessage> method, ref object inspectorMessageEventObject )
		{
			if( _inspectorMessageEventInfo == null ) _inspectorMessageEventInfo = typeof( OscMonoBase ).GetField( "_inspectorMessageEvent", BindingFlags.NonPublic | BindingFlags.Instance );
			if( _addListenerInfo == null ) _addListenerInfo = typeof( UnityEventBase ).GetMethod( "AddListener", BindingFlags.NonPublic | BindingFlags.Instance );
			if( _removeListenerInfo == null ) _removeListenerInfo = typeof( UnityEventBase ).GetMethod( "RemoveListener", BindingFlags.NonPublic | BindingFlags.Instance );
			if( inspectorMessageEventObject == null ) inspectorMessageEventObject = _inspectorMessageEventInfo.GetValue( oscBase );
		}
	}
}
