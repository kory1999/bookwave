using System;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BeWild.AIBook.Runtime.Scene.Pages.BookContent.AudioPlayer;
using BeWild.Framework.Runtime.Utils.UI;
using BW.Framework.Utils;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using BeWild.AIBook.Runtime.Scene.Pages.BookContent.Content;
using BeWild.AIBook.Runtime.Scene.Pages.Home.PlayList;
using BeWild.Framework.Runtime.Utils;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent
{
    public class BookContentPageController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _pageCanvasGroup;
        [SerializeField] private BookFloatWindow _bookFloatWindow;
        [SerializeField] private DraggableImage _pageUIContainer;
        [SerializeField] private BookSoundPlayerPageUI _bookSoundPlayerPage;
        [SerializeField] private WordPanel _wordPanel;
        [SerializeField] private BookFinishPage _bookFinishPage;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private float _flipSizeDelta;

        private Tweener _containerMoveTweener;
        private float _beginDragPosition;
        private bool _isContainerOnScreen = false;
        private BookBriefData _bookBriefData;
        private BookContentData _bookContentData;
        private int _currentPageIndex;
        private Action _requiresOpenFromInside;
        private Action _closePageContent;

        public void Setup(Action closeCallback, Action requiresToOpenCallback)
        {
            _requiresOpenFromInside = requiresToOpenCallback;
            _closePageContent = closeCallback;

            Camera myCamera = Camera.main;

            _pageCanvasGroup.ToggleEnable(true);
            _pageCanvasGroup.ToggleInteract(false);

            _bookFloatWindow.Setup(_canvas.GetComponent<RectTransform>(), myCamera.GetLeftTopWorldPosition(),
                myCamera.GetRightTopWorldPosition(), HandleOnFloatWindowTap);

            _bookSoundPlayerPage.Setup(RequireToClose,
                () => { ToggleContainerContent(true); }, TurnPage, HandleOnTriggerPlayButton,
                HandleOnPlayButtonStatusChanged);

            _pageUIContainer.OnEndDragEvent += HandleOnContainerEndDrag;
            _pageUIContainer.OnBeginDragEvent += HandleOnContainerBeginDrag;
            _pageUIContainer.OnDragEvent += HandleOnDragging;

            _wordPanel.Initialize(null, HandleOnFinish, RequireToClose, TurnPage,
                () => { _bookSoundPlayerPage.TriggerPlayButton(true); },
                () => { ToggleContainerContent(false); });

            _bookFinishPage.Initialize(RequireToClose);

            GlobalEvent.GetEvent<LanguageUpdateEvent>().Subscribe(RefreshBookSoundPagePlayerUI);
        }

        private void HandleOnPlayButtonStatusChanged(bool isPlaying)
        {
            _wordPanel.SetAudioPlayButtonUI(isPlaying);
        }

        private void HandleOnTriggerPlayButton(Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus status)
        {
            if (status == Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus.Stopped ||
                status == Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus.Paused)
            {
                _wordPanel.SetAudioPlayButtonUI(true);
            }
            else
            {
                _wordPanel.SetAudioPlayButtonUI(false);
            }
        }

        public void RefreshBook(BookContentData bookContentData, BookBriefData bookBriefData, int initPage,
            int textPageStartCharacterIndex, double initTime = 0)
        {
            _bookBriefData = bookBriefData;
            _bookContentData = bookContentData;
            _currentPageIndex = initPage;
            BookSoundPlayerPageUI.BookPagesData bookPagesData =
                ConvertBookContentData(bookContentData, bookBriefData.name);
            _wordPanel.UpdateBook(bookContentData);
            _wordPanel.SetAudioPlayButtonUI(_bookSoundPlayerPage.IsPlaying());
            _bookFinishPage.Refresh(bookContentData.bookName, bookContentData.author, bookContentData.bigCoverUrl);
            _bookSoundPlayerPage.RefreshBook(bookPagesData, initPage, () =>
            {
                TurnPage(initPage, initTime);
                _bookFloatWindow.Refresh(bookContentData.bigCoverUrl);

                _wordPanel.SetStartCharacterIndex(textPageStartCharacterIndex);
            });
        }

        public void ToggleVIPLock(bool locked)
        {
            _wordPanel.ToggleVIPLock(locked);
            _bookSoundPlayerPage.ToggleVIPLock(locked);
        }

        private void TurnPage(int page, double initTime = 0)
        {
            if (GameManager.IsGameUnlocked || _bookBriefData.isFree || GameManager.IsFreeChapter(page))
            {
                DoTurnPageLogic();
            }
            else
            {
                GlobalEvent.GetEvent<OpenStoreWithCallbackEvent>().Publish(BookwavesAnalytics.Prefix_Book, () =>
                {
                    if (GameManager.IsGameUnlocked)
                    {
                        DoTurnPageLogic();
                    }
                });
            }

            void DoTurnPageLogic()
            {
                GameManager.RuntimeDataManager.CurrentSelectChapter = page;
                _currentPageIndex = page;

                EventData data = new EventData();
                data.BookID = _bookBriefData.id;
                data.ReadingIndex = page + 1;
                PlayListCenter.Instance.TriggerEvent(PlayCenterEvent.kIndexChaned, data);
                _bookSoundPlayerPage.TurnPage(page, initTime);
                _wordPanel.TurnPageByNumber(page);
                RecordCurrentProgress();
            }
        }

        public void ToggleInteract(bool on)
        {
            _pageCanvasGroup.ToggleInteract(on);

            if (on)
            {
                MobileKeyboardManager.Instance.AddBackListener(RequireToClose,
                    BookwavesConstants.BackButtonPriority_BookContent);
            }
            else
            {
                MobileKeyboardManager.Instance.RemoveBackListener(RequireToClose);
            }
        }

        private void ToggleContainerDrag(bool enable)
        {
            _pageUIContainer.AllowVerticalDrag = enable;
        }

        public void ToggleContainerVisual(bool enable, Action finishCallback = null)
        {
            _containerMoveTweener?.Kill();

            ToggleContainerDrag(false);

            if (enable)
            {
                _containerMoveTweener = _pageUIContainer.rectTransform
                    .DOAnchorPos(Vector2.zero, BookwavesConstants.UIMoveDuration)
                    .SetDelay(Time.deltaTime)
                    .SetEase(Ease.InSine).OnComplete(
                        () =>
                        {
                            _isContainerOnScreen = true;

                            ToggleContainerDrag(true);
                            _bookFloatWindow.Hide();

                            finishCallback?.Invoke();
                        });
            }
            else
            {
                _containerMoveTweener = _pageUIContainer.rectTransform.MoveOutFromScreenBottom(_canvas,
                    BookwavesConstants.UIMoveDuration,
                    Time.deltaTime,
                    Ease.InSine,
                    () =>
                    {
                        _isContainerOnScreen = false;

                        ToggleContainerDrag(true);
                        _bookFloatWindow.Show();

                        Vector2 anchoredPosition = _pageUIContainer.rectTransform.anchoredPosition;
                        anchoredPosition = new Vector2(anchoredPosition.x, anchoredPosition.y - 400f);
                        _pageUIContainer.rectTransform.anchoredPosition = anchoredPosition;

                        finishCallback?.Invoke();
                    });
            }
        }

        private void RequireToClose()
        {
            _closePageContent?.Invoke();
        }

        public void ToggleContainerContent(bool showText)
        {
            GameManager.RuntimeDataManager.IsTextPageShown = showText;
            _bookSoundPlayerPage.GetComponent<CanvasGroup>().ToggleEnable(!showText);
            _wordPanel.GetComponent<CanvasGroup>().ToggleEnable(showText);
        }

        private void HandleOnFinish()
        {
            GlobalEvent.GetEvent<GetAccountDataEvent>().Publish((accountData) =>
            {
                BookReadHistoryData historyData = new BookReadHistoryData();
                historyData.id = _bookContentData.id;
                historyData.progress = ((float) _currentPageIndex) / ((float) _bookContentData.pages.Length);
                historyData.chapter = _bookContentData.pages.Length - 1;
                historyData.currentSeconds = 0;
                historyData.fromTextPage = true;
                historyData.isFinished = true;
                int finishNumber = 0;
                foreach (BookReadHistoryData data in accountData.history)
                {
                    if (data.isFinished && data.id != _bookContentData.id)
                    {
                        finishNumber++;
                    }
                }

                GlobalEvent.GetEvent<RecordBookHistoryEvent>().Publish(historyData);

                _bookFinishPage.ShowFinishPage(finishNumber + 1);
            });
        }

        private void OnDestroy()
        {
            _containerMoveTweener?.Kill();
        }

        private void HandleOnFloatWindowTap(bool confirm)
        {
            _bookFloatWindow.Hide();
            RecordCurrentProgress();
            if (confirm)
            {
                _requiresOpenFromInside?.Invoke();
            }
            else
            {
                _bookSoundPlayerPage.Clear();
                _bookSoundPlayerPage.StopMusic();
            }
        }

        private void HandleOnContainerEndDrag(PointerEventData pointerEventData, DraggableImage draggableImage)
        {
            if (_pageUIContainer.rectTransform.anchoredPosition.y > 0)
            {
                _pageUIContainer.rectTransform.anchoredPosition = Vector2.zero;
            }

            if (_beginDragPosition - pointerEventData.position.y > _flipSizeDelta)
            {
                RequireToClose();
            }
            else
            {
                ToggleContainerVisual(_isContainerOnScreen);
            }
        }

        private void RecordCurrentProgress()
        {
            BookReadHistoryData bookReadHistoryData = new BookReadHistoryData();
            bookReadHistoryData.id = _bookContentData.id;
            bookReadHistoryData.progress = ((float) _currentPageIndex) / ((float) _bookContentData.pages.Length);
            bookReadHistoryData.isFinished = false;
            bookReadHistoryData.currentSeconds = _bookSoundPlayerPage.GetCurrentSeconds();
            bookReadHistoryData.fromTextPage = GameManager.RuntimeDataManager.IsTextPageShown;
            bookReadHistoryData.chapter = _currentPageIndex;
            GlobalEvent.GetEvent<RecordBookHistoryEvent>().Publish(bookReadHistoryData);
        }

        private void HandleOnContainerBeginDrag(PointerEventData pointerEventData, DraggableImage draggableImage)
        {
            _beginDragPosition = pointerEventData.position.y;
        }

        private void HandleOnDragging(PointerEventData pointerEventData, DraggableImage draggableImage)
        {
            if (_pageUIContainer.rectTransform.anchoredPosition.y > 0)
            {
                _pageUIContainer.rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        private BookSoundPlayerPageUI.BookPagesData ConvertBookContentData(BookContentData bookContentData,
            string bookName)
        {
            BookSoundPlayerPageUI.BookPagesData bookPagesData = new BookSoundPlayerPageUI.BookPagesData
            {
                BookCoverUrl = bookContentData.bigCoverUrl,
                PageDatas = new List<BookSoundPlayerPageUI.BookPageData>(),
                BookId = bookContentData.id,
                AuthorName = bookContentData.author,
                IsFree = _bookBriefData.isFree
            };

            string tmpString = string.Empty;
            for (int i = 0; i < bookContentData.pages.Length; i++)
            {
                BookSoundPlayerPageUI.BookPageData bookPageData = new BookSoundPlayerPageUI.BookPageData();
                GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("Wave [i] of [j]", s => tmpString = s);

                tmpString = tmpString.Replace("[i]", (i + 1).ToString());
                tmpString = tmpString.Replace("[j]", bookContentData.pages.Length.ToString());
                bookPageData.Title = tmpString;
                bookPageData.Content = bookName;
                bookPageData.AudioUrl = bookContentData.pages[i].audioUrl;
                bookPagesData.PageDatas.Add(bookPageData);
            }

            return bookPagesData;
        }

        private void RefreshBookSoundPagePlayerUI()
        {
            if (_bookContentData != null)
            {
                BookSoundPlayerPageUI.BookPagesData bookPagesData =
                    ConvertBookContentData(_bookContentData, _bookBriefData.name);

                _bookSoundPlayerPage.RefreshText(bookPagesData);
            }
        }

        private bool IsPageActive()
        {
            return _isContainerOnScreen || _bookFloatWindow.IsInScreen;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.ToggleAutoPlayNext(!hasFocus);
            if (hasFocus && IsPageActive())
            {
                int index = Framework.Runtime.Utils.AudioPlayer.AudioPlayer.GetCurrentClipIndex();
                if (index >= 0 && _currentPageIndex != index)
                {
                    float currentTime = Framework.Runtime.Utils.AudioPlayer.AudioPlayer.GetCurrentTime();
                    BaseLogger.Log(nameof(BookContentPageController),
                        $"application focus, book name: {_bookBriefData.name} clip index:{index}, current index:{_currentPageIndex},current time {currentTime}");
                    TurnPage(index, currentTime);
                }
            }
        }
    }
}