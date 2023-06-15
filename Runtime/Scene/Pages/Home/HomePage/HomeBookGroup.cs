using System;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class HomeBookGroup : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private Button moreButton;
        [SerializeField] private BookListUI bookList;

        private int _groupId;

        public void Initialize(Action<int> moreButtonTap, Action<int> bookTap)
        {
            moreButton.onClick.AddListener(HandleOnMoreButtonTap);
            bookList.Initialize(bookTap, null);

            void HandleOnMoreButtonTap()
            {
                moreButtonTap?.Invoke(_groupId);
            }
        }

        public void SetData(BookListData data)
        {
            if (data != null)
            {
                _groupId = data.bannerId;
            
                title.text = data.title;
            
                bookList.ClearBooks();
                bookList.AddBooks(data);
            }
        }
    }
}