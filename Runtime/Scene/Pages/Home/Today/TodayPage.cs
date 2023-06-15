using System.Collections.Generic;
using System.Linq;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using BW.Framework.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Today
{
    public class TodayPage : HomeView
    {
        [SerializeField] private CanvasGroup normalGroup, errorGroup;
        [SerializeField] private Button refreshButton;
        [SerializeField] private TodayTabGroup tabGroup;
         [SerializeField] private TrendingBookListGroup trendingBookListGroup;
        [SerializeField] private Button searchButton;

        private BookSearchData _data;
        private List<BookBriefData> _books;
        private Dictionary<string, List<BookBriefData>> _trendingDictionary;
        private List<string> _tabButtonNames;
        private int _currentTabIndex;

        public override void Initialize()
        {
            _tabButtonNames = new List<string>();
            _trendingDictionary = new Dictionary<string, List<BookBriefData>>();

            tabGroup.Initialize(HandleOnTabButtonTap);
            trendingBookListGroup.Initialize(HandleOnBookTap,TurnPage);
            
            searchButton.onClick.AddListener(OpenSearchPage);

            normalGroup.ToggleEnable(false);
            errorGroup.ToggleEnable(false);
            refreshButton.onClick.AddListener(HandleOnRefreshButton);

            GlobalEvent.GetEvent<LanguageUpdateEvent>().Subscribe(HandleOnLanguageUpdate);
        }

        public override void ToggleVisual(bool on)
        {
            base.ToggleVisual(on);

            if (_books == null)
            {
                RefreshHomeData();
            }
        }

        private void HandleOnLanguageUpdate()
        {
            _books = null; // clear data because we fetch backend data based on language type
            _trendingDictionary.Clear();
            _tabButtonNames.Clear();
            trendingBookListGroup.Clear();
            tabGroup.Clear();
        }

        private void RefreshHomeData()
        {
            GlobalEvent.GetEvent<GetTrendingEvent>().Publish(1, true, HandleOnHomeDataReceived);
        }

        private void HandleOnRefreshButton()
        {
            TrackEvent(BookwavesAnalytics.Event_Network_ClickRefresh);

            errorGroup.ToggleEnable(false);

            RefreshHomeData();
        }

        private void HandleOnHomeDataReceived(BookSearchData data)
        {
            bool isValidData = IsValidData(data);
            bool showLayout = isValidData || _data != null;

            errorGroup.ToggleEnable(!showLayout);
            normalGroup.ToggleEnable(showLayout);
            
            _trendingDictionary.Clear();
            _tabButtonNames.Clear();

            if (showLayout)
            {
                if (isValidData)
                {
                    _data = data;
                
                    for (int i = 0; i < data.books.Count; i++)
                    {
                        string trendingType = data.books[i].trendingGroup;

                        if (_trendingDictionary.ContainsKey(trendingType))
                        {
                            _trendingDictionary[trendingType].Add(data.books[i]);
                        }
                        else
                        {
                            _tabButtonNames.Add(trendingType);
                            _trendingDictionary.Add(trendingType, new List<BookBriefData>());
                            _trendingDictionary[trendingType].Add(data.books[i]);
                        }
                    }
                
                    tabGroup.InitButton(_tabButtonNames.Count);
                    trendingBookListGroup.InitList(_tabButtonNames.Count);
                    tabGroup.RefreshButton(_tabButtonNames);
                    UpdateVisual();
                }
            }
            else
            {
                TrackEvent(BookwavesAnalytics.Event_Network_ShowError);
            }
        }

        private bool IsValidData(BookSearchData data)
        {
            return data != null && data.books != null && data.books.Count > 0;
        }

        protected virtual void UpdateVisual()
        {
            
            List<BookListUI> tmpList = trendingBookListGroup.GetAllBookListUI();
            for (int i = 0; i < tmpList.Count; i++)
            {
                _books=_books = _trendingDictionary[_tabButtonNames[i]];
                tmpList[i].UpdateBooks(_trendingDictionary[_tabButtonNames[i]]);
                
                List<BookListBook> tmp = new List<BookListBook>();
                tmp = tmpList[i].GetAllBooks();
                for (int j = 0; j < tmp.Count; j++)
                {
                    if (tmp[j] is TrendingBook trendingBook)
                    {
                        trendingBook.SetDetailData(j, _books[j].author, _books[j].trendingInfo);
                    }
                }
            }
        }

        protected virtual void HandleOnBookTap(int id)
        {
            int index = _books.FindIndex(b => b.id == id) + 1;

            TrackEvent($"{BookwavesAnalytics.Prefix_Today_ClickTabBook}_{_tabButtonNames[_currentTabIndex]}_click_{index}");

            MainScene.Event.GetEvent<OpenBookEvent>().Publish(id);
        }

        //Click tab button to refresh
        private void HandleOnTabButtonTap(int index)
        {
            _currentTabIndex = index;
            trendingBookListGroup.TurnToTargetPage(index);
            TrackEvent(BookwavesAnalytics.Prefix_Today_ClickTab + _tabButtonNames[index]);
        }

        private void TurnPage(int i)
        {
            tabGroup.SelectTabByIndex(i);
        }

        private void OpenSearchPage()
        {
            TrackEvent(BookwavesAnalytics.Event_Today_ClickSearch);

            MainScene.Event.GetEvent<OpenSearchPageEvent>().Publish(null, BookwavesConstants.BackButtonPriority_SearchPage);
        }

        private void TrackEvent(string eventName)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(eventName);
        }
    }
}