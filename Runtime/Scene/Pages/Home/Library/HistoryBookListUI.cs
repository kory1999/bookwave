using System;
using System.Collections;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Library
{
    public class HistoryBookListUI : BookListUI
    {
        
        private Action  _popupShowEvent;
        private Action<int> _popupFinishBookEvent;
        private Action<int> _popupShareBookEvent;
        private Action<int> _popupDeleteBookEvent;

        public void SetCallbackForHistoryBook(Action showCallback, Action<int> finishCallback,
            Action<int> shareCallback, Action<int> deleteCallback)
        {
            _popupShowEvent = showCallback;
            _popupFinishBookEvent = finishCallback;
            _popupShareBookEvent = shareCallback;
            _popupDeleteBookEvent = deleteCallback;
        }
        
        public void SetAllClosePopup()
        {
            
            for (int i = 0; i < _books.Count; i++)
            {
                if (_books[i] is LibraryHistoryBook)
                {
                    (_books[i] as LibraryHistoryBook).CloseHistoryPopup();
                }
                
            }
        }
        
        protected override void RefreshVisual(bool clearOldBooks = true)
        {
            if (clearOldBooks)
            {
                for (int i = _books.Count; i > _data.books.Count; i--)
                {
                    Destroy(_books[i - 1].gameObject);
                    _books.RemoveAt(i - 1);
                }
            }

            // set data for groups
            for (int i = 0; i < _data.books.Count; i++)
            {
                // instantiate new group if there is not enough in list
                if (_books.Count - 1 < i)
                {
                    BookListBook newBook = Instantiate(bookPrefab.gameObject, bookPrefab.transform.parent)
                        .GetComponent<BookListBook>();
                    newBook.gameObject.SetActive(true);
                    newBook.Initialize(HandleOnBookTap);
                    
                    if (newBook is LibraryHistoryBook)
                    {
                        (newBook as LibraryHistoryBook).SetCallback(_popupShowEvent, _popupFinishBookEvent,
                            _popupShareBookEvent, _popupDeleteBookEvent);
                    }

                    _books.Add(newBook);
                }

                BookBriefData briefData = _data.books[i];
                _books[i].SetData(briefData.id, briefData.name, briefData.author, briefData.icon,briefData.isFree);
            }
        }
    }

}
