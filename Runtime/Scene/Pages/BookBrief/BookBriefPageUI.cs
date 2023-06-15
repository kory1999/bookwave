using System;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Scene.Pages.BookContent.Content;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage;
using BeWild.Framework.Runtime.Analytics;
using BeWild.Framework.Runtime.Utils;
using BeWild.Framework.Runtime.Utils.UI;
using BW.Framework.Utils;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookBrief
{
    public class BookBriefPageUI : MonoBehaviour
    {
        public class BookBriefPageData
        {
            public int BookID;
            public string BookName;
            public string AuthorName;
            public string BookCoverUrl;
            public int TotalMinutes;
            public List<string> ChapterNames;
            public List<CategoryData> CategoryDatas;
            public string BookDetails;
        }

        [SerializeField] private RectTransform visual, visualInPosition, visualOutPosition;
        [SerializeField] private RawImageHolder _bookCover;
        [SerializeField] private Button _backButton;
        [SerializeField] private SelectButton _downloadButton;
        [SerializeField] private Button _shareButton;
        [SerializeField] private SelectButton _collectButton;
        [SerializeField] private Button _readButton;
        [SerializeField] private Button _listenButton;
        [SerializeField] private TextMeshProUGUI _bookNameText;
        [SerializeField] private TextMeshProUGUI _authorText;
        [SerializeField] private TextMeshProUGUI _totalMinutesText;
        [SerializeField] private TextMeshProUGUI _totalWavesText;
        [SerializeField] private TextMeshProUGUI _bookDetailsText;
        [SerializeField] private BookBriefSelection _selectionPrefab;
        [SerializeField] private Transform _selectionParent;
        [SerializeField] private HomePageCategory _categoryPrefab;
        [SerializeField] private Transform _categoryLayoutGroup;
        [SerializeField] private SatisfactionButton _satisfactionButton;
        [SerializeField] private ContentSizeFitter _contentSizeFitter;
        [SerializeField] private ScrollRect _contentScrollRect;

        private CanvasGroup _myCanvasGroup;
        private Action _readButtonTapCallback;
        private Action _listenButtonTapCallback;
        private Action<int> _categoryTapCallback;
        private Action<int> _chapterTapCallback;
        private BookBriefPageData _pageData;
        private Action _backButtonTapCallback;

        private bool _isLocked=true;

        public bool IsDataValid(int bookId)
        {
            if (_pageData != null)
            {
                return _pageData.BookID == bookId;
            }

            return false;
        }

        public void Initialize(Action closeCallback, Action readButtonTapCallback, Action listenButtonTapCallback,
            Action<int> categoryTapCallback, Action<int> chapterTapCallback)
        {
            _backButtonTapCallback = closeCallback;
            _chapterTapCallback = chapterTapCallback;
            _categoryTapCallback = categoryTapCallback;
            _readButtonTapCallback = readButtonTapCallback;
            _listenButtonTapCallback = listenButtonTapCallback;
            _satisfactionButton.SetUp(null);
            _backButton.onClick.AddListener(HandleOnBackButtonTap);
            _readButton.onClick.AddListener(HandleOnReadButtonTap);
            _listenButton.onClick.AddListener(HandleOnListenButtonTap);
            _collectButton.Setup(HandleOnButtonCollect);
            _shareButton.onClick.AddListener(HandleOnShareButtonTap);
            _downloadButton.Setup(HandleOnDownloadButtonTap);

            visual.localPosition = visualOutPosition.localPosition;
            _myCanvasGroup = GetComponent<CanvasGroup>();
            _myCanvasGroup.ToggleEnable(true);
            _myCanvasGroup.ToggleInteract(false);
        }

        public void RefreshByBookBriefData(BookBriefData data)
        {
            _bookCover.SetTexture(data.icon, true);
            _authorText.text = data.author;
            _bookNameText.text = data.name;
        }

        public void Refresh(BookBriefPageData pageData)
        {
            _pageData = pageData;

            _bookCover.SetTexture(_pageData.BookCoverUrl, true);
            GlobalEvent.GetEvent<GetAccountDataEvent>().Publish((data =>
            {
                _collectButton.SetVisual(data.bookMarks.Contains(_pageData.BookID));
            }));

            _bookNameText.text = _pageData.BookName;
            _authorText.text = _pageData.AuthorName;
            GlobalEvent.GetEvent<GetLocalizationEvent>()
                .Publish("min", s => _totalMinutesText.text = $"{_pageData.TotalMinutes} {s}");
            GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("waves",
                s => _totalWavesText.text = $"{_pageData.ChapterNames.Count} {s}");
            // _totalMinutesText.text = $"{_pageData.TotalMinutes} min";
            // _totalWavesText.text = $"{_pageData.ChapterNames.Count} waves";
            _bookDetailsText.text = _pageData.BookDetails;
            RefreshChapters(pageData.ChapterNames);
            _satisfactionButton.Refresh();
            RefreshCategories(pageData.CategoryDatas);
            DelayInvoker.Instance.Invoke(this, () =>
                {
                    _contentSizeFitter?.Refresh(this, true, null);
                    _listenButton.GetComponentInChildren<ContentSizeFitter>().Refresh(this, true, null,false);
                    _readButton.GetComponentInChildren<ContentSizeFitter>().Refresh(this, true, null,false);
                },
                Time.deltaTime);
        }

        public void ToggleUI(bool on, Action callback)
        {
            visual.DOKill();
            Vector2 target = on ? visualInPosition.localPosition : visualOutPosition.localPosition;
            visual.DOLocalMove(target, BookwavesConstants.UIMoveDuration).onComplete += () => { callback?.Invoke(); };
        }

        public void ToggleInteract(bool on)
        {
            _myCanvasGroup.ToggleInteract(on);

            if (on)
            {
                MobileKeyboardManager.Instance.AddBackListener(HandleOnBackButtonTap,
                    BookwavesConstants.BackButtonPriority_BookBrief);
            }
            else
            {
                MobileKeyboardManager.Instance.RemoveBackListener(HandleOnBackButtonTap);
            }
        }

        public void SetDownloadState(bool on)
        {
            if (_downloadButton)
            {
                _downloadButton.ToggleOnButton(!on);
                _downloadButton.SetVisual(on);   
            }
            else
            {
                Debug.unityLogger.LogError("BookBriefPageUI", "DownloadButton is null");
            }
        }

        public void ToggleVIPLock(bool locked)
        {
            _isLocked = locked;

            for (int i = 0; i < _selectionParent.childCount; i++)
            {
                _selectionParent.GetChild(i).GetComponent<BookBriefSelection>().ToggleVIPLock(locked);
            }
        }

        public void RefreshContentToBeginning()
        {
            _contentScrollRect.StopMovement();
            Vector3 p = _contentScrollRect.content.transform.localPosition;
            p.y = 0;
            _contentScrollRect.content.transform.localPosition = p;
        }

        private void RefreshCategories(List<CategoryData> categoryDatas)
        {
            for (int i = _categoryLayoutGroup.childCount - 1; i >= 0; i--)
            {
                Destroy(_categoryLayoutGroup.GetChild(i).gameObject);
            }

            for (int i = 0; i < categoryDatas.Count; i++)
            {
                bool refreshFinish = false;
                HomePageCategory newCategory = Instantiate(_categoryPrefab.gameObject,_categoryLayoutGroup).GetComponent<HomePageCategory>();
                newCategory.transform.localScale = Vector3.one;
                newCategory.Initialize(categoryDatas[i], _categoryTapCallback);
            }

            if (_categoryLayoutGroup.childCount > 0)
            {
                _categoryLayoutGroup.GetChild(0).GetComponent<ContentSizeFitter>().Refresh(this, false, null);
            }
        }

        private void HandleOnButtonCollect()
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_BookBrief_ClickMark);

            if (_pageData == null)
                return;
            
            GlobalEvent.GetEvent<GetAccountDataEvent>().Publish((data =>
            {
                if (data.bookMarks.Contains(_pageData.BookID))
                {
                    GlobalEvent.GetEvent<RemoveMarkEvent>().Publish(_pageData.BookID);
                    _collectButton.SetVisual(false);
                }
                else
                {
                    GlobalEvent.GetEvent<MarkBookEvent>().Publish(_pageData.BookID);
                    _collectButton.SetVisual(true);
                }
            }));
        }

        private void RefreshChapters(List<string> chapterNames)
        {
            for (int i = _selectionParent.childCount - 1; i >= 0; i--)
            {
                Destroy(_selectionParent.GetChild(i).gameObject);
            }

            for (int i = 0; i < chapterNames.Count; i++)
            {
                Instantiate(_selectionPrefab.gameObject, _selectionParent).GetComponent<BookBriefSelection>().Setup(i,
                    chapterNames[i], true, HandleOnChapterTap, _isLocked);
            }
        }

        private void HandleOnChapterTap(int chapterIndex)
        {
            _chapterTapCallback?.Invoke(chapterIndex);
        }

        private void HandleOnBackButtonTap()
        {
            _backButtonTapCallback?.Invoke();
        }

        private void HandleOnReadButtonTap()
        {
            _readButtonTapCallback?.Invoke();
        }

        private void HandleOnListenButtonTap()
        {
            _listenButtonTapCallback?.Invoke();
        }

        private void HandleOnShareButtonTap()
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_BookBrief_ClickShare);
            AdjustAnalytics.PublishEvent(AdjustAnalytics.ADEvent_BookBrief_ClickShare);

            if (_pageData != null)
            {
                BookwavesNativeUtility.ShareBook(_pageData.BookName, _pageData.AuthorName);    
            }
        }

        private void HandleOnDownloadButtonTap()
        {
            if (_pageData == null)
                return;
            
            if (!_isLocked)
            {
                _downloadButton.ToggleOnButton(false);
                GlobalEvent.GetEvent<GetAccountDataEvent>().Publish((accountData) =>
                {
                    BookReadHistoryData historyData = new BookReadHistoryData();
                    historyData.id = _pageData.BookID;
                    historyData.progress = 0;
                    historyData.chapter = 1;
                    historyData.currentSeconds = 0;
                    historyData.fromTextPage = false;
                    historyData.isFinished = false;
                    GlobalEvent.GetEvent<RecordBookHistoryEvent>().Publish(historyData);
                });

                GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("Start download",
                    text => { GlobalEvent.GetEvent<ShowToastEvent>().Publish(text, 0.2f); });

                GlobalEvent.GetEvent<DownloadBookSoundEvent>().Publish(_pageData.BookID, SetDownloadStateAndToast);
            }
            else
            {
                GlobalEvent.GetEvent<OpenStoreEvent>().Publish(BookwavesAnalytics.Prefix_DownloadButton);
            }
            
        }

        private void SetDownloadStateAndToast(bool on)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_BookBrief_ClickDownload);

            GlobalEvent.GetEvent<GetLocalizationEvent>().Publish(on ? "Download successful" : "Download failed",
                text => { GlobalEvent.GetEvent<ShowToastEvent>().Publish(text, 0.2f); });
            SetDownloadState(on);
        }

       
    }
}