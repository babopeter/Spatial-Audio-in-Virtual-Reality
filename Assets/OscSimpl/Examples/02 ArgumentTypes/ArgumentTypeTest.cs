/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

namespace OscSimpl.Examples
{
	public class ArgumentTypeTest : MonoBehaviour
	{
		public OscIn oscIn;
		public OscOut oscOut;

		public Text outputHeaderLabel;
		public Text inputHeaderLabel;

		public Text floatOutputLabel;
		public Text doubleOutputLabel;
		public Text intOutputLabel;
		public Text longOutputLabel;
		public Text boolOutputLabel;
		public Image colorOutputImage;
		public InputField stringOutputField;
		public InputField charOutputField;
		public InputField timetagYearOutputField;
		public InputField timetagMonthOutputField;
		public InputField timetagDayOutputField;
		public InputField timetagHourOutputField;
		public InputField timetagMinuteOutputField;
		public InputField timetagSecondOutputField;
		public InputField timetagMillisecondOutputField;
		public RawImage blobOutputRawImage;
		public InputField midiPortOutputField;
		public InputField midiStatusOutputField;
		public InputField midiData1OutputField;
		public InputField midiData2OutputField;

		public Slider floatInputSlider;
		public Slider doubleInputSlider;
		public Slider intInputSlider;
		public Slider longInputSlider;
		public InputField stringInputField;
		public InputField charInputField;
		public Toggle boolInputToggle;
		public Image colorInputImage;
		public RawImage blobInputRawImage;
		public InputField timetagYearInputField;
		public InputField timetagMonthInputField;
		public InputField timetagDayInputField;
		public InputField timetagHourInputField;
		public InputField timetagMinuteInputField;
		public InputField timetagSecondInputField;
		public InputField timetagMillisecondInputField;
		public Toggle timetagImmediateIinputToggle;
		public Image impulseInputImage;
		public Image nullInputImage;
		public Image emptyInputImage;
		public InputField midiPortInputField;
		public InputField midiStatusInputField;
		public InputField midiData1InputField;
		public InputField midiData2InputField;

		public Text floatInputLabel;
		public Text doubleInputLabel;
		public Text intInputLabel;
		public Text longInputLabel;
		public Text boolInputLabel;

		Texture2D _blobInputTexture;
		Texture2D _blobOutputTexture;
        StringBuilder _sb;
		OscTimeTag _timetag;
		Color _defaultColor;
		float _hue;
		OscMidiMessage _midiMessage;

		const string floatAddress = "/test/float";
		const string doubleAddress = "/test/double";
		const string intAddress = "/test/int";
		const string longAddress = "/test/long";
		const string stringAddress = "/test/string";
		const string charAddress = "/test/char";
		const string boolAddress = "/test/bool";
		const string colorAddress = "/test/color";
		const string blobAddress = "/test/blob";
		const string timetagAddress = "/test/timetag";
		const string impulseAddress = "/test/impulse";
		const string nullAddress = "/test/null";
		const string emptyAddress = "/test/empty";
		const string midiAddress = "/test/midi";


		void Awake()
		{
			_defaultColor = emptyInputImage.color;
			_timetag = new OscTimeTag( new System.DateTime( 1900, 1, 1 ) );
			_midiMessage = new OscMidiMessage();
			_sb = new StringBuilder();
		}


		void Update()
		{
			_sb.Clear();
			_sb.Append( "OUTPUT (port: " ); _sb.Append( oscOut.port );
			_sb.Append( ", address: " ); _sb.Append( oscOut.remoteIpAddress ); _sb.Append( ")" );
			outputHeaderLabel.text = _sb.ToString();

			_sb.Clear();
			_sb.Append( "INPUT port: " ); _sb.Append( oscIn.port );
			inputHeaderLabel.text = _sb.ToString();
		}



		#region send methods

		// The following methods are meant to be linked to Unity's runtime UI from the Unity Editor.


		public void SendFloat( float value )
		{
			float floatValue = value * 2 - 1;
			oscOut.Send( floatAddress, floatValue );
			floatOutputLabel.text = floatValue.ToString();
		}


		public void SendDouble( float value )
		{
			double doubleValue = value * 2d - 1d;
			oscOut.Send( doubleAddress, doubleValue );
			doubleOutputLabel.text = doubleValue.ToString();
		}


		public void SendInt( float value )
		{
			int intValue = (int) ( int.MaxValue * (double) (value*2-1) );
			oscOut.Send( intAddress, intValue );
			intOutputLabel.text = intValue.ToString();
		}


		public void SendLong( float value )
		{
			long longValue = (long) ( long.MaxValue * (decimal) (value*2-1) );
			oscOut.Send( longAddress, longValue );
			longOutputLabel.text = longValue.ToString();
		}


		public void SendString( string value )
		{
			oscOut.Send( stringAddress, value );

			// We update the output UI to reflect that OSC chars are always ASCII.
			stringOutputField.text = Encoding.ASCII.GetString( Encoding.ASCII.GetBytes( value ) );
		}


		public void SendChar( string value )
		{
			if( value.Length == 0 ) return;

			char charValue = value[0];
			oscOut.Send( charAddress, charValue );

			// We update the output UI to reflect that OSC chars are always ASCII.
			charOutputField.text = Encoding.ASCII.GetString( Encoding.ASCII.GetBytes( value ) );
		}


		public void SendBool( bool value )
		{
			oscOut.Send( boolAddress, value );
			boolOutputLabel.text = value.ToString();
		}


		public void SendColor()
		{
			_hue = ( _hue + 0.2f ) % 1f;
			Color32 color = Color.HSVToRGB( _hue, 0.3f, 1 );
			colorOutputImage.color = color;
			oscOut.Send( colorAddress, color );
		}


		public void GenerateAndSendBlob()
		{
			int size = 16;
			Color32[] pixels = new Color32[size*size];
			for( int p=0; p<pixels.Length; p++ ) pixels[p] = new Color32( (byte) (int) (Random.value*255), (byte) (int) (Random.value*255), (byte) (int) (Random.value*255), 255 );
			if( _blobOutputTexture == null ) _blobOutputTexture = new Texture2D( size, size, TextureFormat.ARGB32, false );
			_blobOutputTexture.SetPixels32( pixels );
			_blobOutputTexture.Apply();
			blobOutputRawImage.texture = _blobOutputTexture;
			byte[] blob = _blobOutputTexture.EncodeToPNG();

			oscOut.Send( blobAddress, blob );
		}


		public void SendTimeTagYear( string stringValue )
		{
			if( stringValue.Length == 0 ) stringValue = "0";
			int year = Mathf.Clamp( System.Convert.ToInt32( stringValue ), 1, 9999 );
			timetagYearOutputField.text = year.ToString();
			System.DateTime time = _timetag.time;
			time = new System.DateTime( year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Millisecond );
			_timetag.time = time;
			oscOut.Send( timetagAddress, _timetag );
		}


		public void SendTimeTagMonth( string stringValue )
		{
			if( stringValue.Length == 0 ) stringValue = "0";
			int month = Mathf.Clamp( System.Convert.ToInt32( stringValue ), 1, 12 );
			timetagMonthOutputField.text = month.ToString();
			System.DateTime time = _timetag.time;
			time = new System.DateTime( time.Year, month, time.Day, time.Hour, time.Minute, time.Second, time.Millisecond );
			_timetag.time = time;
			oscOut.Send( timetagAddress, _timetag );
		}


		public void SendTimeTagDay( string stringValue )
		{
			if( stringValue.Length == 0 ) stringValue = "0";
			int day = Mathf.Clamp( System.Convert.ToInt32( stringValue ), 1, 31 );
			timetagDayOutputField.text = day.ToString();
			System.DateTime time = _timetag.time;
			time = new System.DateTime( time.Year, time.Month, day, time.Hour, time.Minute, time.Second, time.Millisecond );
			_timetag.time = time;
			oscOut.Send( timetagAddress, _timetag );
		}


		public void SendTimeTagHour( string stringValue )
		{
			if( stringValue.Length == 0 ) stringValue = "0";
			int hour = Mathf.Clamp( System.Convert.ToInt32( stringValue ), 0, 23 );
			timetagHourOutputField.text = hour.ToString();
			System.DateTime time = _timetag.time;
			time = new System.DateTime( time.Year, time.Month, time.Month, hour, time.Minute, time.Second, time.Millisecond );
			_timetag.time = time;
			oscOut.Send( timetagAddress, _timetag );
		}


		public void SendTimeTagMinute( string stringValue )
		{
			if( stringValue.Length == 0 ) stringValue = "0";
			int minute = Mathf.Clamp( System.Convert.ToInt32( stringValue ), 0, 59 );
			timetagMinuteOutputField.text = minute.ToString();
			System.DateTime time = _timetag.time;
			time = new System.DateTime( time.Year, time.Month, time.Month, time.Day, minute, time.Second, time.Millisecond );
			_timetag.time = time;
			oscOut.Send( timetagAddress, _timetag );
		}


		public void SendTimeTagSecond( string stringValue )
		{
			if( stringValue.Length == 0 ) stringValue = "0";
			int second = Mathf.Clamp( System.Convert.ToInt32( stringValue ), 0, 59 );
			timetagSecondOutputField.text = second.ToString();
			System.DateTime time = _timetag.time;
			time = new System.DateTime( time.Year, time.Month, time.Month, time.Day, time.Minute, second, time.Millisecond );
			_timetag.time = time;
			oscOut.Send( timetagAddress, _timetag );
		}


		public void SendTimeTagMillisecond( string stringValue )
		{
			if( stringValue.Length == 0 ) stringValue = "0";
			int millisecond = Mathf.Clamp( System.Convert.ToInt32( stringValue ), 0, 999 );
			timetagMillisecondOutputField.text = millisecond.ToString();
			System.DateTime time = _timetag.time;
			time = new System.DateTime( time.Year, time.Month, time.Month, time.Day, time.Minute, time.Second, millisecond );
			_timetag.time = time;
			oscOut.Send( timetagAddress, _timetag );
		}


		public void SendTimeTagImmediate( bool state )
		{
			_timetag.immediately = state;
			oscOut.Send( timetagAddress, _timetag );
		}


		public void SendImpulse()
		{
			oscOut.Send( impulseAddress, new OscImpulse() );
		}


		public void SendNull()
		{
			oscOut.Send( nullAddress, new OscNull() );
		}


		public void SendEmpty()
		{
			oscOut.Send( emptyAddress );
		}


		public void SendMidiMessagePort( string stringValue )
		{
			int value = string.IsNullOrEmpty( stringValue ) ? 0 : Mathf.Clamp( System.Convert.ToInt32( stringValue ), 0, 255 );
			midiPortOutputField.text = value.ToString();
			_midiMessage.port = (byte) value;
			oscOut.Send( midiAddress, _midiMessage );
		}


		public void SendMidiMessageStatus( string stringValue )
		{
			int value = string.IsNullOrEmpty( stringValue ) ? 0 : Mathf.Clamp( System.Convert.ToInt32( stringValue ), 0, 255 );
			midiStatusOutputField.text = value.ToString();
			_midiMessage.status = (byte) value;
			oscOut.Send( midiAddress, _midiMessage );
		}


		public void SendMidiMessageData1( string stringValue )
		{
			int value = string.IsNullOrEmpty( stringValue ) ? 0 : Mathf.Clamp( System.Convert.ToInt32( stringValue ), 0, 255 );
			midiData1OutputField.text = value.ToString();
			_midiMessage.data1 = (byte) value;
			oscOut.Send( midiAddress, _midiMessage );
		}


		public void SendMidiMessageData2( string stringValue )
		{
			int value = string.IsNullOrEmpty( stringValue ) ? 0 : Mathf.Clamp( System.Convert.ToInt32( stringValue ), 0, 255 );
			midiData2OutputField.text = value.ToString();
			_midiMessage.data2 = (byte) value;
			oscOut.Send( midiAddress, _midiMessage );
		}

		#endregion



		#region receive methods

		// The following methods are meant to be set up as "mappings" in the inspector panel of an OscIn object.

		public void OnReceiveFloat( float value )
		{
			floatInputSlider.value = value * 0.5f + 0.5f;
			floatInputLabel.text = value.ToString();
		}


		public void OnReceiveDouble( double value )
		{
			doubleInputSlider.value = (float) ( value * 0.5d + 0.5d );
			doubleInputLabel.text = value.ToString();
		}


		public void OnReceiveInt( int value )
		{
			intInputSlider.value = Mathf.InverseLerp( int.MinValue, int.MaxValue, value );
			intInputLabel.text = value.ToString();
		}


		public void OnReceiveLong( long value )
		{
			longInputSlider.value = (float) ( ( value / (double) long.MaxValue ) * 0.5f + 0.5f );
			longInputLabel.text = value.ToString();
		}


		public void OnReceiveString( string value )
		{
			stringInputField.text = value;
		}


		public void OnReceiveChar( char value )
		{
			charInputField.text = value.ToString();
		}


		public void OnReceiveBool( bool value )
		{
			boolInputToggle.isOn = value;
			boolInputLabel.text = value.ToString();
		}


		public void OnReceiveColor( Color32 value )
		{
			colorInputImage.color = value;
		}


		public void OnReceiveBlob( byte[] value )
		{
			// Presuming we are receiving a image in png or jpeg format.
			if( _blobInputTexture == null ) _blobInputTexture = new Texture2D(2,2);
			_blobInputTexture.LoadImage( value );
			blobInputRawImage.texture = _blobInputTexture;
		}


		public void OnReceiveTimeTag( OscTimeTag timetag )
		{
			timetagYearInputField.text = timetag.time.Year.ToString();
			timetagMonthInputField.text = timetag.time.Month.ToString();
			timetagDayInputField.text = timetag.time.Day.ToString();
			timetagHourInputField.text = timetag.time.Hour.ToString();
			timetagMinuteInputField.text = timetag.time.Minute.ToString();
			timetagSecondInputField.text = timetag.time.Second.ToString();
			timetagMillisecondInputField.text = timetag.time.Millisecond.ToString();
			timetagImmediateIinputToggle.isOn = timetag.immediately;
		}


		public void OnReceiveImpulse()
		{
			StartCoroutine( FlashImageCoroutine( impulseInputImage ) );
		}


		public void OnReceiveNull()
		{
			StartCoroutine( FlashImageCoroutine( nullInputImage ) );
		}


		public void OnReceiveEmpty()
		{
			StartCoroutine( FlashImageCoroutine( emptyInputImage ) );
		}


		public void OnReceiveMidi( OscMidiMessage message )
		{
			midiPortInputField.text = message.port.ToString();
			midiStatusInputField.text = message.status.ToString();
			midiData1InputField.text = message.data1.ToString();
			midiData2InputField.text = message.data2.ToString();
		}

		#endregion


		IEnumerator FlashImageCoroutine( Image image )
		{
			float startTime = Time.time;
			float timeElapsed = 0;
			float duration = 0.2f;
			while( timeElapsed < duration )
			{
				yield return 0;
				image.color = Color.Lerp( Color.black, _defaultColor, timeElapsed / duration );
				timeElapsed = Time.time - startTime;
			}
			image.color = _defaultColor;
		}

	}
}