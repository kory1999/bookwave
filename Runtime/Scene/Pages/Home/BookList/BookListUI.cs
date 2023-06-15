using System;
using System.Collections.Generic;
using System.Linq;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Scene.Pages.Home.Library;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.BookList
{
    public class BookListUI : MonoBehaviour
    {
        private const float TriggerScrollEndOffset = 150f;

        [SerializeField] protected BookListBook bookPrefab;

        protected BookListData _data;
        protected List<BookListBook> _books = new List<BookListBook>();
        private Action<int> _bookTapCallback;
        private Action _onScrollReachEnd;
        private ScrollRect _scroll;
        private Vector2 _sendScrollEndEventValue;
        private bool _sendScrollEndEvent;
        private RectTransform _contentRect;

        #region interface

        public void Initialize(Action<int> onBookTap, Action onScrollReachEnd)
        {
            bookPrefab.gameObject.SetActive(false);

            _scroll = GetComponentInChildren<ScrollRect>(true);
            _scroll.onValueChanged.AddListener(HandleOnScrollValueChanged);

            _bookTapCallback = onBookTap;
            _onScrollReachEnd = onScrollReachEnd;

            _contentRect = _scroll.content;
        }

        public void AddBooks(BookListData data)
        {
            if (_data == null)
            {
                _data = data;
            }
            else
            {
                _data.books.AddRange(data.books);

                _data.books = _data.books.Distinct().ToList();
            }

            RefreshVisual(false);
        }

        public void ClearBooks()
        {
            foreach (var child in GetComponentsInChildren<BookListBook>(true))
            {
                if (child != bookPrefab)
                {
                    Destroy(child.gameObject);
                }
            }

            _books.Clear();
            _data = null;

            _scroll.normalizedPosition = new Vector2(0f, 1f);
        }

        public bool IsEmpty()
        {
            return _data != null && (_data.books == null || _data.books.Count == 0);
        }

        public void UpdateBooks(List<BookBriefData> books)
        {
            _data ??= new BookListData()
            {
                books = new List<BookBriefData>()
            };

            if (books != null)
            {
                _data.books = books;
            }

            RefreshVisual();
        }

        public List<BookListBook> GetAllBooks()
        {
            return _books;
        }

        #endregion

        #region private

        protected virtual void RefreshVisual(bool clearOldBooks = true)
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
                    _books.Add(newBook);
                }

                BookBriefData briefData = _data.books[i];
                _books[i].SetData(briefData.id, briefData.name, briefData.author, briefData.icon, briefData.isFree);
            }
        }

        protected void HandleOnBookTap(int id)
        {
            _bookTapCallback?.Invoke(id);
        }

        private void HandleOnScrollValueChanged(Vector2 value)
        {
            if (_scroll.horizontal)
            {
                CheckValue(value.x, -TriggerScrollEndOffset / _contentRect.rect.width);
            }
            else if (_scroll.vertical)
            {
                CheckValue(value.y, -TriggerScrollEndOffset / _contentRect.rect.height);
            }

            void CheckValue(float currentValue, float endValue)
            {
                if (_sendScrollEndEvent && currentValue >= 0f)
                {
                    _sendScrollEndEvent = false;
                }
                else if (!_sendScrollEndEvent && currentValue < endValue)
                {
                    _sendScrollEndEvent = true;

                    _onScrollReachEnd?.Invoke();
                }
            }
        }

        #endregion
    }
}