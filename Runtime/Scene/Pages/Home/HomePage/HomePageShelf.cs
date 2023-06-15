using System;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Data;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class HomePageShelf : MonoBehaviour
    {
        private const float DesiredDelay = 0.1f;
        
        [SerializeField] private HomePageShelfBook[] books;
        [SerializeField] private Button refreshButton;

        private bool _refreshing;
        
        public void Initialize(Action refreshCallback, Action<int> bookTapCallback)
        {
            refreshButton.onClick.AddListener(() => refreshCallback?.Invoke());
            foreach (HomePageShelfBook book in books)
            {
                book.Initialize(bookTapCallback);
            }
        }

        public void SetData(List<BookBriefData> data, Action callback)
        {
            int pendingCount = 0;
            for (int i = 0; i < books.Length; i++)
            {
                BookBriefData d = i < data.Count ? data[i] : null;
                books[i].ToggleVisual(d != null);

                if (d != null)
                {
                    pendingCount++;
                    books[i].PlayFlipAnimation(d.id, d.author, d.icon, i * DesiredDelay, () =>
                    {
                        pendingCount--;
                        if (pendingCount <= 0)
                        {
                            callback?.Invoke();
                        }
                    });
                }
            }
        }

        public void ToggleRefreshState(bool refreshing)
        {
            _refreshing = refreshing;
        }

        public bool IsRefreshing()
        {
            return _refreshing;
        }
    }
}