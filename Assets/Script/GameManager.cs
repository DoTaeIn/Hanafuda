using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    List<NetworkClient> clients = new List<NetworkClient>();
    NetworkManager networkManager;
    [SerializeField] NetworkObject userListObj;
    [SerializeField] Transform[] UiParent;
    
    public NetworkVariable<int> money = new NetworkVariable<int>();
    
    [SerializeField] List<CardData> cards = new List<CardData>();
    [SerializeField] Dictionary<ulong, PlayerManager> players = new Dictionary<ulong, PlayerManager> ();
    

    private void Awake()
    {
        networkManager = FindObjectOfType<NetworkManager>();
    }

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted; 
        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
            money.Value = 0;
        else
        {
            money.OnValueChanged += OnSomeValueChanged;
        }
    }
    
    private void OnSomeValueChanged(int previous, int current)
    {
        Debug.Log($"Detected NetworkVariable Change: Previous: {previous} | Current: {current}");
    }

    public void betToPot(int amount)
    {
        betToPotServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void betToPotServerRpc(int amt)
    {
        money.Value += amt;
    }


    public void StartGame()
    {
        if (IsServer)
        {
            cards.Shuffle();

            for (int i = 0; i < players.Count; i++)
            {
                players.Values.ToList()[i].cardNums.Add(cards[i*2]);
                players.Values.ToList()[i].cardNums.Add(cards[i*2+1]);
                
                // 0: 01, 1:23 2:45
            }
        }
    }

    

    private void Update()
    {
        
    }

    
    void OnClientConnected(ulong clientId)
    {
        if(IsHost)
        {
            NetworkClient nc = networkManager.ConnectedClients[clientId];
            clients.Add(nc);
            
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn
            (
                userListObj, clientId, false, false, false, Vector3.zero, Quaternion.identity
            ).transform.SetParent(UiParent[(clients.Count - 1)%2]);

            PlayerManager[] temp = UiParent[(clients.Count - 1) % 2].GetComponentsInChildren<PlayerManager>();
            
            players.Add(clientId, temp[^1]);
            
        }
        
        /*
        if (clientId != 0)
            networkManager.SpawnManager.SpawnedObjects[clientId].gameObject.transform.SetParent(UiParent[clientId%2]);
        */
        
        Debug.LogAssertion($"ID {clientId} Joined");
    }

    void OnClientDisconnected(ulong clientId)
    {
        if (IsHost)
        {
            for (int i = clients.Count - 1; i >= 0; i--)
                if (clients[i].ClientId == clientId)
                {
                    //Debug.Log(NetworkManager.Singleton.SpawnManager.SpawnedObjects[clientId].name);
                    players[clientId].gameObject.GetComponentInChildren<NetworkObject>().Despawn(true);
                    players.Remove(clientId);
                    clients.RemoveAt(i);
                }
        }
        Debug.LogAssertion($"ID {clientId} Left");
    }
    
    void OnServerStarted()
    {
        //networkManager.SpawnManager.GetLocalPlayerObject().gameObject.transform.SetParent(UiParent[0]);
    }
    
    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted; 
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1); // Get a random index
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]); // Swap elements
        }
    }
}
