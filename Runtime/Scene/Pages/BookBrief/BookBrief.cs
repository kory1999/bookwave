using System;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage.Logic;
using BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage;
using BeWild.Framework.Runtime.Analytics;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookBrief
{
    public class BookBrief : PageBase
    {
        [SerializeField] private BookBriefPageUI _pageUI;

        private BookBriefData _bookBriefData;
        private bool _shouldRefresh;
        private bool _isGameUnlocked;
        private bool _scrollToBeginning;

        protected override void Initialize()
        {
            MainScene.Event.GetEvent<OpenBookBriefEvent>().Subscribe(HandleOnDataUpdate);
            _pageUI.Initialize(HandleOnCloseButton, HandleOnReadButton, HandleOnListenButton, HandleOnCategoryButton,
                HandleOnChapterButton);
        }

        protected override void DoToggleVisual(bool on, Action finishCallback)
        {
            _pageUI.ToggleUI(on, finishCallback);

            UpdateUIWithData();
        }

        protected override void DoToggleInteract(bool on)
        {
            _pageUI.ToggleInteract(on);

            if (on)
            {
                UpdateUIWithData();
            }
        }

        public override void ToggleVIPState(bool unlock)
        {
            base.ToggleVIPState(unlock);

            _isGameUnlocked = unlock;

            _pageUI.ToggleVIPLock(!unlock && !_bookBriefData.isFree);
        }

        public override void RefreshPage(bool isGameUnlocked)
        {
            base.RefreshPage(isGameUnlocked);

            // make sure search page is under book brief.
            MainScene.Event.GetEvent<SetSearchPageSortingOrderEvent>()
                .Publish(BookwavesConstants.BackButtonPriority_SearchPage);

            UpdateUIWithData();

            _isGameUnlocked = isGameUnlocked;

            ToggleVIPState(isGameUnlocked);
        }

        private void UpdateUIWithData()
        {
            if (_shouldRefresh)
            {
                _shouldRefresh = false;

                TryShowRateUs();

                TrackEvent(BookwavesAnalytics.Event_BookBrief_Show);
                AdjustAnalytics.PublishEvent(AdjustAnalytics.ADEvent_BookBrief_Show);

                BookBriefData bookBriefData = _bookBriefData;
                _pageUI.RefreshByBookBriefData(bookBriefData);
                
                ConvertBookBriefDataToPageData(bookBriefData, data =>
                {
                    _pageUI.Refresh(data);

                    if (_scrollToBeginning)
                    {
                        _scrollToBeginning = false;
                        _pageUI.RefreshContentToBeginning();
                    }
                });
            }
            
            //set download state
            //不放在Refresh中是因为Refresh的判断并没有判断是是否下载完成
            GlobalEvent.GetEvent<GetBookSoundCacheEvent>()
                .Publish(_bookBriefData.id, list => { _pageUI.SetDownloadState(list != null); });
            
        }

        private void TryShowRateUs()
        {
            int count = PlayerPrefs.GetInt(PlayerPrefsHelper.Key_EnterBookBriefCount, 0) + 1;

            if (count == 3)
            {
                BookwavesNativeUtility.TryWeeklyRateUs();
            }

            PlayerPrefs.SetInt(PlayerPrefsHelper.Key_EnterBookBriefCount, count);
        }

        private void ConvertBookBriefDataToPageData(BookBriefData bookBriefData,
            Action<BookBriefPageUI.BookBriefPageData> bookBriefPageDataCallback)
        {
            GlobalEvent.GetEvent<GetHomePageDataEvent>().Publish(false, homePageData =>
            {
                if (homePageData == null)
                {
                    return; // in case of off line.
                }

                GlobalEvent.GetEvent<GetBookContentEvent>().Publish(bookBriefData.id,
                    (data) =>
                    {
                        if (data == null)
                        {
                            return; // in case of off line.
                        }

                        BookBriefPageUI.BookBriefPageData pageData = new BookBriefPageUI.BookBriefPageData();
                        pageData.BookID = data.id;
                        pageData.AuthorName = bookBriefData.author;
                        pageData.BookName = bookBriefData.name;
                        pageData.TotalMinutes = (int)(data.totalLength / 60f);
                        pageData.BookCoverUrl = bookBriefData.icon;
                        pageData.BookDetails = data.introduction;
                        pageData.CategoryDatas = new List<CategoryData>();
                        foreach (var c in data.categories)
                        {
                            CategoryData cData = homePageData.FindCategory(c);
                            if (cData != null)
                            {
                                pageData.CategoryDatas.Add(cData);
                            }
                        }

                        List<string> chapterNames = new List<string>();
                        for (int i = 0; i < data.pages.Length; i++)
                        {
                            chapterNames.Add(data.pages[i].heading);
                        }

                        pageData.ChapterNames = chapterNames;
                        bookBriefPageDataCallback?.Invoke(pageData);
                    });
            });
        }

        private void HandleOnCloseButton()
        {
            TrackEvent(BookwavesAnalytics.Event_BookBrief_Close);

            DoClose();
        }

        private void HandleOnReadButton()
        {
            TrackEvent(BookwavesAnalytics.Event_BookBrief_ClickRead);

            if (_pageUI.IsDataValid(_bookBriefData.id))
            {
                MainScene.Event.GetEvent<OpenBookContentEvent>().Publish(_bookBriefData, 0, true, 0);    
            }
            else
            {
                Debug.unityLogger.LogError( "BookBriefPage", "HandleOnReadButton: data is not valid.");
            }
        }

        private void HandleOnListenButton()
        {
            TrackEvent(BookwavesAnalytics.Event_BookBrief_ClickListen);

            if (_pageUI.IsDataValid(_bookBriefData.id))
            {
                MainScene.Event.GetEvent<OpenBookContentEvent>().Publish(_bookBriefData, 0, false, 0);    
            }
            else
            {
                Debug.unityLogger.LogError( "BookBriefPage", "HandleOnListenButton: data is not valid.");
            }
        }

        private void HandleOnCategoryButton(int id)
        {
            TrackEvent(BookwavesAnalytics.Prefix_BookBrief_ClickCategory + id);

            Dictionary<string, object> p = new Dictionary<string, object>(1);
            p.Add("id", id);
            AdjustAnalytics.PublishEvent(AdjustAnalytics.ADEvent_BookDetail_ClickTagNO, p);

            GlobalEvent.GetEvent<GetHomePageDataEvent>().Publish(false, homePageData =>
            {
                if (homePageData == null)
                {
                    return;
                }

                CategoryLogic logic = new CategoryLogic();
                logic.Initialize(new BookListData(), id);
                MainScene.Event.GetEvent<OpenSearchPageEvent>()
                    .Publish(logic, BookwavesConstants.BackButtonPriority_BookBrief + 1);
            });
        }

        private void HandleOnChapterButton(int id)
        {
            TrackEvent(BookwavesAnalytics.Event_BookBrief_ClickChapter);

            if (_pageUI.IsDataValid(_bookBriefData.id))
            {
                MainScene.Event.GetEvent<OpenBookContentEvent>().Publish(_bookBriefData, id, true, 0);    
            }
            else
            {
                Debug.unityLogger.LogError( "BookBriefPage", "HandleOnChapterButton: data is not valid.");
            }
        }

        private void HandleOnDataUpdate(BookBriefData bookBriefData)
        {
            if (bookBriefData != null)
            {
                if ((_bookBriefData != bookBriefData) || (_bookBriefData.id != bookBriefData.id))
                {
                    _scrollToBeginning = true;

                    _bookBriefData = bookBriefData;
                    _shouldRefresh = true;
                }
                
              
            }
        }

        private void TrackEvent(string eventName)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(eventName);
        }
    }
}