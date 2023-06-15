using System;
using System.Collections;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using BW.Framework.Utils;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class SearchPageDefaultPage : MonoBehaviour
    {
        [SerializeField] private RectTransform tagParent;
        [SerializeField] private SearchPageTrendingTag tagPrefab;
        [SerializeField] private BookListBook book;

        private List<SearchPageTrendingTag> _trendingTags = new List<SearchPageTrendingTag>();
        private List<BookListBook> _books = new List<BookListBook>();
        private SearchPageData _data;
        private Action<string> _searchTextTap;
        private Action<int> _bookTapCallback;

        private bool _forceUpdateMark;

        public void Initialize(Action<string> searchTap, Action<int> bookTap)
        {
            _searchTextTap = searchTap;
            _bookTapCallback = bookTap;
            
            tagPrefab.gameObject.SetActive(false);
            book.gameObject.SetActive(false);
        }

        public void SetForceUpdateFlag()
        {
            _forceUpdateMark = true;
        }

        public void Show()
        {
            if (_data == null || _forceUpdateMark)
            {
                _forceUpdateMark = false;
                GlobalEvent.GetEvent<GetSearchPageDataEvent>().Publish(true, HandleOnSearchPageDataReceived);
            }
            else
            {
                ToggleVisual(true);
            }
        }

        public void Hide()
        {
            ToggleVisual(false);
        }

        private void ToggleVisual(bool on)
        {
            GetComponent<CanvasGroup>().ToggleEnable(on);
        }

        private void HandleOnSearchPageDataReceived(SearchPageData data)
        {
            _data = data;
            
            ToggleVisual(true);

            UpdateTrendingTags();
            
            UpdateBooks();
        }

        private void UpdateTrendingTags()
        {
            for (int i = _trendingTags.Count; i > _data.TrendingSearch.Length; i--)
            {
                Destroy(_trendingTags[i - 1].gameObject);
                _trendingTags.RemoveAt(i - 1);
            }
            
            for (int i = 0; i < _data.TrendingSearch.Length; i++)
            {
                if (_trendingTags.Count - 1 < i)
                {
                    SearchPageTrendingTag newTag = Instantiate(tagPrefab.gameObject, tagPrefab.transform.parent)
                        .GetComponent<SearchPageTrendingTag>();
                    newTag.gameObject.SetActive(true);
                    newTag.Initialize(value =>
                    {
                        GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Prefix_Search_ClickTrending);

                        _searchTextTap?.Invoke(value);
                    });
                    _trendingTags.Add(newTag);
                }

                _trendingTags[i].SetData(_data.TrendingSearch[i], i == 0);
            }

            StartCoroutine(DelayRefreshRectTransformSize());
        }

        private IEnumerator DelayRefreshRectTransformSize()
        {
            yield return null;

            float interval = 30f;
            float totalWidth = tagParent.rect.width;
            float lineInterval = _trendingTags[0].GetComponent<RectTransform>().rect.height + interval;
            // update layout based on size
            float lineEnd = 50f;
            int lineIndex = 0;
            bool nextLine = false;
            float maxWith = 0;
            foreach (var t in _trendingTags)
            {
                RectTransform cRect = t.GetComponent<RectTransform>();
                float cWidth = cRect.rect.width;

                if (lineEnd + interval + cWidth > totalWidth)
                {
                    if (nextLine)
                    {
                        lineIndex++;
                        lineEnd = 50;
                        nextLine = false;
                    }
                    else
                    {
                        nextLine = true;
                    }
                }

                cRect.anchoredPosition = new Vector2(lineEnd, -lineIndex * lineInterval);
                lineEnd += interval + cWidth;
                
                if (lineEnd > maxWith)
                {
                    maxWith = lineEnd;
                }
            }

            tagParent.sizeDelta = new Vector2(maxWith, (lineIndex + 1) * lineInterval);
        }

        private void UpdateBooks()
        {
            for (int i = _books.Count; i > _data.HotRecent.Count; i--)
            {
                Destroy(_books[i - 1].gameObject);
                _books.RemoveAt(i - 1);
            }
            
            for (int i = 0; i < _data.HotRecent.Count; i++)
            {
                if (_books.Count - 1 < i)
                {
                    BookListBook newBook = Instantiate(book.gameObject, book.transform.parent)
                        .GetComponent<BookListBook>();
                    newBook.gameObject.SetActive(true);
                    newBook.Initialize(id =>
                    {
                        GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Prefix_Search_ClickHotBook);

                        _bookTapCallback.Invoke(id);
                    });
                    _books.Add(newBook);
                }

                BookBriefData data = _data.HotRecent[i];
                _books[i].SetData(data.id, data.name, data.author, data.icon,data.isFree);
            }
        }
    }
}