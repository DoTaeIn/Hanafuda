using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public string code;
    public int maxConnections = 3;

    private void Awake()
    {
        networkManager = FindObjectOfType<NetworkManager>();
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void CreateGame()
    { 
        Allocation a =  await RelayService.Instance.CreateAllocationAsync(maxConnections); 
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(a, "dtls"));
        
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

        code = NetworkManager.Singleton.StartHost() ? joinCode : "";
    }

    public async void JoinGame(string joinCode)
    {
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
        Debug.Log(!string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient());
    }
    
    
}
