using System;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage.Logic;
using BeWild.Framework.Runtime.Analytics;
using BW.Framework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class HomePage : HomeView
    {
        [SerializeField] private Button searchButton;
        [SerializeField] private HomePageBanner banner;
        [SerializeField] private HomeFreeForToday freeForToday;
        [SerializeField] private HomeBookGroup recommend, newRelease;
        [SerializeField] private HomePageCategoryBase categoryGroup;
        [SerializeField] private HomePageCollection collection;
        [SerializeField] private HomePageShelf shelf;
        [SerializeField] private CanvasGroup normalGroup, errorGroup;
        [SerializeField] private Button refreshButton;

        private HomePageData _data;

        private ISearchPageLogic _currentSearchPageLogic;

        public override void Initialize()
        {
            searchButton.onClick.AddListener(HandleOnSearchButton);
            normalGroup.ToggleEnable(false);
            errorGroup.ToggleEnable(false);
            refreshButton.onClick.AddListener(HandleOnRefreshButton);
            
            banner.Initialize(HandleOnBannerBookTap);

            InitializeBookGroups(recommend, BookwavesAnalytics.Key_Home_Recommend);
            InitializeBookGroups(newRelease, BookwavesAnalytics.Key_Home_NewRelease);
            
            freeForToday.Initialize(HandleOnFreeBookButton);
            categoryGroup.Initialize(HandleOnCategoryGroupTap);
            
            collection.Initialize(HandleOnCollectionTap, null);
            shelf.Initialize(HandleOnShelfRefreshTap, HandleOnShelfBookTap);
            
            GlobalEvent.GetEvent<LanguageUpdateEvent>().Subscribe(HandleOnLanguageUpdate);
        }

        public override void ToggleVisual(bool on)
        {
            base.ToggleVisual(on);
            
            if (on)
            {
                if (_data == null)
                {
                    RefreshHomeData();
                }
            }
        }

        #region events

        private void HandleOnLanguageUpdate()
        {
            _data = null;   // clear data because we fetch backend data based on language type
        }

        private void RefreshHomeData()
        {
            GlobalEvent.GetEvent<GetHomePageDataEvent>().Publish(true, HandleOnHomeDataReceived);
        }

        private void HandleOnSearchButton()
        {
            TrackEvent(BookwavesAnalytics.Event_Home_ClickSearch);
            AdjustAnalytics.PublishEvent(AdjustAnalytics.ADEvent_4U_ClickSearch);
            OpenSearchPage(null);
        }

        private void HandleOnRefreshButton()
        {
            TrackEvent(BookwavesAnalytics.Event_Network_ClickRefresh);
            
            errorGroup.ToggleEnable(false);
            
            RefreshHomeData();
        }

        private void HandleOnHomeDataReceived(HomePageData data)
        {
            bool isValidData = IsValidData(data);
            bool showLayout = isValidData || _data != null;
            
            errorGroup.ToggleEnable(!showLayout);
            normalGroup.ToggleEnable(showLayout);
            
            if (showLayout)
            {
                if (isValidData)
                {
                    _data = data;
                    UpdateFullVisual();
                }
            }
            else
            {
                TrackEvent(BookwavesAnalytics.Event_Network_ShowError);
            }
        }

        private bool IsValidData(HomePageData data)
        {
            return data != null && data.booksList != null && data.booksList.Count > 0;
        }

        private void HandleOnBannerBookTap(int bookId)
        {
            HandleOnBookTap(bookId);
        }

        private void HandleOnCollectionTap(int collectionId)
        {
            TrackEvent(BookwavesAnalytics.Prefix_Home_ClickCollection + collectionId);

            CollectionData data = _data.collection.Find(c => c.Id == collectionId);

            if (data == null)
            {
                return;
            }

            CollectionLogic logic = new CollectionLogic();
            logic.Initialize(new BookListData(), data);
            
            OpenSearchPage(logic);
        }

        private void HandleOnShelfRefreshTap()
        {
            TrackEvent(BookwavesAnalytics.Event_Home_ClickShelfSwitch);
            
            UpdateShelf();
        }

        private void HandleOnShelfBookTap(int bookId)
        {
            TrackEvent(BookwavesAnalytics.Event_Home_ClickShelfBook);

            HandleOnBookTap(bookId);
        }

        private void HandleOnFreeBookButton()
        {
            TrackEvent(BookwavesAnalytics.Event_Home_ClickFreeForToday);
            
            HandleOnBookTap(_data.booksList[0].books[0].id);
        }

        private void HandleOnMoreButtonTap(int id, string trackingName)
        {
            BookListData data = _data.booksList.Find(b => b.bannerId == id);
            TrackEvent(BookwavesAnalytics.Prefix_Home_ClickMore + trackingName + BookwavesAnalytics.Suffix_Home_ClickMore);
            if (String.Equals(trackingName, BookwavesAnalytics.Key_Home_Recommend))
            {
                AdjustAnalytics.PublishEvent(AdjustAnalytics.ADEvent_4U_ClickRecommendedMore);    
            }
            else if (String.Equals(trackingName, BookwavesAnalytics.Key_Home_NewRelease))
            {
                AdjustAnalytics.PublishEvent(AdjustAnalytics.ADEvent_4U_ClickNewReleaseMore);
            }
            
            BookGroupLogic logic = new BookGroupLogic();
            logic.Initialize(data);
            
            OpenSearchPage(logic);
        }

        private void HandleOnBookTap(int id)
        {
            MainScene.Event.GetEvent<OpenBookEvent>().Publish(id);
        }

        private void HandleOnCategoryGroupTap(int id)
        {
            SwitchToCategoryBooksPage(id, true);
        }

        #endregion

        #region page state

        private void SwitchToCategoryBooksPage(int id, bool fromCategoryGroup)
        {
            CategoryData categoryData = _data.FindCategory(id);

            if (categoryData == null)
            {
                return;
            }
                
            TrackEvent((fromCategoryGroup ? 
                BookwavesAnalytics.Prefix_Home_ClickCategoryGroup : 
                BookwavesAnalytics.Prefix_Home_ClickCategoryPage) + categoryData.name);
            
            CategoryLogic logic = new CategoryLogic();
            logic.Initialize(new BookListData(), id);
            
            OpenSearchPage(logic);
        }

        #endregion

        #region visual

        private void InitializeBookGroups(HomeBookGroup group, string trackingName)
        {
            group.Initialize(id =>
            {
                HandleOnMoreButtonTap(id, trackingName);
            }, bookId =>
            {
                TrackEvent(BookwavesAnalytics.Event_Home_ClickBookGroupBook);

                HandleOnBookTap(bookId);
            });
        }

        private void UpdateFullVisual()
        {
            if (_data != null)
            {
                GlobalEvent.GetEvent<GetLanguageEvent>().Publish(language =>
                {
                    if (language == AppLanguage.Spanish)
                    {
                        TrackEvent(BookwavesAnalytics.Event_Home_ShowSpanish);
                    }
                });

                UpdateBanner();

                UpdateBookGroups();

                UpdateCategoryGroup();

                UpdateCollection();
                
                UpdateShelf();
            }
        }

        private void UpdateBanner()
        {
            banner.SetData(_data.banner);
        }

        private void UpdateBookGroups()
        {
            bool showFree = false;
            bool showRecommend = false;
            bool showNewRelease = false;
            if (_data.booksList != null)
            {
                showFree = _data.booksList.Count > 0 && _data.booksList[0].books.Count > 0;
                showRecommend = _data.booksList.Count > 1;
                showNewRelease = _data.booksList.Count > 2;
            }

            freeForToday.gameObject.SetActive(showFree);
            if (showFree)
            {
                BookBriefData book = _data.booksList[0].books[0];
                freeForToday.SetData(book.name, book.author, book.icon);
            }

            recommend.gameObject.SetActive(showRecommend);
            if (showRecommend)
            {
                recommend.SetData(_data.booksList[1]);
            }
            
            newRelease.gameObject.SetActive(showNewRelease);
            if (showNewRelease)
            {
                newRelease.SetData(_data.booksList[2]);
            }
        }

        private void UpdateCategoryGroup()
        {
            categoryGroup.ShowCategory(_data.GetCategoryList(GameManager.Language));
        }

        private void UpdateCollection()
        {
            collection.SetData(_data.collection);
        }

        private void UpdateShelf()
        {
            if (!shelf.IsRefreshing())
            {
                shelf.ToggleRefreshState(true);
                
                GlobalEvent.GetEvent<GetHomeShelfDataEvent>().Publish(data =>
                {
                    if (data != null)
                    {
                        shelf.SetData(data.books, () => shelf.ToggleRefreshState(false));
                    }
                    else
                    {
                        shelf.ToggleRefreshState(false);
                    }
                });
            }
        }

        #endregion

        private void OpenSearchPage(ISearchPageLogic logic)
        {
            MainScene.Event.GetEvent<OpenSearchPageEvent>().Publish(logic, BookwavesConstants.BackButtonPriority_SearchPage);
        }

        private void TrackEvent(string eventName)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(eventName);
        }
    }
}