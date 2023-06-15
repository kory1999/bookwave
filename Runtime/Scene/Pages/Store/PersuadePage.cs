using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Store
{
    public class PersuadePage : MonoBehaviour
    {
        [SerializeField] private Text priceText;
        [SerializeField] private Button purchaseButton;

        public bool Shown = false;
        
        public void Show(string priceString, Action purchaseCallback)
        {
            Shown = true;
            priceText.text = priceText.text.Replace(StorePage.PriceTag, priceString);
            
            purchaseButton.onClick.AddListener(() => purchaseCallback?.Invoke());
        }
    }
}