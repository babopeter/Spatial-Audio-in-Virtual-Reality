/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2019 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEditor;
using UnityEngine;

namespace OscSimpl
{
	[CustomPropertyDrawer( typeof( OscMapping ) )]
	public class OscMappingDrawer : PropertyDrawer
	{
		public const int removeButtonWidth = 18;
		public const int removeButtonHeight = 15;
		const int messageTypeDropdownWidth = 125;
		public const int fieldHeight = 17;

		const int horizontalBoxPadding = 3;
		const int verticalBoxPadding = 3;

		const int horizontalFieldPadding = 8;


		public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
		{
			SerializedProperty type = property.FindPropertyRelative( "type" );
			SerializedProperty handler = GetHandler( property, type.enumValueIndex );
			return EditorGUI.GetPropertyHeight( handler ) + verticalBoxPadding * 2 - 2;
		}
		
		
		public override void OnGUI( Rect rect, SerializedProperty property, GUIContent label )
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty( rect, label, property );
			
			// Get properties.
			SerializedProperty type = property.FindPropertyRelative( "type" );
			SerializedProperty address = property.FindPropertyRelative( "_address" );
			SerializedProperty handler = GetHandler( property, type.enumValueIndex );

			// Apply indentation.
			rect = EditorGUI.IndentedRect( rect );

			// Draw background.
			EditorGUI.DrawRect( rect, OscEditorUI.boxColor );

			// Adjust rect and store.
			rect.yMin += verticalBoxPadding;
			rect.yMax -= verticalBoxPadding;
			rect.xMin += horizontalBoxPadding;
			rect.xMax -= horizontalBoxPadding;
			Rect area = rect;
			float handlerHeight = EditorGUI.GetPropertyHeight( handler );

			// Check area.
			//EditorGUI.DrawRect( area, Color.red );

			// Draw event handler.
			rect.y -= 1;
			rect.xMin -= 1;
			rect.xMax += 2;
			rect.height = handlerHeight;
			EditorGUI.PropertyField( rect, handler );

			// Draw a rect covering the header of the event handler.
			rect.xMin += 1;
			rect.xMax -= 2;
			rect.y += 2;
			rect.height = 21;
			EditorGUI.DrawRect( rect, OscEditorUI.eventHandlerHeaderColor );

			// Draw address field.
			rect = area;
			rect.xMin -= 10;
			rect.y += verticalBoxPadding;
			rect.height = fieldHeight;
			rect.xMax -= messageTypeDropdownWidth + removeButtonWidth;
			EditorGUI.BeginChangeCheck();
			string newString = EditorGUI.TextField( rect, address.stringValue );
			if( EditorGUI.EndChangeCheck() ){
				address.stringValue = newString;
			}

			// Draw OscMessageType dropdown.
			rect = area;
			rect.y += verticalBoxPadding;
			rect.height = fieldHeight;
			rect.xMax -= removeButtonWidth + horizontalFieldPadding;
			rect.xMin = rect.xMax - messageTypeDropdownWidth;
			EditorGUI.BeginChangeCheck();
			int newEnumIndex = (int) (OscMessageType) EditorGUI.EnumPopup( rect, (OscMessageType) type.enumValueIndex );
			if( EditorGUI.EndChangeCheck() ){
				type.enumValueIndex = newEnumIndex;
			}

			EditorGUI.EndProperty ();
		}

		
		SerializedProperty GetHandler( SerializedProperty property, int typeIndex )
		{
			switch( typeIndex ){
				case 0: return property.FindPropertyRelative( OscMessageType.OscMessage + "Handler" );
				case 1: return property.FindPropertyRelative( OscMessageType.Float + "Handler" );
				case 2: return property.FindPropertyRelative( OscMessageType.Double + "Handler" );
				case 3: return property.FindPropertyRelative( OscMessageType.Int + "Handler" );
				case 4: return property.FindPropertyRelative( OscMessageType.Long + "Handler" );
				case 5: return property.FindPropertyRelative( OscMessageType.String + "Handler" );
				case 6: return property.FindPropertyRelative( OscMessageType.Char + "Handler" );
				case 7: return property.FindPropertyRelative( OscMessageType.Bool + "Handler" );
				case 8: return property.FindPropertyRelative( OscMessageType.Color + "Handler" );
				case 9: return property.FindPropertyRelative( OscMessageType.Blob + "Handler" );
				case 10: return property.FindPropertyRelative( OscMessageType.TimeTag + "Handler" );
				case 11: return property.FindPropertyRelative( OscMessageType.Midi + "Handler" );
				case 12: return property.FindPropertyRelative( OscMessageType.ImpulseNullEmpty + "Handler" );
			}
			return null;
		}
	}
}