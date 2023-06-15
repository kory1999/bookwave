using System.Collections.Generic;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage.Logic
{
    public class BookGroupLogic : ISearchPageLogic
    {
        private SearchPage _searchPage;
        private BookListData _data;

        public void Initialize(BookListData data, object param = null)
        {
            _data = data;

            _data.books ??= new List<BookBriefData>();
            _data.currentPageIndex = 1;
        }

        public void SetSearchPage(SearchPage searchPage)
        {
            _searchPage = searchPage;
        }

        public void StartLogic()
        {
            _searchPage.ShowBookPage();
            _searchPage.ShowTitle(_data.title);
            _searchPage.ClearBooks();
            _searchPage.AddBooks(_data);
            
            UpdateEndMark();
        }

        public void TryFetchMore()
        {
            if (!IsAllBooksLoaded())
            {
                RequestMoreBooks();
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
            return BookwavesAnalytics.Event_Home_ClickBookGroupBook;
        }

        private void RequestMoreBooks()
        {
            _searchPage.ToggleLoadingHint(true);
            
            int index = _data.currentPageIndex + 1;
            GlobalEvent.GetEvent<GetHomeMoreDataEvent>().Publish(_data.bannerId, index, data =>
            {
                if (_searchPage == null || _data == null)
                {
                    return;
                }
                
                // notice that the book list in search page is the same one as _data, so we don't add book here
                // _data.books.AddRange(data.books);
                
                _data.totalCount = data.totalCount;
                data.currentPageIndex = index;
                
                _searchPage.ToggleLoadingHint(false);
                
                _searchPage.AddBooks(new BookListData()
                {
                    books = data.books,
                    currentPageIndex = index,
                    totalCount = data.totalCount
                });
                
                UpdateEndMark();
            });
        }

        private void UpdateEndMark()
        {
            _searchPage.ToggleEndMark(IsAllBooksLoaded() && _data.books.Count > 0);
        }
    }
}