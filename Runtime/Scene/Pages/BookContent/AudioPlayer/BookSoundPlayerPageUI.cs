using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using BeWild.AIBook.Runtime.Scene.Pages.Home.PlayList;
using BeWild.Framework.Runtime.Utils.AudioPlayer;
using BW.Framework.Utils;
using RenderHeads.Media.AVProVideo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.AudioPlayer
{
    public class BookSoundPlayerPageUI : MonoBehaviour
    {
        public class BookPagesData
        {
            public int BookId;
            public string BookCoverUrl;
            public string AuthorName;
            public bool IsFree;
            public List<BookPageData> PageDatas;
        }

        public class BookPageData
        {
            public string Title;
            public string Content;
            public string AudioUrl;
        }

        [SerializeField] private RawImageHolder _bookCover;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _content;
        [SerializeField] private SoundSlider _soundSlider;
        [SerializeField] private Button _rollBackButton;
        [SerializeField] private Button _goForwardButton;
        [SerializeField] private Button _toPreviousButton;
        [SerializeField] private Button _toNextButton;
        [SerializeField] private Button _chapterSelectButton;
        [SerializeField] private Button _playlistButton;
        [SerializeField] private Button _hideButton;
        [SerializeField] private Button _changeToTextPageButton;
        [SerializeField] private VariableButton _playButton;
        [SerializeField] private SelectionText _speedSelectionText;
        [SerializeField] private SelectPanel _chaperSelectPanel;

        private BookSoundPlayer _bookSoundPlayer;
        private BookPagesData _pagesData;
        private int _currentPageIndex = -1;
        private Action<int, double> _onTurnPageCallback;
        private Action<Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus> _triggerPlayButtonCallback;
        private Action<bool> _onBookSoundPlayerStatusChangedCallback;
        private bool _vipLock;
        private bool _allowToast = true;

        public void Setup(Action hideCallback, Action changeToTextPageCallback, Action<int, double> onTurnPageCallback,
            Action<Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus> triggerPlayButtonCallback,
            Action<bool> onBookSoundPlayerStatusChangedCallback)
        {
            _onBookSoundPlayerStatusChangedCallback = onBookSoundPlayerStatusChangedCallback;
            _triggerPlayButtonCallback = triggerPlayButtonCallback;
            _bookSoundPlayer = new BookSoundPlayer();
            _bookSoundPlayer.Initialize();
            _bookSoundPlayer.OnFrameUpdateEvent += HandleOnProgressUpdate;
            _onTurnPageCallback = onTurnPageCallback;

            _hideButton.onClick.AddListener(() =>
            {
                TrackEvent(BookwavesAnalytics.Event_BookContent_ClickListenClose);
                _chaperSelectPanel.ToggleEnable(false);
                hideCallback?.Invoke();
            });

            _changeToTextPageButton.onClick.AddListener(() =>
            {
                TrackEvent(BookwavesAnalytics.Event_BookContent_ClickListenRead);
                changeToTextPageCallback?.Invoke();
            });

            _rollBackButton.onClick.AddListener(() =>
            {
                TrackEvent(BookwavesAnalytics.Event_BookContent_ClickListenRewind);
                if (_bookSoundPlayer.IsPlaying())
                {
                    _bookSoundPlayer.SetTime(_bookSoundPlayer.CurrentSeconds - 5);
                }
            });

            _goForwardButton.onClick.AddListener(() =>
            {
                TrackEvent(BookwavesAnalytics.Event_BookContent_ClickListenFastForward);
                if (_bookSoundPlayer.IsPlaying())
                {
                    _bookSoundPlayer.SetTime(_bookSoundPlayer.CurrentSeconds + 10);
                }
            });

            _toPreviousButton.onClick.AddListener(() =>
            {
                TrackEvent(BookwavesAnalytics.Event_BookContent_ClickListenPrevious);

                if (_currentPageIndex > 0)
                {
                    _onTurnPageCallback?.Invoke(_currentPageIndex - 1, 0);
                }
                else
                {
                    ShowToast("This is the first wave");
                }
            });

            _toNextButton.onClick.AddListener(() =>
            {
                TrackEvent(BookwavesAnalytics.Event_BookContent_ClickListenNext);
                
                if (_currentPageIndex < _pagesData.PageDatas.Count - 1)
                {
                    _onTurnPageCallback?.Invoke(_currentPageIndex + 1, 0);
                }
                else
                {
                    ShowToast("This is the final wave");
                }
            });

            _chapterSelectButton.onClick.AddListener(() =>
            {
                _chaperSelectPanel.ToggleEnable(!_chaperSelectPanel.Enabled);
            });

            _playlistButton.onClick.AddListener(() =>
            {
                TrackEvent(BookwavesAnalytics.Event_BookContent_ClickPlayList);
                
                PlayListCenter.Instance.TriggerEvent(PlayCenterEvent.kOpenPage, null);
            });

            _speedSelectionText.Setup(HandleOnSpeedSelectTextTap);

            _playButton.Setup("Play", HandleOnPlayButtonTap);

            _bookSoundPlayer.OnAudioPlayerEvent += HandleOnAudioPlayerTriggerEvent;

            _bookSoundPlayer.OnStatusChangedEvent += HandleOnBookSoundPlayerStatusChanged;

            _chaperSelectPanel.Setup(HandleOnSelectionButtonTap);

            _soundSlider.Setup();

            _soundSlider.OnSliderEndDragEvent += HandleOnSliderEndDrag;

            _chaperSelectPanel.ToggleEnable(false);
        }

        bool CheckData()
        {
            if (_pagesData == null)
            {
                ShowToast("Data not ready!");
                return false;
            }

            return true;
        }

        public bool IsPlaying()
        {
            return _bookSoundPlayer.CurrentStatus ==
                   Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus.Playing;
        }

        public void RefreshBook(BookPagesData pagesData, int pageIndex, Action callback)
        {
            _chaperSelectPanel.ToggleEnable(false);
            if (_pagesData == null || (pagesData.BookId != _pagesData.BookId))
            {
                _pagesData = pagesData;
                _currentPageIndex = -1;

                _bookCover.SetTexture(_pagesData.BookCoverUrl, true);

                _bookSoundPlayer.Prepare(GetAudioSoundList(pagesData), pagesData.BookId, callback);

                _chaperSelectPanel.Refresh(GetButtonNames(pagesData.PageDatas.Count), GetSelectionName(pageIndex));
            }
            else
            {
                callback?.Invoke();
            }
        }
        
        public void Clear()
        {
            _currentPageIndex = -1;
            _pagesData = null;
        }

        public void ToggleVIPLock(bool locked)
        {
            _vipLock = locked;
            _chaperSelectPanel.ToggleLock(locked);
        }

        public void TurnPage(int pageIndex, double initTime = 0)
        {
            pageIndex = Mathf.Clamp(pageIndex, 0, _pagesData.PageDatas.Count - 1);
            
            if ((_vipLock && !GameManager.IsFreeChapter(pageIndex) && !_pagesData.IsFree))
            {
                return;
            }

            if (_currentPageIndex == pageIndex)
            {
                if (_bookSoundPlayer.IsPlaying())
                {
                    return;
                }
            }

            _currentPageIndex = pageIndex;

            BookPageData currentPageData = _pagesData.PageDatas[_currentPageIndex];
            _title.text = currentPageData.Title;
            _content.text = currentPageData.Content;
            _soundSlider.UpdateProgress(0, 0);
            if (!GameManager.RuntimeDataManager.IsTextPageShown)
            {
                if (_bookSoundPlayer.Play(_currentPageIndex))
                {
                    _bookSoundPlayer.OnAudioPlayerEvent += SeekToPositionAndPlay;
                }
            }
            else
            {
                if (_bookSoundPlayer.CurrentStatus ==
                    Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus.Playing)
                {
                    if (_bookSoundPlayer.Play(_currentPageIndex))
                    {
                        _bookSoundPlayer.OnAudioPlayerEvent += SeekToPositionAndPlay;
                    }
                }
                else
                {
                    if (_bookSoundPlayer.Play(_currentPageIndex))
                    {
                        _bookSoundPlayer.OnAudioPlayerEvent += SeekToPosition;
                    }
                }
            }

            _chaperSelectPanel.SelectButton(GetSelectionName(_currentPageIndex));

            void SeekToPositionAndPlay(Framework.Runtime.Utils.AudioPlayer.AudioPlayer.EventType eventType)
            {
                if (eventType == Framework.Runtime.Utils.AudioPlayer.AudioPlayer.EventType.StartPlay)
                {
                    _bookSoundPlayer.OnAudioPlayerEvent -= SeekToPositionAndPlay;
                    _bookSoundPlayer.SetTime((float) initTime);
                }
            }
            
            void SeekToPosition(Framework.Runtime.Utils.AudioPlayer.AudioPlayer.EventType eventType)
            {
                if (eventType == Framework.Runtime.Utils.AudioPlayer.AudioPlayer.EventType.StartPlay)
                {
                    _bookSoundPlayer.OnAudioPlayerEvent -= SeekToPosition;
                    _bookSoundPlayer.SetTime((float) initTime);
                    _bookSoundPlayer.Pause();
                }
            }
        }

        public void TriggerPlayButton(bool fromRead = false)
        {
            if (!CheckData())
                return;
            //播放列表设置
            EventData data = new EventData();
            data.BookID = _pagesData.BookId;
            data.State = PlayItemState.kUnknown;
            BaseLogger.Log(nameof(BookSoundPlayerPageUI),$"TriggerPlayButton: current status is {_bookSoundPlayer.CurrentStatus.ToString()}");
            if (_bookSoundPlayer.CurrentStatus == Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus.Paused)
            {
                TrackEvent(fromRead
                    ? BookwavesAnalytics.Event_BookContent_ClickReadPlay
                    : BookwavesAnalytics.Event_BookContent_ClickListenPlay);

                _bookSoundPlayer.Resume();
                data.State = PlayItemState.kListening;
                _playButton.ChangeButtonStatus("Pause");
                _triggerPlayButtonCallback?.Invoke(Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus.Paused);
            }
            else if (_bookSoundPlayer.CurrentStatus ==
                     Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus.Stopped)
            {
                TrackEvent(fromRead
                    ? BookwavesAnalytics.Event_BookContent_ClickReadPlay
                    : BookwavesAnalytics.Event_BookContent_ClickListenPlay);
                _playButton.ChangeButtonStatus("Pause");
                data.State = PlayItemState.kListening;
                _bookSoundPlayer.Play(_currentPageIndex);
                _triggerPlayButtonCallback?.Invoke(Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus.Stopped);
            }
            else if (_bookSoundPlayer.CurrentStatus ==
                     Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus.Playing)
            {
                TrackEvent(fromRead
                    ? BookwavesAnalytics.Event_BookContent_ClickReadPause
                    : BookwavesAnalytics.Event_BookContent_ClickListenPause);

                _bookSoundPlayer.Pause();
                _playButton.ChangeButtonStatus("Play");
                data.State = PlayItemState.kUnknown;
                _triggerPlayButtonCallback?.Invoke(Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus.Playing);
            }

            PlayListCenter.Instance.TriggerEvent(PlayCenterEvent.kChangeItemState, data);
        }

        public void StopMusic()
        {
            _bookSoundPlayer.Stop();
        }

        public void Pause()
        {
            if (_bookSoundPlayer.CurrentStatus == Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus.Playing)
            {
                _bookSoundPlayer.Pause();
            }
        }

        public void RefreshText(BookPagesData pagesData)
        {
            _pagesData = pagesData;
            BookPageData currentPageData = _pagesData.PageDatas[0];
            _title.text = currentPageData.Title;
            _chaperSelectPanel.Refresh(GetButtonNames(_pagesData.PageDatas.Count), GetSelectionName(0));
        }

        public double GetCurrentSeconds()
        {
            return _bookSoundPlayer.CurrentSeconds;
        }

        private void HandleOnSliderEndDrag(float sliderValue)
        {
            _bookSoundPlayer.SetProgress(sliderValue);
        }

        private float GetCurrentSpeed()
        {
            return _speedSelectionText.CurrentSpeed;
        }

        private void HandleOnSpeedSelectTextTap(string oldText, string newText)
        {
            TrackEvent(BookwavesAnalytics.Event_BookContent_ClickListenSpeed);

            _bookSoundPlayer.SetSpeed(GetCurrentSpeed());
        }

        private void HandleOnProgressUpdate(float current, float total)
        {
            _soundSlider.UpdateProgress(current, total);
        }

        private string GetSelectionName(int pageIndex)
        {
            string tmpString = "Wave";
            GlobalEvent.GetEvent<GetLocalizationEvent>().Publish(tmpString, s => tmpString = s);
            return $"{tmpString} {pageIndex + 1}";
        }

        private List<string> GetButtonNames(int pageCount)
        {
            List<string> names = new List<string>();
            GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("Wave", s =>
            {
                for (int i = 0; i < pageCount; i++)
                {
                    names.Add($"{s} {i + 1}");
                }
            });


            return names;
        }

        private void HandleOnPlayButtonTap(string currentStatus)
        {
            TriggerPlayButton();
        }

        private void HandleOnAudioPlayerTriggerEvent(
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.EventType status)
        {
            if (status == Framework.Runtime.Utils.AudioPlayer.AudioPlayer.EventType.FinishedPlay)
            {
                if (!GameManager.RuntimeDataManager.IsTextPageShown)
                {
                    FlipPage();
                }
            }
            else if (status == Framework.Runtime.Utils.AudioPlayer.AudioPlayer.EventType.FinishedPlayList)
            {
                TrackEvent(BookwavesAnalytics.Event_BookContent_ListenEnd);
                Debug.Log("<debug> End listen ");
                if (AutoHandleOnNextBook())
                {
                    BookwavesNativeUtility.TryWeeklyRateUs();
                }
            }
        }

        private void HandleOnBookSoundPlayerStatusChanged(
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus oldStatus,
            Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus newStatus)
        {
            if (newStatus == Framework.Runtime.Utils.AudioPlayer.AudioPlayer.PlayerStatus.Playing)
            {
                _playButton.ChangeButtonStatus("Pause");
                _onBookSoundPlayerStatusChangedCallback?.Invoke(true);
            }
            else
            {
                _playButton.ChangeButtonStatus("Play");
                _onBookSoundPlayerStatusChangedCallback?.Invoke(false);
            }
        }

        private void FlipPage(bool returnToFirst = false)
        {
            BaseLogger.Log(nameof(BookSoundPlayerPageUI),$"FlipPage");
            if (_currentPageIndex < _pagesData.PageDatas.Count - 1)
            {
                _onTurnPageCallback?.Invoke(_currentPageIndex + 1, 0);
            }
            else if (returnToFirst)
            {
                _onTurnPageCallback?.Invoke(0, 0);
            }
            else
            {
                ShowToast("This is the final wave");
            }
        }

        private List<AudioPlayerClip> GetAudioSoundList(BookPagesData pagesData)
        {
            List<AudioPlayerClip> clipInfos = new List<AudioPlayerClip>();
            List<string> clipUrls = null;
            // check cache first
            GlobalEvent.GetEvent<GetBookSoundCacheEvent>().Publish(pagesData.BookId, cache =>
            {
                if (cache != null)
                {
                    clipUrls = cache;
                }
            });

            bool fromRemote = clipUrls == null;
            // if cache is null, get from server
            clipUrls = clipUrls ??= pagesData.PageDatas.ConvertAll(d => d.AudioUrl);
            
            string coverUrl = FileCacher.Get3WLoaderFullPath(pagesData.BookCoverUrl);
            bool bNative = fromRemote;
            string url = "";
            bool deleteCacheRecord = false;
            for (int i = 0; i < clipUrls.Count; i++)
            {
                url = clipUrls[i];
                bNative = !fromRemote;
                
                if (!fromRemote)
                {
                    if (!File.Exists(clipUrls[i]))
                    {
                        bNative = false;
                        url = pagesData.PageDatas[i].AudioUrl;
                        deleteCacheRecord = true;
                    }
                }
                clipInfos.Add(new AudioPlayerClip
                {
                    audioUrl = url,
                    author = pagesData.AuthorName,
                    audioType = bNative ? 0 : 1,
                    imageUrl = coverUrl,
                    name = pagesData.PageDatas[i].Title
                });
            }

            if (deleteCacheRecord)
            {
                GlobalEvent.GetEvent<DeleteBookSoundCacheEvent>().Publish(pagesData.BookId);
            }

            return clipInfos;
        }

        private void HandleOnSelectionButtonTap(string buttonName)
        {
            TrackEvent(BookwavesAnalytics.Event_BookContent_ClickListenChapter);

            int[] pages = buttonName.GetIntArray();
            if (pages != null && pages.Length == 1)
            {
                _onTurnPageCallback?.Invoke(pages[0] - 1, 0);
            }
        }

        private bool AutoHandleOnNextBook()
        {
            if (_pagesData == null)
            {
                BaseLogger.Log(nameof(BookSoundPlayerPageUI),$"auto handle on next book, but pages data is null",LogType.Error);
                return false;
            }
            EventData d = new EventData();
            d.BookID = _pagesData.BookId;
            if (!_pagesData.IsFree && !GameManager.IsGameUnlocked)
            {
                GlobalEvent.GetEvent<OpenStoreEvent>().Publish(BookwavesAnalytics.Prefix_Playlist);
            }
            else
            {
                PlayListCenter.Instance.TriggerEvent(PlayCenterEvent.kRequestNextBook, d);
            }

            return true;
        }

        private void TrackEvent(string eventName)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(eventName);
        }

        private void ShowToast(string text)
        {
            _allowToast = false;
            
            GlobalEvent.GetEvent<GetLocalizationEvent>().Publish(text, localizedText =>
            {
                GlobalEvent.GetEvent<ShowToastEvent>().Publish(localizedText, 1.5f);
            });

            StartCoroutine(DelayEnableToast());
        }

        private IEnumerator DelayEnableToast()
        {
            yield return new WaitForSeconds(1f);

            _allowToast = true;
        }
    }
}