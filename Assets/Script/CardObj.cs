using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardObj : MonoBehaviour
{
    public CardData _cardData;
    Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void SetCardData()
    {
        _image.sprite = _cardData.Sprite();
    }
}
