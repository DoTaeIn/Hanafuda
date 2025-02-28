using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    private NetworkManager networkManager;

    private void Awake()
    {
        networkManager = FindObjectOfType<NetworkManager>();
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async Task<string> StartRelayHost(int maxConnections = 3)
    { 
        Allocation a =  await RelayService.Instance.CreateAllocationAsync(maxConnections); 
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(a, "dtls"));
        
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);
        
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }
}
