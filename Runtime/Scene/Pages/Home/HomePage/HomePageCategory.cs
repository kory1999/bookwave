using System;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using TMPro;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class HomePageCategory : RawImageHolder
    {
        private int _id;
        private bool _initialized;

        public int ID => Data.id;

        protected CategoryData Data;
        
        public virtual void Initialize(CategoryData data, Action<int> tapCallback)
        {
            Data = data;
            
            if (!string.IsNullOrEmpty(data.name))
            {
                GetComponentInChildren<TMP_Text>().text = data.name;
            }

            if (!string.IsNullOrEmpty(data.iconUrl))
            {
                SetTexture(data.iconUrl);
            }

            if (!_initialized)
            {
                _initialized = true;
                
                GetComponent<Button>().onClick.AddListener(() => tapCallback?.Invoke(ID));
            }
        }
    }
}