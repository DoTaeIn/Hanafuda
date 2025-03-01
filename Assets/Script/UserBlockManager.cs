using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UserBlockManager : MonoBehaviour
{
    PlayerManager playerManager;
    GameManager gameManager;
    
    [SerializeField] Image backGround;
    [SerializeField] GameObject handObj;
    [SerializeField] Sprite backImage;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        gameManager = FindObjectOfType<GameManager>();
    }

    public void Expand()
    {
        handObj.SetActive(true);
        backGround.rectTransform.sizeDelta = new Vector2(400, 450);
        backGround.rectTransform.localPosition = new Vector2(0, 0);
    }

    public void Shrink()
    {
        handObj.SetActive(false);
        backGround.rectTransform.sizeDelta = new Vector2(400, 165);
        backGround.rectTransform.localPosition = new Vector2(0, 0);
    }

    public void ShowCards()
    {
        Expand();
        if (handObj.activeSelf)
        {
            Image[] cardImages = handObj.GetComponentsInChildren<Image>();
            Debug.Log(cardImages.Length);
            cardImages[0].sprite = GetCardSprite(playerManager.cardNums[0]);
            cardImages[1].sprite = GetCardSprite(playerManager.cardNums[1]);
        }
    }

    public void HideCards()
    {
        Image[] cardImages = handObj.GetComponentsInChildren<Image>();

        cardImages[0].sprite = backImage;
        cardImages[1].sprite = backImage;
    }
    
    
    private Sprite GetCardSprite(int cardId)
    {
        CardData card = FindCardById(cardId);
        return card != null ? card.Sprite() : null;
    }
    
    private CardData FindCardById(int cardId)
    {
        return gameManager.cards.FirstOrDefault(c => c.CardID == cardId);
    }
}
