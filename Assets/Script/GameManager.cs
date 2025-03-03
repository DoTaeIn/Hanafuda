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
    
    public List<CardData> cards = new List<CardData>();
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
        if (IsServer) // Server distributes the cards
        {
            cards.Shuffle(); // Shuffle the deck

            foreach (var playerEntry in players) // Iterate over all players
            {
                var player = playerEntry.Value;

                if (player.cardNums.Count != 2)
                {
                    int index = players.Keys.ToList().IndexOf(playerEntry.Key) * 2;
                    player.cardNums.Add(cards[index].CardID);
                    player.cardNums.Add(cards[index + 1].CardID);
                }
                
            }

            for (int i = 0; i < players.Keys.Count; i++)
            {
                // Ensure the RPC is actually sent to the correct client
                Debug.Log($"[StartGame] Sending ShowCardsRpc to Player {players.Keys.ToList()[i]} (OwnerClientId: {players.Keys.ToList()[i]})");

                // Server sends an RPC to the **specific client** to show their cards
                ShowCardsRpc(players.Keys.ToList()[i]);
            }
        }
    }


    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]  
    public void ShowCardsRpc(ulong playerId)
    {
        Debug.Log($"[ShowCardsRpc] Received RPC for player: {playerId}, LocalClientId: {NetworkManager.Singleton.LocalClientId}");

        if (NetworkManager.Singleton.LocalClientId == playerId) // Ensure only the correct player executes it
        {
            Debug.Log($"[TEST] ShowCardsRpc() CALLED ON CLIENT {playerId}");

            foreach (var ublock in UiParent[playerId].GetComponentsInChildren<UserBlockManager>())
            {
                Debug.Log($"[ShowCardsRpc] Checking UI block with OwnerClientId {ublock.GetComponent<NetworkObject>().OwnerClientId}");

                if (ublock.GetComponent<NetworkObject>().OwnerClientId == playerId)
                {
                    Debug.Log($"[ShowCardsRpc] Showing cards for Player {playerId}");
                    ublock.ShowCards();
                }
            }
        }
        else
        {
            Debug.Log($"[ShowCardsRpc] Skipping execution for Player {playerId}, does not match LocalClientId {NetworkManager.Singleton.LocalClientId}");
        }
    }







    

    private void Update()
    {
        
    }

    
    void OnClientConnected(ulong clientId)
    {
        if (IsHost)
        {
            NetworkClient nc = networkManager.ConnectedClients[clientId];
            clients.Add(nc);
        
            NetworkObject newPlayerObject = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn
            (
                userListObj, clientId, false, false, false, Vector3.zero, Quaternion.identity
            );

            newPlayerObject.transform.SetParent(UiParent[(clients.Count - 1) % 2]);
        
            players.Add(clientId, newPlayerObject.GetComponent<PlayerManager>());

            // 🔥 Ensure the correct client owns this object
            newPlayerObject.ChangeOwnership(clientId);

            Debug.Log($"[OnClientConnected] {clientId} connected, Assigned Ownership: {newPlayerObject.OwnerClientId}");
            
            Debug.Log($"{players.Count}");
        }
    }


    void OnClientDisconnected(ulong clientId)
    {
        if (IsHost)
        {
            for (int i = clients.Count - 1; i >= 0; i--)
                if (clients[i].ClientId == clientId)
                {
                    //Debug.Log(NetworkManager.Singleton.SpawnManager.SpawnedObjects[clientId].name);
                    //players[clientId].gameObject.GetComponentInChildren<NetworkObject>().Despawn(true);
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

    public void resetGame()
    {
        if (IsServer)
        {
            Debug.Log("[resetGame] Resetting game...");
            CheckWinner();
            foreach (PlayerManager player in players.Values)
            {
                player.cardNums.Clear(); // Clear previous cards
                player.gameObject.GetComponent<UserBlockManager>().HideCards();
            }

            // Shuffle and deal new cards
            cards.Shuffle();
            AssignNewCards();
        }
    }

    
    void AssignNewCards()
    {
        if (IsServer)
        {
            int i = 0;
            foreach (var playerEntry in players)
            {
                var player = playerEntry.Value;

                // Assign new cards
                player.cardNums.Add(cards[i].CardID);
                player.cardNums.Add(cards[i + 1].CardID);
                i += 2;
            }
        }
    }

    void CheckWinner()
    {
        if (IsServer)
        {
            Dictionary<ulong, int> winners = new Dictionary<ulong, int>();

            foreach (HandManager hand in FindObjectsByType<HandManager>(FindObjectsSortMode.None))
            {
                ulong clientId = hand.gameObject.GetComponent<NetworkObject>().OwnerClientId;
                int score = hand.handNumber();
                winners.Add(clientId, score);
                Debug.Log($"[CheckWinner] Player {clientId} has won {score}");
            }

            // Determine the winner (Highest Score Wins)
            ulong winnerId = winners.OrderByDescending(entry => entry.Value).First().Key;

            Debug.Log($"[CheckWinner] Winner is Client {winnerId} with Score {winners[winnerId]}");

            // Notify the winner
            HitWinnerRpc(winnerId);

            // Reveal all players' cards after 2 seconds (for effect)
            RevealAllCards();
        }
    }


    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    void HitWinnerRpc(ulong clientId)
    {
        Debug.Log($"[HitWinnerRpc] Received RPC. Checking if LocalClientId ({NetworkManager.Singleton.LocalClientId}) matches winnerId ({clientId})");

        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log($"[HitWinnerRpc] Local client is the winner! Updating UI.");
        
            if (players.ContainsKey(clientId))
            {
                players[clientId].gameObject.GetComponent<UserBlockManager>().HitCard();
            }
            else
            {
                Debug.LogError($"[HitWinnerRpc] PlayerManager for client {clientId} not found!");
            }
        }
    }


    
    //TODO: Clients can see all cards but Host can not fix this.
    
    void RevealAllCards()
    {
        if (IsServer)
        {
            foreach (var player in players)
            {
                ulong playerId = player.Key;
                //Debug.Log($"[RevealAllCards] Sending cards of Player {playerId}: {string.Join(", ", cardIds)}");

                // Notify all clients about each player's cards
                ShowOpponentCardsRpc(playerId);
            }
        }
    }


    
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    void ShowOpponentCardsRpc(ulong playerId)
    {
        Debug.Log($"[ShowOpponentCardsRpc] Revealing Player {playerId}'s cards. LocalClientId: {NetworkManager.Singleton.LocalClientId}");

        foreach (UserBlockManager block in FindObjectsByType<UserBlockManager>(FindObjectsSortMode.None))
        {
            ulong ownerId = block.GetComponent<NetworkObject>().OwnerClientId;
            Debug.Log($"[ShowOpponentCardsRpc] Checking UI block for player {ownerId}");

            // Ensure correct owner is being revealed
            if (ownerId == playerId)
            {
                Debug.Log($"[ShowOpponentCardsRpc] Updating UI for Player {ownerId}");
                block.ShowCards();
            }
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
