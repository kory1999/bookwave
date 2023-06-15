using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage;
using BW.Framework.Utils;

namespace BeWild.AIBook.Runtime.Scene
{
    public class MainSceneEvent : EventAggregator {}
    
    public class OpenBookEvent : EventBase<int>{}
    public class OpenBookBriefEvent : EventBase<BookBriefData>{}
    
    // try to open book content, by brief data, chapter index, if open text page
    public class OpenBookContentEvent : EventBase<BookBriefData, int, bool,double>{}
    public class OpenHomePageEvent : EventBase{}
    
    // logic, sorting order
    public class OpenSearchPageEvent : EventBase<ISearchPageLogic, int>{};
    public class SetSearchPageSortingOrderEvent : EventBase<int>{};
}