using System;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using BeWild.Framework.Runtime.Utils.UI;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Today
{
    public class TrendingBookListGroup : MonoBehaviour
    {
        [SerializeField] private VerticalScrollPageGroup verticalScroll;
        [SerializeField] private BookListUI trendingBookListPrefab;

        [SerializeField] private Transform _listParent;
        private Action<int> _bookClickEvent;
        private Action<int> _turnPageEvent;
        private List<BookListUI> _bookListUis;

        public void Initialize(Action<int> callback, Action<int> turnPageCallback)
        {
            _bookListUis = new List<BookListUI>();
            _bookClickEvent = callback;
            _turnPageEvent = turnPageCallback;
        }

        public void InitList(int number)
        {
            if (number > _bookListUis.Count)
            {
                int index=number-_bookListUis.Count;
                for (int i = 0; i < index; i++)
                {
                    BookListUI tmpUI = Instantiate(trendingBookListPrefab, _listParent, false);
                    RectTransform rectTransform = tmpUI.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, _listParent.GetComponent<RectTransform>().rect.height);
                    tmpUI.Initialize(_bookClickEvent, null);
                    _bookListUis.Add(tmpUI);

                    verticalScroll.SetFocusScrollEventHooker(tmpUI.transform);
                }
            }
            else
            {
                int index=_bookListUis.Count-number;
                for(int i=0;i<index;i++)
                {
                    BookListUI tmpUI = _bookListUis[_bookListUis.Count - 1];
                    _bookListUis.Remove(tmpUI);
                    Destroy(tmpUI.gameObject);
                }
            }

            verticalScroll.Initialize(_turnPageEvent);
        }

        public void Clear()
        {
            foreach (BookListUI ui in _bookListUis)
            {
                Destroy(ui.gameObject);
            }

            _bookListUis.Clear();
        }

        public List<BookListUI> GetAllBookListUI()
        {
            return _bookListUis;
        }

        public void TurnToTargetPage(int index)
        {
            verticalScroll.TurnToTargetPage(index, false);
        }
    }
}