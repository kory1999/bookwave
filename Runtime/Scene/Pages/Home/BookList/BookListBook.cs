using System;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.BookList
{
    public class BookListBook : RawImageHolder
    {
        [SerializeField] private Text nameText, authorText;
        [SerializeField] private Button button;

        private int _id;
        protected bool _isFree;
        public int Id => _id;

        public virtual void Initialize(Action<int> tapCallback)
        {
            button.onClick.AddListener(() => tapCallback?.Invoke(_id));
        }

        public virtual void SetData(int id, string bookName, string author, string imageUrl,bool isFree)
        {
            _id = id;
            _isFree = isFree;

            if (nameText != null)
            {
                nameText.text = bookName;
            }
            
            if (authorText != null)
            {
                authorText.text = author;
            }

            SetTexture(imageUrl);
        }
    }
}