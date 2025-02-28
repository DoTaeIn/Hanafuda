using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class HandManager : MonoBehaviour
{
    private List<CardObj> hands = new List<CardObj>();
    
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
        hands = GetComponentsInChildren<CardObj>().ToList();
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
                hands[0]._cardData.Month, hands[1]._cardData.Month
            };
            
            
            if(AreListsEqual(tree[i], temp))
                return i+10;
        }
        return -1;
    }

    public int handNumber()
    {
        if(hands[0]._cardData.isLight() && hands[1]._cardData.isLight())
            if (hands[0]._cardData.Month == m_Month.March || hands[1]._cardData.Month == m_Month.March)
                return 30;
            else 
                return 29;
        if(hands[0]._cardData.Month == hands[1]._cardData.Month)
            return (int)hands[0]._cardData.Month + 18;
        if(isInTree() == -1)
            return ((int)hands[0]._cardData.Month + (int)hands[1]._cardData.Month + 2) % 10;
        return isInTree();
    }
    
    
    
}
