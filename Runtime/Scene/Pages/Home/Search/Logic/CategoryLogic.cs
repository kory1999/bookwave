using System.Collections.Generic;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage.Logic
{
    public class CategoryLogic : ISearchPageLogic
    {
        private SearchPage _searchPage;
        private BookListData _data;
        private int _categoryId;

        public void Initialize(BookListData data, object param)
        {
            _data = data;

            _categoryId = (int)param;

            _data.books ??= new List<BookBriefData>();
            _data.currentPageIndex = 0;
        }

        public void SetSearchPage(SearchPage searchPage)
        {
            _searchPage = searchPage;
        }

        public void StartLogic()
        {
            _searchPage.SetCurrentCategoryId(_categoryId);
            _searchPage.ShowBookPage();
            _searchPage.ShowCategory(_categoryId);
            _searchPage.ClearBooks();
            _searchPage.AddBooks(_data, false);
            
            DoFetchData();
        }

        public void TryFetchMore()
        {
            if (!IsAllBooksLoaded())
            {
                DoFetchData();
            }
        }

        public void Stop()
        {
            _searchPage = null;
            _data = null;
        }

        private bool IsAllBooksLoaded()
        {
            return _data.books.Count >= _data.totalCount;
        }

        public string GetBookTapTrackingEvent()
        {
            return BookwavesAnalytics.Event_Home_ClickCategoryBook;
        }

        private void DoFetchData()
        {
            _searchPage.ToggleLoadingHint(true);
            
            int index = _data.currentPageIndex + 1;
            GlobalEvent.GetEvent<GetBooksInCategoryDataEvent>().Publish(_categoryId, index, data =>
            {
                if (_searchPage == null || _data == null)
                {
                    return;
                }
                
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