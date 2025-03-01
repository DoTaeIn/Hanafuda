using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : NetworkBehaviour
{
    HandManager handManager;
    GameManager gameManager;
    
    Button betButton;
    Button dieButton;
    
    public List<CardData> cardNums = new List<CardData>();
    public int betMoney;
    public int myMoney = 100;
    private void Awake()
    {
        handManager = GetComponent<HandManager>();
        gameManager = FindObjectOfType<GameManager>();
        
    }
    
    
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Debug.Log(gameObject.name + " spawned");
            betButton = GameObject.Find("Bet Button").GetComponent<Button>();
            betButton.onClick.AddListener(betmoney);
            dieButton = GameObject.Find("Die Button").GetComponent<Button>();
        }
        else
        {
            Debug.Log(gameObject.name + " not spawned");
        }
    }
    


    public void betmoney()
    {
        gameManager.betToPotServerRpc(10);   
    }


    [Rpc(SendTo.Server)]
    private void setMoneyServerRpc(int amt)
    {
            
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void setMoneyClientrRpc(int amt)
    {
        
    }

    [Rpc(SendTo.Server)]
    private void BetMoneyServerRpc(int amt)
    {
        betMoney += amt;
        myMoney -= amt;
        if(IsOwner)
            BetMoneyClientRpc(amt);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void BetMoneyClientRpc(int amt)
    {
        betMoney += amt;
        myMoney -= amt;
    }
}
