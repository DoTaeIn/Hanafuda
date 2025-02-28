using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum m_Month
{
    January,
    February,
    March,
    April,
    May,
    June,
    July,
    August,
    September,
    October,
    November,
    December
}

public enum m_Type
{
    Default,
    Animal,
    Stripe,
    Light
}

public enum m_Stripe_Type
{
    Red_Poetry,
    Blue_Poetry,
    Red_Stripe
}

[CreateAssetMenu(fileName = "new Card", menuName = "Cards")]
public class CardData : ScriptableObject
{
    [SerializeField] private Sprite m_Sprite;
    [SerializeField] private m_Month month = m_Month.January;
    [SerializeField] private m_Type type = m_Type.Default;
    
    [EnumCondition("type", (int)m_Type.Stripe)]
    [SerializeField] private m_Stripe_Type stripeType = m_Stripe_Type.Red_Poetry;
    
    [EnumCondition("month", (int)m_Month.November, (int)m_Month.December)]
    [SerializeField] private bool isDouble;
    
    public Sprite Sprite() { return m_Sprite; }
    public m_Month Month { get => month; }
    public m_Type Type { get => type; }
    
    public bool isLight() {return type == m_Type.Light;}

    public m_Stripe_Type? StripeType()
    {
        return type == m_Type.Default ? (m_Stripe_Type?)null : stripeType;
    }
    
    
}
