﻿    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OverriddenNetworkDiscovery : NetworkDiscovery
{
    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        NetworkManager.singleton.networkAddress = fromAddress;
        NetworkManager.singleton.StartClient();
    }
}
