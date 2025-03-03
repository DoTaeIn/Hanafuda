using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class HandManager : MonoBehaviour
{
    PlayerManager playerManager;
    UserBlockManager userBlockManager;
    public List<CardData> hands = new List<CardData>();
    
    private List<List<m_Month>> tree = new List<List<m_Month>>
    {
        new List<m_Month> { m_Month.April , m_Month.June },
        new List<m_Month> { m_Month.October, m_Month.April },
        new List<m_Month> { m_Month.January, m_Month.October },
        new List<m_Month> { m_Month.January, m_Month.September },
        new List<m_Month> { m_Month.January, m_Month.April },
        new List<m_Month> { m_Month.January, m_Month.February },
        new List<m_Month> { m_Month.April, m_Month.September }
    };
    
    
    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        userBlockManager = GetComponent<UserBlockManager>();
        
    }

    private void Start()
    {
        
    }


    bool AreListsEqual<T>(List<T> list1, List<T> list2)
    {
        return list1.Count == list2.Count && !list1.Except(list2).Any() && !list2.Except(list1).Any();
    }
    
    private int isInTree()
    {
        for (int i = 0; i < tree.Count; i++)
        {
            List<m_Month> temp = new List<m_Month>
            {
                hands[0].Month, hands[1].Month
            };
            
            
            if(AreListsEqual(tree[i], temp))
                return i+10;
        }
        return -1;
    }

    public int handNumber()
    {
        CardData card1 = userBlockManager.FindCardById(playerManager.cardNums[0]);
        CardData card2 = userBlockManager.FindCardById(playerManager.cardNums[1]);

        if (card1 == null || card2 == null)
        {
            Debug.LogError("FindCardById returned null for one or both cards.");
        }

        hands.Add(card1);
        hands.Add(card2);
        
        
        if(hands[0].isLight() && hands[1].isLight())
            if (hands[0].Month == m_Month.March || hands[1].Month == m_Month.March)
                return 30;
            else 
                return 29;
        if(hands[0].Month == hands[1].Month)
            return (int)hands[0].Month + 18;
        if(isInTree() == -1)
            return ((int)hands[0].Month + (int)hands[1].Month + 2) % 10;
        
        return isInTree();
    }
    
    
    
}
