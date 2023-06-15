using System.Collections.Generic;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.Framework.Runtime.Analytics;
using BW.Framework.Utils;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage.Logic
{
    public class SearchLogic : ISearchPageLogic
    {
        private SearchPage _searchPage;
        private BookListData _data;
        private string _searchValue;

        public void Initialize(BookListData data, object param)
        {
            _data = data;

            _searchValue = (string)param;

            _data.books ??= new List<BookBriefData>();
            _data.currentPageIndex = 0;
        }

        public void SetSearchPage(SearchPage searchPage)
        {
            _searchPage = searchPage;
        }

        public void StartLogic()
        {
            _searchPage.ShowBookPage();
            _searchPage.ClearBooks();

            if (!string.IsNullOrEmpty(_searchValue))
            {
                _searchPage.AddBooks(_data, false);
            
                DoSearch();
            }
        }

        public void TryFetchMore()
        {
            if (!IsAllBooksLoaded())
            {
                DoSearch();
            }
        }

        public void Stop()
        {
            _searchPage = null;
            _data = null;
        }

        public string GetBookTapTrackingEvent()
        {
            return BookwavesAnalytics.Event_Home_ClickSearchBook;
        }

        private bool IsAllBooksLoaded()
        {
            return _data.books.Count >= _data.totalCount;
        }

        private void DoSearch()
        {
            _searchPage.ToggleLoadingHint(true);
            
            int index = _data.currentPageIndex + 1;
            GlobalEvent.GetEvent<SearchBooksEvent>().Publish(_searchValue, index, data =>
            {
                if (_searchPage == null || _data == null)
                {
                    return;
                }

                if (data == null)
                {
                    BaseLogger.Log(nameof(SearchLogic), "SearchBooksEvent data is null,the search value is " + _searchValue, LogType.Error);
                    return;
                }
                
                GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_Home_SearchResult);
                AdjustAnalytics.PublishEvent(AdjustAnalytics.ADEvent_Search_ResultShow);

                _data.books.AddRange(data.books);
                _data.totalCount = data.totalCount;
                _data.currentPageIndex = index;
                
                _searchPage.ToggleLoadingHint(false);
                _searchPage.ToggleEndMark(IsAllBooksLoaded() && _data.books.Count > 0);
                
                _searchPage.AddBooks(new BookListData()
                {
                    books = data.books,
                    totalCount = data.totalCount,
                    currentPageIndex = index
                });
            });
        }
    }
}