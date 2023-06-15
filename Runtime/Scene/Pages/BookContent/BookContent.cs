using System;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BeWild.AIBook.Runtime.Scene.Pages.Home.PlayList;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent
{
    public class BookContent : PageBase
    {
        [SerializeField] private BookContentPageController _bookContentPageController;

        protected override void Initialize()
        {
            _bookContentPageController.Setup(DoClose, RequiresToOpenContentFromInside);
        }

        public override void RefreshPage(bool isGameUnlocked)
        {
            base.RefreshPage(isGameUnlocked);
            // ShowPage();

            GlobalEvent.GetEvent<GetBookContentEvent>().Publish(GameManager.RuntimeDataManager.BookBriefData.id,
                bookContentData =>
                {
                    GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Prefix_BookContent_Show +
                                                                  GameManager.RuntimeDataManager.BookBriefData.id);
                    bool showTextPage = GameManager.RuntimeDataManager.IsTextPageShown;
                    int textPageStartIndex = GameManager.RuntimeDataManager.TextPageStartCharacterIndex;
                    int initPage = GameManager.RuntimeDataManager.CurrentSelectChapter;
                    double initSeconds = GameManager.RuntimeDataManager.JumpToCurrentSeconds;
                    _bookContentPageController.ToggleContainerContent(showTextPage);
                    _bookContentPageController.RefreshBook(bookContentData,
                        GameManager.RuntimeDataManager.BookBriefData, initPage, textPageStartIndex, initSeconds);

                    bool soundHaveCache = false;
                    GlobalEvent.GetEvent<GetBookSoundCacheEvent>().Publish(bookContentData.id,
                        urls => { soundHaveCache = urls != null; });

                    PlayItem item = new PlayItem(bookContentData.id, bookContentData.pages.Length, initPage + 1,
                        bookContentData.bookName, soundHaveCache);
                    EventData data = new EventData();
                    data.Items = new List<PlayItem>(1);
                    data.Items.Add(item);
                    data.BookID = bookContentData.id;
                    data.ReadingIndex = initPage;
                    data.State = showTextPage ? PlayItemState.kReading : PlayItemState.kListening;

                    PlayListCenter.Instance.TriggerEvent(PlayCenterEvent.kAddItem, data);
                    PlayListCenter.Instance.TriggerEvent(PlayCenterEvent.kChangeItemState, data);


                    GameManager.RuntimeDataManager.TextPageStartCharacterIndex = 0;
                });

            ToggleVIPState(isGameUnlocked);
        }

        protected override void DoToggleVisual(bool on, Action finishCallback)
        {
            _bookContentPageController.ToggleContainerVisual(on, finishCallback);

            if (on)
            {
                ShowPage();
            }
        }

        protected override void DoToggleInteract(bool on)
        {
            _bookContentPageController.ToggleInteract(on);
        }

        public override void ToggleVIPState(bool unlock)
        {
            base.ToggleVIPState(unlock);

            _bookContentPageController.ToggleVIPLock(!unlock && !GameManager.RuntimeDataManager.BookBriefData.isFree);
        }

        private void ShowPage()
        {
            GlobalEvent.GetEvent<GetBookContentEvent>().Publish(GameManager.RuntimeDataManager.BookBriefData.id,
                bookContentData =>
                {
                    GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Prefix_BookContent_Show +
                                                                  GameManager.RuntimeDataManager.BookBriefData.id);
                    //判断是显示SoundPage还是ReadPage
                    bool showTextPage = GameManager.RuntimeDataManager.IsTextPageShown;
                    int textPageStartIndex = GameManager.RuntimeDataManager.TextPageStartCharacterIndex;
                    int initPage = GameManager.RuntimeDataManager.CurrentSelectChapter;
                    double initSeconds = GameManager.RuntimeDataManager.JumpToCurrentSeconds;
                    _bookContentPageController.ToggleContainerContent(showTextPage);
                    _bookContentPageController.RefreshBook(bookContentData,
                        GameManager.RuntimeDataManager.BookBriefData, initPage, textPageStartIndex, initSeconds);


                    bool soundHaveCache = false;
                    GlobalEvent.GetEvent<GetBookSoundCacheEvent>().Publish(bookContentData.id,
                        urls => { soundHaveCache = urls != null; });

                    PlayItem item = new PlayItem(bookContentData.id, bookContentData.pages.Length, initPage + 1,
                        bookContentData.bookName, soundHaveCache);
                    EventData data = new EventData();
                    data.Items = new List<PlayItem>(1);
                    data.Items.Add(item);
                    data.BookID = bookContentData.id;
                    data.ReadingIndex = initPage;
                    data.State = showTextPage ? PlayItemState.kReading : PlayItemState.kListening;

                    PlayListCenter.Instance.TriggerEvent(PlayCenterEvent.kAddItem, data);
                    PlayListCenter.Instance.TriggerEvent(PlayCenterEvent.kChangeItemState, data);

                    GameManager.RuntimeDataManager.TextPageStartCharacterIndex = 0;
                });
        }

        private void RequiresToOpenContentFromInside()
        {
            MainScene.Event.GetEvent<OpenBookContentEvent>().Publish(GameManager.RuntimeDataManager.BookBriefData,
                GameManager.RuntimeDataManager.CurrentSelectChapter, GameManager.RuntimeDataManager.IsTextPageShown, 0);
        }
    }
}