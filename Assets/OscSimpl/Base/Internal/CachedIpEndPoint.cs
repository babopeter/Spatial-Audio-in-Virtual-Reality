/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk

	IPEndPoint.Serialize generates garbage. It called by Socket.SendTo (which is used by UdpClient.Send).
	This is a workaround that caches the resulting SocketAddress to avoid successive called to Serialize.
*/

using System.Net;

namespace OscSimpl
{
	public class CachedIpEndPoint : IPEndPoint
	{
		SocketAddress _cachedSocketAddress;
		
		
		public CachedIpEndPoint( long address, int port ) : base( address, port ){}
		
		
		public CachedIpEndPoint( IPAddress address, int port ) : base( address, port ){}
		
		
		public override SocketAddress Serialize()
		{
			if( _cachedSocketAddress == null ) _cachedSocketAddress = base.Serialize();
			return _cachedSocketAddress;
		}
	}
}