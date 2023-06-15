using BeWild.AIBook.Runtime.Data;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public interface ISearchPageLogic
    {
        void Initialize(BookListData data, object param = null);
        void SetSearchPage(SearchPage searchPage);
        void StartLogic();
        void TryFetchMore();
        void Stop();
        string GetBookTapTrackingEvent();
    }
}