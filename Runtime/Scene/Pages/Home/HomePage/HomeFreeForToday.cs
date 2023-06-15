using System;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class HomeFreeForToday : RawImageHolder
    {
        [SerializeField] private Button button;

        public void Initialize(Action tapCallback)
        {
            button.onClick.AddListener(() => tapCallback?.Invoke());
        }

        public void SetData(string bookNameText, string authorText, string imageUrl)
        {
            SetTexture(imageUrl);
        }
    }
}