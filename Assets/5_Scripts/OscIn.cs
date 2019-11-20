/*
	Created by Carl Emil Carlsen.
	Copyright 2016-2019 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using OscSimpl;


/// <summary>
/// MonoBehaviour for receiving OscMessage objects. The "OSC Server".
/// </summary>
[ExecuteInEditMode]
public class OscIn : OscMonoBase
{
    [SerializeField] int _port = 7000;
    [SerializeField] OscReceiveMode _mode = OscReceiveMode.UnicastBroadcast;
    [SerializeField] string _multicastAddress = string.Empty;
    [SerializeField] bool _filterDuplicates = true;
    [SerializeField] bool _addTimeTagsToBundledMessages = false;
    [SerializeField] bool _dirtyMappings = true;
    [SerializeField] List<OscMapping> _mappings = new List<OscMapping>();

    UdpReceiver _receiver = null;
    object _lock = new object();
    List<TempBuffer> _lockedBufferList = new List<TempBuffer>(); // We read into multiple buffers to ensure that multiple packets can be received per Unity update.
    int _lockedBufferCount = 0;
    bool _isOpen;
    Queue<OscPacket> _packetQueue = new Queue<OscPacket>();
    List<string> _uniqueAddresses = new List<string>(); // for filtering duplicates.
    Dictionary<int, Dictionary<string, OscMapping>> _regularMappingLookup;
    List<OscMapping> _specialPatternMappings;
    bool _wasClosedOnDisable;



    // For the inspector
#if UNITY_EDITOR
    [SerializeField] bool _settingsFoldout;
    [SerializeField] bool _mappingsFoldout;
    [SerializeField] bool _messagesFoldout;
#endif


    /// <summary>
    /// Gets the local port that this application is set to listen to. (read only).
    /// To set, call the Open method.
    /// </summary>
    public int port { get { return _port; } }

    /// <summary>
    /// Gets the transmission mode (read only). Can either be UnicastBroadcast or Multicast.
    /// The mode is automatically derived from arguments passed to the Open method.
    /// </summary>
    public OscReceiveMode mode { get { return _mode; } }

    /// <summary>
    /// Gets the remote address to the multicast group that this application is set to listen to (read only).
    /// To set, call the Open method and provide a valid multicast address.
    /// </summary>
    public string multicastAddress { get { return _multicastAddress; } }

    /// <summary>
    /// Gets the primary local network IP address for this device (read only).
    /// If the the loopback address "127.0.0.1" is returned ensure that your device is connected to a network. 
    /// Using a VPN may block you from getting the local IP.
    /// </summary>
    public static string localIpAddress { get { return OscHelper.GetLocalIpAddress(); } }

    /// <summary>
	/// Gets the alternative local network IP addresses for this device (read only).
	/// Your device may be connected through multiple network adapters, for example through wifi and ethernet.
	/// </summary>
	public static ReadOnlyCollection<string> localIpAddressAlternatives { get { return OscHelper.GetLocalIpAddressAlternatives(); } }

    /// <summary>
    /// Indicates whether the Open method has been called and the object is ready to receive.
    /// </summary>
    public bool isOpen { get { return _receiver != null && _receiver.isOpen; } }

    /// <summary>
    /// When enabled, only one message per OSC address will be forwarded every Update call.
    /// The last (newest) message received will be used. Default is true.
    /// </summary>
    public bool filterDuplicates
    {
        get { return _filterDuplicates; }
        set { _filterDuplicates = value; }
    }

    /// <summary>
    /// When enabled, timetags from bundles are added to contained messages as last argument.
    /// Incoming bundles are never exposed, so if you want to access a time tag from a incoming bundle then enable this.
    /// Default is false.
    /// </summary>
    public bool addTimeTagsToBundledMessages
    {
        get { lock (_lock) return _addTimeTagsToBundledMessages; }
        set { lock (_lock) _addTimeTagsToBundledMessages = value; }
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
                if (isOpen) Open(_port, _multicastAddress);
            }
        }
    }

    /// <summary>
    /// Gets the number of messages received since last update.
    /// </summary>
    public int messageCount { get { lock (_lock) return _messageCount; } }


    void Awake()
    {
        if (!Application.isPlaying) return;

        if (_openOnAwake && enabled && !isOpen)
        {
            Open(_port, _multicastAddress);
        }

        _dirtyMappings = true;
    }


    void OnEnable()
    {
        if (!Application.isPlaying) return;

        if (_wasClosedOnDisable && !isOpen)
        {
            Open(_port, _multicastAddress);
        }
    }


    void OnDisable()
    {
        if (!Application.isPlaying) return;

        if (isOpen)
        {
            Close();
            _wasClosedOnDisable = true;
        }
    }


    void Update()
    {
        _messageCount = 0;

        if (_mappings == null || !isOpen) return;

        // As little work as possible while we touch the locked buffers.
        //Debug.Log( "UNITY THREAD: " + DateTime.Now.Second + ":" + DateTime.Now.Millisecond );
        lock (_lock)
        {
            // Parse messages and put them in a queue.
            for (int i = 0; i < _lockedBufferCount; i++)
            {
                TempBuffer buffer = _lockedBufferList[i];
                OscPacket packet;
                int index = 0;
                if (OscPacket.TryReadFrom(buffer.content, ref index, buffer.count, out packet)) _packetQueue.Enqueue(packet);
                //Debug.Log( "Frame: " + Time.frameCount + ", buffer index: " + i + ", size :" + buffer.count );
            }
            _lockedBufferCount = 0;
        }

        // If no messages, job done.
        if (_packetQueue.Count == 0) return;

        // Update mappings.
        if (_regularMappingLookup == null || _dirtyMappings) UpdateMappings();

        // Now we can take our time, go through the queue and dispatch the messages.
        while (_packetQueue.Count > 0) UnpackRecursivelyAndDispatch(_packetQueue.Dequeue());
        if (_filterDuplicates) _uniqueAddresses.Clear();
    }


    void OnDestroy()
    {
        if (!Application.isPlaying) return;

        if (isOpen) Close();

        // Forget all mappings.
        _onAnyMessage.RemoveAllListeners();
        foreach (OscMapping mapping in _mappings) mapping.Clear();
    }


    void UnpackRecursivelyAndDispatch(OscPacket packet)
    {
        if (packet is OscBundle)
        {
            OscBundle bundle = packet as OscBundle;
            foreach (OscPacket subPacket in bundle.packets)
            {
                if (_addTimeTagsToBundledMessages && subPacket is OscMessage)
                {
                    OscMessage message = subPacket as OscMessage;
                    message.Add(bundle.timeTag);
                }
                UnpackRecursivelyAndDispatch(subPacket);
            }
            OscPool.Recycle(bundle);
        }
        else
        {
            OscMessage message = packet as OscMessage;
            if (_filterDuplicates)
            {
                if (_uniqueAddresses.Contains(message.address))
                {
                    OscPool.Recycle(message);
                    return;
                }
                _uniqueAddresses.Add(message.address);
            }
            Dispatch(packet as OscMessage);
            _messageCount++; // Influenced by _filterDuplicates.
        }
    }



    void Dispatch(OscMessage message)
    {
        bool anyMessageActivated = _onAnyMessageListenerCount > 0;
        bool messageExposed = anyMessageActivated;

        // Regular mappings.
        Dictionary<string, OscMapping> mappingLookup;
        if (_regularMappingLookup.TryGetValue(message.GetAddressHash(), out mappingLookup))
        {
            OscMapping mapping;
            if (mappingLookup.TryGetValue(message.address, out mapping))
            {
                InvokeMapping(mapping, message);
                if (!messageExposed) messageExposed = mapping.type == OscMessageType.OscMessage;
            }
        }

        // Special pattern mappings.
        foreach (OscMapping specialMapping in _specialPatternMappings)
        {
            if (specialMapping.IsMatching(message.address))
            {
                InvokeMapping(specialMapping, message);
                if (!messageExposed) messageExposed = specialMapping.type == OscMessageType.OscMessage;
            }
        }

        // Any message handler.
        if (anyMessageActivated) _onAnyMessage.Invoke(message);

#if UNITY_EDITOR
        // Editor inspector.
        _inspectorMessageEvent.Invoke(message);
#endif

        // Recycle when possible.
        if (!anyMessageActivated && !messageExposed) OscPool.Recycle(message);
    }


    void InvokeMapping(OscMapping mapping, OscMessage message)
    {
        switch (mapping.type)
        {
            case OscMessageType.OscMessage:
                mapping.OscMessageHandler.Invoke(message);
                break;
            case OscMessageType.Float:
                float floatValue;
                if (message.TryGet(0, out floatValue)) mapping.FloatHandler.Invoke(floatValue);
                break;
            case OscMessageType.Double:
                double doubleValue;
                if (message.TryGet(0, out doubleValue)) mapping.DoubleHandler.Invoke(doubleValue);
                break;
            case OscMessageType.Int:
                int intValue;
                if (message.TryGet(0, out intValue)) mapping.IntHandler.Invoke(intValue);
                break;
            case OscMessageType.Long:
                long longValue;
                if (message.TryGet(0, out longValue)) mapping.LongHandler.Invoke(longValue);
                break;
            case OscMessageType.String:
                string stringValue = string.Empty;
                if (message.TryGet(0, ref stringValue)) mapping.StringHandler.Invoke(stringValue);
                break;
            case OscMessageType.Char:
                char charValue;
                if (message.TryGet(0, out charValue)) mapping.CharHandler.Invoke(charValue);
                break;
            case OscMessageType.Bool:
                bool boolValue;
                if (message.TryGet(0, out boolValue)) mapping.BoolHandler.Invoke(boolValue);
                break;
            case OscMessageType.Color:
                Color32 colorValue;
                if (message.TryGet(0, out colorValue)) mapping.ColorHandler.Invoke(colorValue);
                break;
            case OscMessageType.Midi:
                OscMidiMessage midiValue;
                if (message.TryGet(0, out midiValue)) mapping.MidiHandler.Invoke(midiValue);
                break;
            case OscMessageType.Blob:
                byte[] blobValue = null;
                if (message.TryGet(0, ref blobValue)) mapping.BlobHandler.Invoke(blobValue);
                break;
            case OscMessageType.TimeTag:
                OscTimeTag timeTagValue;
                if (message.TryGet(0, out timeTagValue)) mapping.TimeTagHandler.Invoke(timeTagValue);
                break;
            case OscMessageType.ImpulseNullEmpty:
                mapping.ImpulseNullEmptyHandler.Invoke();
                break;
        }
    }




    /// <summary>
    /// Open to receive messages on specified port and (optionally) from specified multicast IP address.
    /// Returns success status.
    /// </summary>
    public bool Open(int port, string multicastAddress = "")
    {
        // Ensure that we have a receiver, even when not in Play mode.
        if (_receiver == null) _receiver = new UdpReceiver(OnDataReceivedAsync);

        // Close and existing receiver.
        if (isOpen) Close();

        // Validate port number range.
        if (port < OscConst.portMin || port > OscConst.portMax)
        {
            StringBuilder sb = OscDebug.BuildText(this);
            sb.Append("Open failed. Port "); sb.Append(port); sb.Append(" is out of range.\n");
            Debug.LogWarning(sb.ToString());
            return false;
        }
        _port = port;

        // Derive mode from multicastAddress.
        IPAddress multicastIP;
        if (!string.IsNullOrEmpty(multicastAddress) && IPAddress.TryParse(multicastAddress, out multicastIP))
        {
            if (Regex.IsMatch(multicastAddress, OscConst.multicastAddressPattern))
            {
                _mode = OscReceiveMode.UnicastBroadcastMulticast;
                _multicastAddress = multicastAddress;
            }
            else
            {
                StringBuilder sb = OscDebug.BuildText(this);
                sb.Append("Open failed. Multicast IP address "); sb.Append(multicastAddress);
                sb.Append(" is out not valid. It must be in range 224.0.0.0 to 239.255.255.255.\n");
                Debug.LogWarning(sb.ToString());
                return false;
            }
        }
        else
        {
            _multicastAddress = string.Empty;
            _mode = OscReceiveMode.UnicastBroadcast;
        }

        // Set buffer size.
        _receiver.bufferSize = _udpBufferSize;

        // Try open.
        if (!_receiver.Open(_port, _multicastAddress))
        {
            Debug.Log("Failed to open");
            return false;
        }

        // Deal with the success
        if (Application.isPlaying)
        {
            StringBuilder sb = OscDebug.BuildText(this);
            if (_mode == OscReceiveMode.UnicastBroadcast)
            {
                sb.Append("Ready to receive unicast and broadcast messages on port ");
            }
            else
            {
                sb.Append("Ready to receive multicast messages on address "); sb.Append(_multicastAddress); sb.Append(", unicast and broadcast messages on port ");
            }
            sb.Append(_port); sb.AppendLine();
            Debug.Log(sb.ToString());
        }

        return true;
    }


    // This is called independant of the unity update loop. Can happen at any time.
    void OnDataReceivedAsync(byte[] data, int byteCount)
    {
        // We want to do as little work as possible here so that UdpReceiver can continue it's work.
        lock (_lock)
        {
            TempBuffer buffer;
            if (_lockedBufferCount >= _lockedBufferList.Count)
            {
                buffer = new TempBuffer();
                _lockedBufferList.Add(buffer);
            }
            else
            {
                buffer = _lockedBufferList[_lockedBufferCount];
            }
            buffer.AdaptableCopyFrom(data, byteCount);
            _lockedBufferCount++;
        }
    }


    /// <summary>
    /// Close and stop receiving messages.
    /// </summary>
    public void Close()
    {
        if (_receiver != null) _receiver.Close();

        lock (_lock)
        {
            _lockedBufferList.Clear();
            _lockedBufferCount = 0;
        }

        _wasClosedOnDisable = false;
    }


    // TODO
    // This is the syntex I would prefer, but it gives the following error:
    // 		CS0121: The call is ambiguous between the following methods or properties:
    //		`OscIn.Map(string, UnityEngine.Events.UnityAction<float>)' and 
    //		`OscIn.Map(string, UnityEngine.Events.UnityAction<int>)'
    // I am letting it stay here in the hope that the syntax will be supported in a later version of .NET.
    /*
	public void Map( string address, UnityAction<float> method ) { Map( address, method, OscMessageType.Float ); }
	public void Map( string address, UnityAction<int> method ) { Map( address, method, OscMessageType.Double ); }
	void TestOnFloat( float value ) { }
	void TestMap(){
		Map( "/", TestOnFloat ); // Error here.
	}
	*/

    /// <summary>
    /// Request that incoming messages with 'address' are forwarded to 'method'.
    /// </summary>
    public void Map(string address, UnityAction<OscMessage> method) { Map(address, method, OscMessageType.OscMessage); }

    /// <summary>
    /// Request that a float type argument is extracted from incoming messages with matching 'address' and forwarded to 'method'.
    /// </summary>
    public void MapFloat(string address, UnityAction<float> method) { Map(address, method, OscMessageType.Float); }

    /// <summary>
    /// Request that a double type argument is extracted from incoming messages with matching 'address' and forwarded to 'method'.
    /// </summary>
    public void MapDouble(string address, UnityAction<double> method) { Map(address, method, OscMessageType.Double); }

    /// <summary>
    /// Request that a int type argument is extracted from incoming messages with matching 'address' and forwarded to 'method'.
    /// </summary>
    public void MapInt(string address, UnityAction<int> method) { Map(address, method, OscMessageType.Int); }

    /// <summary>
    /// Request that a long type argument is extracted from incoming messages with matching 'address' and forwarded to 'method'.
    /// </summary>
    public void MapLong(string address, UnityAction<long> method) { Map(address, method, OscMessageType.Long); }

    /// <summary>
    /// Request that a string type argument is extracted from incoming messages with matching 'address' and forwarded to 'method'.
    /// This method produces heap garbage. Use Map( string, UnityAction<OscMessage> ) instead and then use TryGet( int, ref string ) 
    /// to read into a cached string. See how in the Optimisations example.
    /// </summary>
    [Obsolete("This method produces heap garbage. Use Map( string, UnityAction<OscMessage> ) instead and then use TryGet( int, ref string ) to read into a cached string. See how in the Optimisations example.")]
    public void MapString(string address, UnityAction<string> method) { Map(address, method, OscMessageType.String); }

    /// <summary>
    /// Request that a char type argument is extracted from incoming messages with matching 'address' and forwarded to 'method'.
    /// </summary>
    public void MapChar(string address, UnityAction<char> method) { Map(address, method, OscMessageType.Char); }

    /// <summary>
    /// Request that a bool type argument is extracted from incoming messages with matching 'address' and forwarded to 'method'.
    /// </summary>
    public void MapBool(string address, UnityAction<bool> method) { Map(address, method, OscMessageType.Bool); }

    /// <summary>
    /// Request that a Color32 type argument is extracted from incoming messages with matching 'address' and forwarded to 'method'.
    /// </summary>
    public void MapColor(string address, UnityAction<Color32> method) { Map(address, method, OscMessageType.Color); }

    /// <summary>
    /// Request that a byte blob argument is extracted from incoming messages with matching 'address' and forwarded to 'method'.
    /// This method produces heap garbage. Use Map( string, UnityAction<OscMessage> ) instead and then use TryGet( int, ref byte[] ) 
    /// to read into a cached array. See how in the Optimisations example.
    /// </summary>
    [Obsolete("This method produces heap garbage. Use Map( string, UnityAction<OscMessage> ) instead and use TryGet( int, ref byte[] ) to read into a cached array. See how in the Optimisations example.")]
    public void MapBlob(string address, UnityAction<byte[]> method) { Map(address, method, OscMessageType.Blob); }

    /// <summary>
    /// Request that a time tag argument is extracted from incoming messages with matching 'address' and forwarded to 'method'.
    /// </summary>
    public void MapTimeTag(string address, UnityAction<OscTimeTag> method) { Map(address, method, OscMessageType.TimeTag); }

    /// <summary>
    /// Request that a OscMidiMessage type argument is extracted from incoming messages with matching 'address' and forwarded to 'method'.
    /// </summary>
    public void MapMidi(string address, UnityAction<OscMidiMessage> method) { Map(address, method, OscMessageType.Midi); }


    /// <summary>
    /// Request that 'method' is invoked when a message with matching 'address' is received with type tag Impulse (i), Null (N) or simply without arguments.
    /// </summary>
    public void MapImpulseNullOrEmpty(string address, UnityAction method)
    {
        // Validate.
        if (!ValidateAddressForMapping(address) || method == null || !ValidateMethodTarget(method.Target, address)) return;

        // Get or create mapping.
        OscMapping mapping = null;
        GetOrCreateMapping(address, OscMessageType.ImpulseNullEmpty, out mapping);

        // Add listener.
        mapping.ImpulseNullEmptyHandler.AddPersistentListener(method);

        // Set dirty flag.
        _dirtyMappings = true;
    }


    void Map<T>(string address, UnityAction<T> method, OscMessageType type)
    {
        // Validate.
        if (!ValidateAddressForMapping(address) || method == null || !ValidateMethodTarget(method.Target, address)) return;

        // Get or create mapping.
        OscMapping mapping = null;
        GetOrCreateMapping(address, type, out mapping);

        // Add listener.
        switch (type)
        {
            case OscMessageType.OscMessage: mapping.OscMessageHandler.AddPersistentListener(method as UnityAction<OscMessage>); break;
            case OscMessageType.Bool: mapping.BoolHandler.AddPersistentListener(method as UnityAction<bool>); break;
            case OscMessageType.Float: mapping.FloatHandler.AddPersistentListener(method as UnityAction<float>); break;
            case OscMessageType.Int: mapping.IntHandler.AddPersistentListener(method as UnityAction<int>); break;
            case OscMessageType.Char: mapping.CharHandler.AddPersistentListener(method as UnityAction<char>); break;
            case OscMessageType.Color: mapping.ColorHandler.AddPersistentListener(method as UnityAction<Color32>); break;
            case OscMessageType.Midi: mapping.MidiHandler.AddPersistentListener(method as UnityAction<OscMidiMessage>); break;
            case OscMessageType.Double: mapping.DoubleHandler.AddPersistentListener(method as UnityAction<double>); break;
            case OscMessageType.Long: mapping.LongHandler.AddPersistentListener(method as UnityAction<long>); break;
            case OscMessageType.TimeTag: mapping.TimeTagHandler.AddPersistentListener(method as UnityAction<OscTimeTag>); break;
            case OscMessageType.String: mapping.StringHandler.AddPersistentListener(method as UnityAction<string>); break;
            case OscMessageType.Blob: mapping.BlobHandler.AddPersistentListener(method as UnityAction<byte[]>); break;
        }

        // Set dirty flag.
        _dirtyMappings = true;
    }



    string GetMethodLabel(object source, System.Reflection.MethodInfo methodInfo)
    {
        string simpleType = source.GetType().ToString();
        int dotIndex = simpleType.LastIndexOf('.') + 1;
        simpleType = simpleType.Substring(dotIndex, simpleType.Length - dotIndex);
        return simpleType + "." + methodInfo.Name;
    }


    bool ValidateAddressForMapping(string address)
    {
        // Check for address prefix.
        if (address.Length < 2 || address[0] != OscConst.addressPrefix)
        {
            StringBuilder sb = OscDebug.BuildText(this);
            sb.Append("Ignored attempt to create mapping. OSC addresses must begin with slash '/'.");
            Debug.LogWarning(sb.ToString());
            return false;
        }

        // Check for whitespace.
        if (address.Contains(" "))
        {
            StringBuilder sb = OscDebug.BuildText(this);
            sb.Append("Ignored attempt to create mapping. OSC addresses cannor whitespaces.");
            Debug.LogWarning(sb.ToString());
            return false;
        }

        return true;
    }


    bool ValidateMethodTarget(object target, string address)
    {
        if (target == null)
        {
            StringBuilder sb = OscDebug.BuildText(this);
            sb.Append("Ignored attempt to create mapping. Method cannot be null.\n");
            sb.Append(address);
            Debug.LogWarning(sb);
            return false;
        }

        if (!(target is UnityEngine.Object))
        {
            StringBuilder sb = OscDebug.BuildText(this);
            sb.Append("Ignored attempt to create mapping. Method must be a member of an object that inrehits from ScriptableObject or MonoBehaviour.\n");
            sb.Append(address);
            Debug.LogWarning(sb);
            return false;
        }

        return true;
    }


    bool GetOrCreateMapping(string address, OscMessageType type, out OscMapping mapping)
    {
        mapping = _mappings.Find(m => m.address == address);
        if (mapping == null)
        {
            mapping = new OscMapping(address, type);
            _mappings.Add(mapping);
        }
        else if (mapping.type != type)
        {
            StringBuilder sb = OscDebug.BuildText(this);
            sb.Append("Failed to map address'"); sb.Append(address);
            sb.Append("' to method with argument type '"); sb.Append(type);
            sb.Append("'. Address is already set to receive type '"); sb.Append(mapping.type);
            sb.Append("', either in the editor, or by a script.\nOnly one type per address is allowed.\n");
            Debug.LogWarning(sb.ToString());
            return false;
        }
        return true;
    }


    /// <summary>
    /// Request that 'method' is no longer invoked by OscIn.
    /// </summary>
    public void Unmap(UnityAction<OscMessage> method) { Unmap(method, OscMessageType.OscMessage); }


    /// <summary>
    /// Request that 'method' is no longer invoked by OscIn.
    /// </summary>
    public void UnmapFloat(UnityAction<float> method) { Unmap(method, OscMessageType.Float); }


    /// <summary>
    /// Request that 'method' is no longer invoked by OscIn.
    /// </summary>
    public void UnmapInt(UnityAction<int> method) { Unmap(method, OscMessageType.Int); }


    /// <summary>
    /// Request that 'method' is no longer invoked by OscIn.
    /// </summary>
    public void UnmapString(UnityAction<string> method) { Unmap(method, OscMessageType.String); }


    /// <summary>
    /// Request that 'method' is no longer invoked by OscIn.
    /// </summary>
    public void UnmapBool(UnityAction<bool> method) { Unmap(method, OscMessageType.Bool); }


    /// <summary>
    /// Request that 'method' is no longer invoked by OscIn.
    /// </summary>
    public void UnmapColor(UnityAction<Color32> method) { Unmap(method, OscMessageType.Color); }


    /// <summary>
    /// Request that 'method' is no longer invoked by OscIn.
    /// </summary>
    public void UnmapChar(UnityAction<char> method) { Unmap(method, OscMessageType.Char); }


    /// <summary>
    /// Request that 'method' is no longer invoked by OscIn.
    /// </summary>
    public void UnmapDouble(UnityAction<double> method) { Unmap(method, OscMessageType.Double); }


    /// <summary>
    /// Request that 'method' is no longer invoked by OscIn.
    /// </summary>
    public void UnmapLong(UnityAction<long> method) { Unmap(method, OscMessageType.Long); }


    /// <summary>
    /// Request that 'method' is no longer invoked by OscIn.
    /// </summary>
    public void UnmapTimeTag(UnityAction<OscTimeTag> method) { Unmap(method, OscMessageType.TimeTag); }


    /// <summary>
    /// Request that 'method' is no longer invoked by OscIn.
    /// </summary>
    public void UnmapMidi(UnityAction<OscMidiMessage> method) { Unmap(method, OscMessageType.Midi); }


    /// <summary>
    /// Request that 'method' is no longer invoked by OscIn.
    /// </summary>
    public void UnmapBlob(UnityAction<byte[]> method) { Unmap(method, OscMessageType.Blob); }


    /// <summary>
    /// Request that 'method' is no longer invoked by OscIn.
    /// </summary>
    public void UnmapImpulseNullOrEmpty(UnityAction method)
    {
        // UnityEvent is secret about whether we removed a runtime handler, so we have to iterate the whole array og mappings.
        for (int m = _mappings.Count - 1; m >= 0; m--)
        {
            OscMapping mapping = _mappings[m];

            // If there are no methods mapped to the hanlder left, then remove mapping.
            if (mapping.ImpulseNullEmptyHandler.GetPersistentEventCount() == 0) _mappings.RemoveAt(m);

        }
        _dirtyMappings = true;
    }

    /// <summary>
    /// Request that all methods that are mapped to OSC 'address' will no longer be invoked.
    /// </summary>
    public void UnmapAll(string address)
    {
        OscMapping mapping = _mappings.Find(m => m.address == address);
        if (mapping != null)
        {
            mapping.Clear();
            _mappings.Remove(mapping);
        }
    }


    /// <summary>
    /// Subscribe to all outgoing messages.
    /// The state of 'filterDuplicates' does apply.
    /// </summary>
    public void MapAnyMessage(UnityAction<OscMessage> message)
    {
        _onAnyMessage.AddPersistentListener(message);
        _onAnyMessageListenerCount++;
    }


    /// <summary>
    /// Unsubscribe to all outgoing messages.
    /// </summary>
    public void UnmapAnyMessage(UnityAction<OscMessage> message)
    {
        _onAnyMessage.RemovePersistentListener(message);
        _onAnyMessageListenerCount--;
    }


    void Unmap<T>(UnityAction<T> method, OscMessageType type)
    {
        for (int m = _mappings.Count - 1; m >= 0; m--)
        {
            OscMapping mapping = _mappings[m];

            int eventCount = 0;
            switch (type)
            {
                case OscMessageType.OscMessage:
                    mapping.OscMessageHandler.RemovePersistentListener(method as UnityAction<OscMessage>);
                    eventCount = mapping.OscMessageHandler.GetPersistentEventCount(); break;
                case OscMessageType.Bool:
                    mapping.BoolHandler.RemovePersistentListener(method as UnityAction<bool>);
                    eventCount = mapping.BoolHandler.GetPersistentEventCount(); break;
                case OscMessageType.Float:
                    mapping.FloatHandler.RemovePersistentListener(method as UnityAction<float>);
                    eventCount = mapping.FloatHandler.GetPersistentEventCount(); break;
                case OscMessageType.Int:
                    mapping.IntHandler.RemovePersistentListener(method as UnityAction<int>);
                    eventCount = mapping.IntHandler.GetPersistentEventCount(); break;
                case OscMessageType.Char:
                    mapping.CharHandler.RemovePersistentListener(method as UnityAction<char>);
                    eventCount = mapping.CharHandler.GetPersistentEventCount(); break;
                case OscMessageType.Color:
                    mapping.ColorHandler.RemovePersistentListener(method as UnityAction<Color32>);
                    eventCount = mapping.ColorHandler.GetPersistentEventCount(); break;
                case OscMessageType.Midi:
                    mapping.MidiHandler.RemovePersistentListener(method as UnityAction<OscMidiMessage>);
                    eventCount = mapping.MidiHandler.GetPersistentEventCount(); break;
                case OscMessageType.Double:
                    mapping.DoubleHandler.RemovePersistentListener(method as UnityAction<double>);
                    eventCount = mapping.DoubleHandler.GetPersistentEventCount(); break;
                case OscMessageType.Long:
                    mapping.LongHandler.RemovePersistentListener(method as UnityAction<long>);
                    eventCount = mapping.LongHandler.GetPersistentEventCount(); break;
                case OscMessageType.TimeTag:
                    mapping.TimeTagHandler.RemovePersistentListener(method as UnityAction<OscTimeTag>);
                    eventCount = mapping.TimeTagHandler.GetPersistentEventCount(); break;
                case OscMessageType.String:
                    mapping.StringHandler.RemovePersistentListener(method as UnityAction<string>);
                    eventCount = mapping.StringHandler.GetPersistentEventCount(); break;
                case OscMessageType.Blob:
                    mapping.BlobHandler.RemovePersistentListener(method as UnityAction<byte[]>);
                    eventCount = mapping.BlobHandler.GetPersistentEventCount(); break;
            }

            // If there are no methods mapped to the hanlder left, then remove mapping.
            if (eventCount == 0) _mappings.RemoveAt(m);
        }
        _dirtyMappings = true;
    }


    void UpdateMappings()
    {
        // Create or clear collections.
        if (_regularMappingLookup == null) _regularMappingLookup = new Dictionary<int, Dictionary<string, OscMapping>>();
        else _regularMappingLookup.Clear();
        if (_specialPatternMappings == null) _specialPatternMappings = new List<OscMapping>();
        else _specialPatternMappings.Clear();

        // Add mappings.
        foreach (OscMapping mapping in _mappings)
        {
            if (mapping.hasSpecialPattern)
            {
                _specialPatternMappings.Add(mapping);
            }
            else
            {
                int hash = OscStringHash.Pack(mapping.address);
                Dictionary<string, OscMapping> mappingLookup;
                if (!_regularMappingLookup.TryGetValue(hash, out mappingLookup))
                {
                    mappingLookup = new Dictionary<string, OscMapping>();
                    _regularMappingLookup.Add(hash, mappingLookup);
                }
                mappingLookup.Add(mapping.address, mapping);
            }
        }

        // Update flag.
        _dirtyMappings = false;
    }



    class TempBuffer
    {
        public int count;
        public byte[] content;

        public void AdaptableCopyFrom(byte[] data, int count)
        {
            if (content == null || content.Length != data.Length) content = new byte[data.Length];
            Buffer.BlockCopy(data, 0, content, 0, count);
            this.count = count;
        }
    }


    [Obsolete("Use MapAnyMessage() and UnmapAnyMessage()")]
    public OscMessageEvent onAnyMessage
    {
        get { return _onAnyMessage; }
        set { _onAnyMessage = value; }
    }

    [Obsolete("Use localIpAddress instead.")]
    public static string ipAddress { get { return localIpAddress; } }
}