/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace OscSimpl
{
	public static class OscHelper
	{
		static string _cachedLocalIpAddress = OscConst.loopbackAddress;
        static List<string> _cachedLocalIpAddresses = new List<string>();
		static float _lastLocalIpAddressUpdateTime = float.MinValue;
        static float _lastLocalIpAddressAlternativesUpdateTime = float.MinValue;


		/// <summary>
		/// Gets the primary local ip address.
		/// </summary>
		public static string GetLocalIpAddress()
		{
			// If in runtime, then only update every OscConst.localIpUpdateInterval.
			if( Application.isPlaying && Time.time - _lastLocalIpAddressUpdateTime < OscConst.localIpUpdateInterval ) return _cachedLocalIpAddress;

            // To get the primary local IP we connect a UDP socket and read the local end point.
			// https://stackoverflow.com/questions/6803073/get-local-ip-address
			try {
			    // This is a bit slow and generates ~1 KB garbage.
				using( Socket socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, 0 ) ) {
					socket.Connect( "8.8.8.8", 65530 );
					IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
					_cachedLocalIpAddress = endPoint.Address.ToString();
				}
			} catch {
				_cachedLocalIpAddress = OscConst.loopbackAddress;
			}
			_lastLocalIpAddressUpdateTime = Time.time;

			return _cachedLocalIpAddress;
		}


        /// <summary>
		/// Gets the alternative local ip addresses. Your device may have multiple network adapters, for example ethernet and wifi.
		/// </summary>
		public static ReadOnlyCollection<string> GetLocalIpAddressAlternatives()
		{
            // If in runtime, then only update every OscConst.localIpUpdateInterval.
			if( Application.isPlaying && Time.time - _lastLocalIpAddressAlternativesUpdateTime < OscConst.localIpUpdateInterval ) return _cachedLocalIpAddresses.AsReadOnly();

            // Get the primary ip and clear the list of alternatives.
            string _defaultAddress = GetLocalIpAddress();
            _cachedLocalIpAddresses.Clear();

            // Collect the inter networks that are not the primary ip.
			IPHostEntry host = Dns.GetHostEntry( OscConst.hostName );
			foreach( IPAddress ip in host.AddressList ) {
				if( ip.AddressFamily == AddressFamily.InterNetwork ){
                    string address = ip.ToString();
                    if( address != _defaultAddress ) _cachedLocalIpAddresses.Add( address );
                }
			}
            _lastLocalIpAddressAlternativesUpdateTime = Time.time;

            return _cachedLocalIpAddresses.AsReadOnly();
		}


		/// <summary>
		/// Starts a coroutine in Edit Mode. Call UpdateCoroutineInEditMode subsequently on every update.
		/// </summary>
		public static IEnumerator StartCoroutineInEditMode( IEnumerator enumerator, ref DateTime lastPingTime )
		{
			lastPingTime = DateTime.Now;
			return enumerator;
		}

		/// <summary>
		/// Updates a coroutine in Edit Mode. The method currently only supports WaitForSeconds yield instructions.
		/// </summary>
		public static void UpdateCoroutineInEditMode( IEnumerator coroutine, ref DateTime lastPingTime )
		{
			float waitDuration = 0;
			if( coroutine.Current is WaitForSeconds ){
				FieldInfo secondsField = typeof( WaitForSeconds ).GetField( "m_Seconds", BindingFlags.NonPublic | BindingFlags.Instance );
				if( secondsField == null ){
					Debug.LogWarning( "UpdateCoroutineInEditMode failed. Needs update for newer UnityEngine." + Environment.NewLine );
					return;
				}
				waitDuration = (float) secondsField.GetValue( coroutine.Current as WaitForSeconds );
			}
			float secondsElapsed = (float) ( DateTime.Now - lastPingTime ).TotalSeconds;
			if( secondsElapsed > waitDuration ){
				coroutine.MoveNext();
				lastPingTime = DateTime.Now;
			}
		}
	}
}