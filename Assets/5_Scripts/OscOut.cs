/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2019 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using OscSimpl;

/// <summary>
/// MonoBehaviour for sending OscPackets. The "OSC Client".
/// </summary>
public class OscOut : OscMonoBase
{
    [SerializeField] int _port = 8000;
    [SerializeField] OscSendMode _mode = OscSendMode.UnicastToSelf;
    [SerializeField, FormerlySerializedAs("_ipAddress")] string _remoteIpAddress = IPAddress.Loopback.ToString(); // 127.0.0.1;
    [SerializeField] OscRemoteStatus _remoteStatus = OscRemoteStatus.Unknown;
    [SerializeField] bool _multicastLoopback = true; // UdpClient default is true
    [SerializeField] bool _bundleMessagesOnEndOfFrame = false;
    [SerializeField] bool _splitBundlesAvoidingBufferOverflow = true;

    UdpClient _udpClient;
    IPEndPoint _endPoint;

    byte[] _cache;

    bool _wasClosedOnDisable; // Flag to indicate if we should open UDPTransmitter OnEnable.

    Coroutine _endOfFrameCoroutine;
    WaitForEndOfFrame _endOfFrameYield = new WaitForEndOfFrame();
    IEnumerator _pingCoroutine;
    WaitForSeconds _pingYield = new WaitForSeconds(_pingInterval);
    SerializedOscMessageBuffer _endOfFrameBuffer;
    OscTimeTag _endOfFrameTimeTag = new OscTimeTag();

    Queue<OscMessage> _tempMessageQueue = new Queue<OscMessage>();

    DateTime _dateTime;

    // For the inspector.
#if UNITY_EDITOR
    [SerializeField] bool _settingsFoldout;
    [SerializeField] bool _messagesFoldout;
#endif

    const float _pingInterval = 1f; // Seconds

    /// <summary>
    /// Gets the port to be send to on the target remote device (read only).
    /// To set, call the Open method.
    /// </summary>
    public int port { get { return _port; } }

    /// <summary>
    /// Gets the transmission mode (read only). Can either be UnicastToSelf, Unicast, Broadcast or Multicast.
    /// The mode is automatically derived from the IP address passed to the Open method.
    /// </summary>
    public OscSendMode mode { get { return _mode; } }

    /// <summary>
    /// Gets the IP address of the target remote device (read only). To set, call the 'Open' method.
    /// </summary>
    public string remoteIpAddress { get { return _remoteIpAddress; } }

    /// <summary>
    /// Indicates whether the Open method has been called and the object is ready to send.
    /// </summary>
    public bool isOpen { get { return _udpClient != null && _udpClient.Client != null; } }

    /// <summary>
    /// Gets the remote connection status (read only). Can either be Connected, Disconnected or Unknown.
    /// </summary>
    public OscRemoteStatus remoteStatus { get { return _remoteStatus; } }

    /// <summary>
    /// Gets the number of messages send since last update.
    /// </summary>
    public int messageCount { get { return _messageCount; } }

    /// <summary>
    /// Indicates whether outgoing multicast messages are also delivered to the sending application.
    /// Default is true.
    /// </summary>
    public bool multicastLoopback
    {
        get { return _multicastLoopback; }
        set
        {
            _multicastLoopback = value;
            // Re-open.
            if (isOpen && _mode == OscSendMode.Multicast) Open(_port, _remoteIpAddress);
        }
    }

    /// <summary>
    /// When enabled, all messages will be queued and send at end of the frame (i.e. Unity's WaitForEndOfFrame),
    /// packed into OscBundles that stay below a safe byte size (see OscHelper.bufferSizeSafetyLimit for the 
    /// actual limit). This option is recommended for better performance. Default is false (because not all
    /// OSC libaries supports bundles well).
    /// </summary>
    public bool bundleMessagesOnEndOfFrame
    {
        get { return _bundleMessagesOnEndOfFrame; }
        set
        {
            _bundleMessagesOnEndOfFrame = value;
            if (!value)
            {
                _endOfFrameBuffer.Clear();
            }
        }
    }


    /// <summary>
    /// When enabled, bundles will be split into multiple bundle if they exceed the UDP buffer size. Default is true.
    /// If you diable this, then make sure that udpBufferSize is large enough for your packets.
    /// </summary>
    public bool splitBundlesAvoidingBufferOverflow
    {
        get { return _splitBundlesAvoidingBufferOverflow; }
        set { _splitBundlesAvoidingBufferOverflow = value; }
    }


    /// <summary>
    /// Gets or sets the size of the UDP buffer.
    /// </summary>
    public int udpBufferSize
    {
        get { return _udpBufferSize; }
        set
        {
            int newBufferSize = Mathf.Clamp(value, OscConst.udpBufferSizeMin, OscConst.udpBufferSizeMax);
            if (newBufferSize != _udpBufferSize)
            {
                _udpBufferSize = newBufferSize;
                if (isOpen) Open(_port, _remoteIpAddress);
            }
        }
    }


    void Awake()
    {
        _endOfFrameBuffer = new SerializedOscMessageBuffer(_udpBufferSize);
        if (enabled && Application.isPlaying && _openOnAwake) Open(_port, _remoteIpAddress);
    }


    // OnEnable is only called when Application.isPlaying.
    void OnEnable()
    {
        if (!isOpen && _wasClosedOnDisable) Open(_port, _remoteIpAddress);
    }


    // OnEnable is only called when Application.isPlaying.
    void OnDisable()
    {
        if (isOpen)
        {
            Close();
            _wasClosedOnDisable = true;
        }
        _remoteStatus = OscRemoteStatus.Unknown;

        if (_endOfFrameCoroutine != null) StopCoroutine(_endOfFrameCoroutine);
    }


    void Update()
    {
        // Reset message count.
        // OscOut is set to -500o in ScriptExecutionOrder, 
        // so we can assume that no messages has been send 
        // at this poing.
        _messageCount = 0;

        // Since DateTime.Now is slow, we just create it once and update it with unity time.
        if (_dateTime.Ticks == 0) _dateTime = DateTime.Now;
        else _dateTime = _dateTime.AddSeconds(Time.deltaTime);

        // Coroutines only work at runtime.
        if (Application.isPlaying)
        {
            if (_bundleMessagesOnEndOfFrame)
            {
                if (_endOfFrameCoroutine == null) _endOfFrameCoroutine = StartCoroutine(SendBundleOnEndOfFrame());
            }
            else
            {
                if (_endOfFrameCoroutine != null) StopCoroutine(_endOfFrameCoroutine);
            }
        }
    }


    void OnDestroy()
    {
        if (isOpen) Close();
    }


    /// <summary>
    /// Open to send messages to specified port and (optional) IP address.
    /// If no IP address is given, messages will be send locally on this device.
    /// Returns success status.
    /// </summary>
    public bool Open(int port, string remoteIpAddress = "")
    {
        // TODO this should be moved to a UdpSender class.

        // Close and stop pinging.
        if (_udpClient != null) Close();

        // Validate IP.
        IPAddress ip;
        if (string.IsNullOrEmpty(remoteIpAddress)) remoteIpAddress = IPAddress.Loopback.ToString();
        if (remoteIpAddress == IPAddress.Any.ToString() || !IPAddress.TryParse(remoteIpAddress, out ip))
        {
            StringBuilder sb = OscDebug.BuildText(this);
            sb.Append("Open failed. Invalid IP address "); sb.Append(remoteIpAddress); sb.Append(".\n");
            Debug.LogWarning(sb.ToString());
            return false;
        }
        if (ip.AddressFamily != AddressFamily.InterNetwork)
        {
            StringBuilder sb = OscDebug.BuildText(this);
            sb.Append("Open failed. Only IPv4 addresses are supported. "); sb.Append(remoteIpAddress);
            sb.Append(" is "); sb.Append(ip.AddressFamily); sb.Append(".\n");
            Debug.LogWarning(sb.ToString());
            return false;
        }
        _remoteIpAddress = remoteIpAddress;

        // Detect and set transmission mode.
        if (_remoteIpAddress == IPAddress.Loopback.ToString())
        {
            _mode = OscSendMode.UnicastToSelf;
        }
        else if (_remoteIpAddress == IPAddress.Broadcast.ToString())
        {
            _mode = OscSendMode.Broadcast;
        }
        else if (Regex.IsMatch(_remoteIpAddress, OscConst.multicastAddressPattern))
        {
            _mode = OscSendMode.Multicast;
        }
        else
        {
            _mode = OscSendMode.Unicast;
        }

        // Validate port number range
        if (port < OscConst.portMin || port > OscConst.portMax)
        {
            StringBuilder sb = OscDebug.BuildText(this);
            sb.Append("Open failed. Port "); sb.Append(port); sb.Append(" is out of range.\n");
            Debug.LogWarning(sb.ToString());
            return false;
        }
        _port = port;

        // Create new client and end point.
        _udpClient = new UdpClient();
        _endPoint = new CachedIpEndPoint(ip, _port);

        // Multicast senders do not need to join a multicast group, but we need to set a few options.
        if (_mode == OscSendMode.Multicast)
        {
            // Set a time to live, indicating how many routers the messages is allowed to be forwarded by.
            _udpClient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, OscConst.timeToLive);

            // Apply out multicastLoopback field.
            _udpClient.MulticastLoopback = _multicastLoopback;
        }

        // Set time to live to max. I haven't observed any difference, but we better be safe.
        _udpClient.Ttl = OscConst.timeToLive;

        // If an outgoing packet happen to exceeds the MTU (Maximum Transfer Unit) then throw an error instead of fragmenting.
        _udpClient.DontFragment = true;

        // Set buffer size.
        _udpClient.Client.SendBufferSize = _udpBufferSize;

        // DO NOT CONNECT UDP CLIENT!
        //_udpClient.Connect( _endPoint );

        // Note to self about connecting UdpClient:
        // We do not use udpClient.Connect(). Instead we pass the IPEndPoint directly to _udpClient.Send().
        // This is because a connected UdpClient purposed for sending will throw a number of (for our use) unwanted exceptions and in some cases disconnect.
        //		10061: SocketException: Connection refused 									- When we attempt to unicast to loopback address when no application is listening.
        //		10049: SocketException: The requested address is not valid in this context	- When we attempt to broadcast while having no access to the local network.
        //		10051: SocketException: Network is unreachable								- When we pass a unicast or broadcast target to udpClient.Connect() while having no access to a network.

        // Handle pinging
        if (Application.isPlaying)
        {
            _remoteStatus = _mode == OscSendMode.UnicastToSelf ? OscRemoteStatus.Connected : OscRemoteStatus.Unknown;
            if (_mode == OscSendMode.Unicast)
            {
                _pingCoroutine = PingCoroutine();
                StartCoroutine(_pingCoroutine);
            }
        }

        // Create cache buffer.
        if (_cache == null || _cache.Length != _udpBufferSize) _cache = new byte[_udpBufferSize];

        // Log
        if (Application.isPlaying)
        {
            string addressTypeString = string.Empty;
            switch (_mode)
            {
                case OscSendMode.Broadcast: addressTypeString = "broadcast"; break;
                case OscSendMode.Multicast: addressTypeString = "multicast"; break;
                case OscSendMode.Unicast: addressTypeString = "IP"; break;
                case OscSendMode.UnicastToSelf: addressTypeString = "local"; break;
            }

            StringBuilder sb = OscDebug.BuildText(this);
            sb.Append("Ready to send to "); sb.Append(addressTypeString);
            sb.Append(" address "); sb.Append(_remoteIpAddress);
            sb.Append(" on port "); sb.Append(port); sb.Append(".\n");
            Debug.Log(sb.ToString());
        }

        return true;
    }


    /// <summary>
    /// Close and stop sending messages.
    /// </summary>
    public void Close()
    {
        if (_pingCoroutine != null)
        {
            StopCoroutine(_pingCoroutine);
            _pingCoroutine = null;
        }
        _remoteStatus = OscRemoteStatus.Unknown;

        if (_udpClient == null) return;
        _udpClient.Close();
        _udpClient = null;

        _wasClosedOnDisable = false;
    }


    /// <summary>
    /// Send an OscMessage or OscBundle. Data is serialized and no reference is stored, so you can safely 
    /// change values and send packet immediately again.
    /// Returns success status. 
    /// </summary>
    public bool Send(OscPacket packet)
    {
        if (!isOpen) return false;

        // On any message.
        InvokeAnyMessageEventRecursively(packet);

        // Bundle at end of frame case.
        if (_bundleMessagesOnEndOfFrame && packet is OscMessage)
        {
            _endOfFrameBuffer.Add(packet as OscMessage);
            return true; // Assume success.
        }

        // Split bundle case.
        if (_splitBundlesAvoidingBufferOverflow && packet is OscBundle && packet.Size() > _udpBufferSize)
        {
            ExtractMessages(packet, _tempMessageQueue);
            int bundleByteCount = OscConst.bundleHeaderSize;
            OscBundle splitBundle = OscPool.GetBundle();
            while (_tempMessageQueue.Count > 0)
            {
                OscMessage message = _tempMessageQueue.Dequeue();
                // Check if message is too big.
                int messageSize = message.Size() + FourByteOscData.byteCount; // Bundle stores size of each message in a 4 byte integer.
                if (messageSize > _udpBufferSize)
                {
                    StringBuilder sb = OscDebug.BuildText(this);
                    sb.Append("Failed to send message. Message size at "); sb.Append(messageSize);
                    sb.Append(" bytes exceeds udp buffer size at "); sb.Append(_udpBufferSize);
                    sb.Append(" bytes. Try increasing the buffer size.'\n");
                    Debug.LogWarning(sb.ToString());
                    return false;
                }
                // If bundle is full, send it and prepare for new bundle.
                if (bundleByteCount + messageSize > _udpBufferSize)
                {
                    if (!Send(splitBundle)) return false;
                    bundleByteCount = OscConst.bundleHeaderSize;
                    splitBundle.Clear();
                }
                splitBundle.packets.Add(message);
                bundleByteCount += messageSize;
            }
            if (splitBundle.packets.Count > 0 && !Send(splitBundle)) return false;
            OscPool.Recycle(splitBundle);
            return true;
        }

        // Try to pack the message.
        int index = 0;
        if (!packet.TryWriteTo(_cache, ref index)) return false;

        // Send data!
        return TrySendCache(index);
    }


    /// <summary>
    /// Send an OscMessage with a single argument.
    /// </summary>
    public void Send(string address, float value) { SendPooled(OscPool.GetMessage(address).Add(value)); }


    /// <summary>
    /// Send an OscMessage with a single argument.
    /// </summary>
    public void Send(string address, double value) { SendPooled(OscPool.GetMessage(address).Add(value)); }


    /// <summary>
    /// Send an OscMessage with a single argument.
    /// </summary>
    public void Send(string address, int value) { SendPooled(OscPool.GetMessage(address).Add(value)); }


    /// <summary>
    /// Send an OscMessage with a single argument.
    /// </summary>
    public void Send(string address, long value) { SendPooled(OscPool.GetMessage(address).Add(value)); }


    /// <summary>
    /// Send an OscMessage with a single argument.
    /// </summary>
    public void Send(string address, string value) { SendPooled(OscPool.GetMessage(address).Add(value)); }


    /// <summary>
    /// Send an OscMessage with a single argument.
    /// </summary>
    public void Send(string address, char value) { SendPooled(OscPool.GetMessage(address).Add(value)); }


    /// <summary>
    /// Send an OscMessage with a single argument.
    /// </summary>
    public void Send(string address, bool value) { SendPooled(OscPool.GetMessage(address).Add(value)); }


    /// <summary>
    /// Send an OscMessage with a single argument.
    /// </summary>
    public void Send(string address, Color32 value) { SendPooled(OscPool.GetMessage(address).Add(value)); }


    /// <summary>
    /// Send an OscMessage with a single argument.
    /// </summary>
    public void Send(string address, byte[] value) { SendPooled(OscPool.GetMessage(address).Add(value)); }


    /// <summary>
    /// Send an OscMessage with a single argument.
    /// </summary>
    public void Send(string address, OscTimeTag value) { SendPooled(OscPool.GetMessage(address).Add(value)); }


    /// <summary>
    /// Send an OscMessage with a single argument.
    /// </summary>
    public void Send(string address, OscMidiMessage value) { SendPooled(OscPool.GetMessage(address).Add(value)); }


    /// <summary>
    /// Send an OscMessage with a single argument.
    /// </summary>
    public void Send(string address, OscNull value) { SendPooled(OscPool.GetMessage(address).Add(value)); }


    /// <summary>
    /// Send an OscMessage with a single argument.
    /// </summary>
    public void Send(string address, OscImpulse value) { SendPooled(OscPool.GetMessage(address).Add(value)); }


    /// <summary>
    /// Send an OscMessage with no arguments.
    /// </summary>
    public void Send(string address) { SendPooled(OscPool.GetMessage(address)); }


    /// <summary>
    /// Subscribe to all outgoing messages.
    /// </summary>
    public void MapAnyMessage(UnityAction<OscMessage> method)
    {
        _onAnyMessage.AddPersistentListener(method);
        _onAnyMessageListenerCount++;
        //Debug.Log( "MapAnyMessage " + method + " " + _onAnyMessage.GetPersistentEventCount() );
    }


    /// <summary>
    /// Unsubscribe to all outgoing messages.
    /// </summary>
    public void UnmapAnyMessage(UnityAction<OscMessage> method)
    {
        _onAnyMessage.RemovePersistentListener(method);
        _onAnyMessageListenerCount--;
        //Debug.Log( "UnmapAnyMessage " + method + " " + _onAnyMessage.GetPersistentEventCount() );
    }


    void SendPooled(OscMessage message)
    {
        if (isOpen) Send(message);

        if (_onAnyMessageListenerCount == 0) OscPool.Recycle(message);
    }


    void ExtractMessages(OscPacket packet, Queue<OscMessage> list)
    {
        if (packet is OscMessage)
        {
            list.Enqueue(packet as OscMessage);
        }
        else
        {
            OscBundle bundle = packet as OscBundle;
            foreach (OscPacket subPacket in bundle.packets) ExtractMessages(subPacket, list);
        }
    }


    void InvokeAnyMessageEventRecursively(OscPacket packet)
    {
        if (packet is OscBundle)
        {
            OscBundle bundle = packet as OscBundle;
            foreach (OscPacket subPacket in bundle.packets) InvokeAnyMessageEventRecursively(subPacket);
        }
        else
        {
            OscMessage message = packet as OscMessage;
            _onAnyMessage.Invoke(message);

#if UNITY_EDITOR
            _inspectorMessageEvent.Invoke(message);
#endif
            _messageCount++;
        }
    }


    bool TrySendCache(int byteCount)
    {
        // TODO this should be moved to a UdpSender class.

        try
        {
            // Send!!
            _udpClient.Send(_cache, byteCount, _endPoint);

            // Socket error reference: https://msdn.microsoft.com/en-us/library/windows/desktop/ms740668(v=vs.85).aspx
        }
        catch (SocketException ex)
        {
            if (ex.ErrorCode == 10051)
            { // "Network is unreachable"
              // Ignore. We get this when broadcasting while having no access to a network.

            }
            else if (ex.ErrorCode == 10065)
            { // "No route to host"
              // Ignore. We get this sometimes when unicasting.

            }
            else if (ex.ErrorCode == 10049)
            { // "The requested address is not valid in this context"
              // Ignore. We get this when we broadcast and have no access to the local network. For example if we are using a VPN.

            }
            else if (ex.ErrorCode == 10061)
            { // "Connection refused"
              // Ignore.

            }
            else if (ex.ErrorCode == 10064)
            { // "Host is down"
              // Ignore. We get this when the remote target is not found.
            }
            else if (ex.ErrorCode == 10040)
            { // "Message too long"
                StringBuilder sb = OscDebug.BuildText(this);
                sb.Append("Failed to send message. Packet size at "); sb.Append(byteCount);
                sb.Append(" bytes exceeds udp buffer size at "); sb.Append(_udpBufferSize);
                sb.Append(" bytes. Try increasing the buffer size or enable 'splitBundlesAvoidingBufferOverflow.'\n");
                Debug.LogWarning(sb.ToString());

            }
            else
            {
                StringBuilder sb = OscDebug.BuildText(this);
                sb.Append("Failed to send message to "); sb.Append(_remoteIpAddress);
                sb.Append(" on port "); sb.Append(port); sb.Append(".\n");
                sb.Append(ex.ErrorCode); sb.Append(ex);
                Debug.LogWarning(sb.ToString());
            }
            return false;
        }
        catch (Exception ex)
        {
            StringBuilder sb = OscDebug.BuildText(this);
            sb.Append("Failed to send message to "); sb.Append(_remoteIpAddress);
            sb.Append(" on port "); sb.Append(port); sb.Append(".\n");
            sb.Append(ex);
            Debug.LogWarning(sb.ToString());
            return false;
        }

        return true;
    }


    IEnumerator SendBundleOnEndOfFrame()
    {
        while (_bundleMessagesOnEndOfFrame)
        {
            // Wait.
            yield return _endOfFrameYield;

            // Prepare for composing bundle.
            _endOfFrameTimeTag.time = _dateTime;
            int cacheIndex = 0;
            OscBundle.TryWriteHeader(_endOfFrameTimeTag, _cache, ref cacheIndex);

            // Loop through serialized messages in the end-of-frame-buffer.
            int bufferIndex = 0;
            for (int m = 0; m < _endOfFrameBuffer.count; m++)
            {
                // Compute size.
                int messageSize = _endOfFrameBuffer.GetSize(m); // Bundle stores size of each message in a 4 byte integer.

                // Check limit.
                if (_splitBundlesAvoidingBufferOverflow && cacheIndex + FourByteOscData.byteCount + messageSize >= _udpBufferSize)
                {
                    // We have reached the safelty limit, now send the bundle.
                    TrySendCache(cacheIndex);

                    // Get ready for composing next bundle.
                    cacheIndex = OscConst.bundleHeaderSize;
                }

                // Write message size and data.
                new FourByteOscData(messageSize).TryWriteTo(_cache, ref cacheIndex);
                Buffer.BlockCopy(_endOfFrameBuffer.data, bufferIndex, _cache, cacheIndex, messageSize);
                bufferIndex += messageSize;
                cacheIndex += messageSize;
            }

            // Send bundle if there is anything in it and clean.
            if (cacheIndex > OscConst.bundleHeaderSize) TrySendCache(cacheIndex);
            _endOfFrameBuffer.Clear();
        }

        _endOfFrameCoroutine = null;
    }


    IEnumerator PingCoroutine()
    {
        while (true)
        {
            Ping ping = new Ping(_remoteIpAddress);
            yield return _pingYield;
            _remoteStatus = (ping.isDone && ping.time >= 0) ? OscRemoteStatus.Connected : OscRemoteStatus.Disconnected;
        }
    }


    [Obsolete("Use MapAnyMessage() and UnmapAnyMessage()")]
    public OscMessageEvent onAnyMessage
    {
        get { return _onAnyMessage; }
        set { _onAnyMessage = value; }
    }


    [Obsolete("Use Send( message ) instead.")]
    public void Send(string address, params object[] args)
    {
        OscMessage message = OscPool.GetMessage(address);
        message.Add(args);
        SendPooled(message);
    }

    [Obsolete("Use remoteIpAddress instead.")]
    public string ipAddress { get { return _remoteIpAddress; } }
}