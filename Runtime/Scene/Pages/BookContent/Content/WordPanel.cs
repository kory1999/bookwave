using System;
using System.Collections;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BeWild.AIBook.Runtime.Scene.Pages.BookContent.Overlay;
using BW.Framework.Utils;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.Content
{
    public class WordPanel : MonoBehaviour
    {
        [SerializeField] private BookUI _bookUI;
        [SerializeField] private PageGroup _pageGroup;
        [SerializeField] private RectTransform _pagesParent;
        [SerializeField] private HighlightButton _hightLightButton;
        [SerializeField] private CanvasGroup _highlightButtonCanvasGroup;
        [SerializeField] private float _scrollSpeed;
        [SerializeField] private float fontSizeFactors;
        [SerializeField] private float _highLightButtonYOffset;
        [SerializeField] private RectTransform _highlightButtonTopAnchor;
        [SerializeField] private RectTransform _highlightButtonBottomAnchor;

        private Action<int,double> _refreshPageCallback;
        private Action _clickFinishButtonEvent;

        private float _scrollTime;

        private float _totalDistance;
        private Tweener _tweener;
        private BookContentData _bookContentData;
        private bool _isDarkMode;
        private bool _isShowFontSize;
        private bool _isPad;
        private bool _isFullScreen;
        private bool _allowClick = true;
        private int? _tempStartIndex;
        private int? _tempEndIndex;
        private AccountData _accountData;
        private Coroutine _setTextSizeCoroutine;
        private bool _isLocked = false;
        private bool _isHighlightMode = false;

        #region Interface

        public void Initialize(Action<bool> satisfactionCallback, Action finishCallback, Action hideCallback,
            Action<int,double> refreshPageCallback, Action playSoundCallback, Action changePageCallback)
        {
            _isDarkMode = false;
            _isShowFontSize = false;
            _isPad = false;
            _isFullScreen = false;
            IsPad();
            GlobalEvent.GetEvent<GetAccountDataEvent>().Publish(accountData => { _accountData = accountData; });
            _clickFinishButtonEvent = finishCallback;
            _refreshPageCallback = refreshPageCallback;

            _bookUI.Initialize(() =>
                {
                    TrackEvent(BookwavesAnalytics.Event_BookContent_ClickReadClose);

                    hideCallback?.Invoke();
                }, playSoundCallback, () =>
                {
                    TrackEvent(BookwavesAnalytics.Event_BookContent_ClickReadListen);

                    changePageCallback?.Invoke();
                }, ShowFontSizePanel, DarkMode,
                SetFontSize, ClickFinishButtonEvent, _isPad);
            _pageGroup.Initialize(satisfied =>
                {
                    TrackEvent((satisfied
                        ? BookwavesAnalytics.Prefix_BookContent_ClickReadSatisfied
                        : BookwavesAnalytics.Prefix_BookContent_ClickReadUnSatisfied) + GameManager.RuntimeDataManager.BookBriefData.id);

                    satisfactionCallback?.Invoke(satisfied);
                }, TurnPage, FullScreenMode, OnScrollPage, OnEndScrollPage, HandleOnSelectFinish,
                null, ToggleClick, HandleOnChildPageBeginDrag, HandleOnCurrentPageScrollStopMove, _isPad);
            _hightLightButton.Setup(HandleOnHighlightButtonTap);
        }

        public void SetAudioPlayButtonUI(bool on)
        {
            _bookUI.SetAudioPlayUISprite(on);
        }


        public void UpdateBook(BookContentData data)
        {
            _bookContentData = data;
            _pageGroup.Refresh(data);

            DelayInvoker.Instance.Invoke(this, RefreshHighlight, Time.deltaTime);
        }

        public void ToggleVIPLock(bool locked)
        {
            _isLocked = locked;
            _pageGroup.ToggleVIPLock(locked);
        }

        public void TurnPageByNumber(int number)
        {
            int value = (_pageGroup.CurrentPageNumber - 1) - number;
            _pageGroup.transform.position = _pagesParent.transform.position + _pageGroup.ScreenSize * value;
            _pageGroup.CurrentPageNumber = number + 1;
            SetBookUIMessage();
        }

        public void SetStartCharacterIndex(int index)
        {
            if (index == 0)
                return;
            DelayInvoker.Instance.Invoke(this,
                () => _pageGroup.CurrentPageContent.MoveScrollerToCharacterIndex(index),
                Time.deltaTime);
        }

        #endregion

        private void ShowHighlightPanel(Vector3 worldPosition)
        {
            _isHighlightMode = true;
            BaseLogger.Log(nameof(WordPanel), $"show HighlightPanel {worldPosition}");
            _tweener?.Kill();
            float currentAlpha = _highlightButtonCanvasGroup.alpha;
            _highlightButtonCanvasGroup.ToggleEnable(true);
            _highlightButtonCanvasGroup.alpha = currentAlpha;
            _highlightButtonCanvasGroup.transform.position =
                new Vector3(_highlightButtonCanvasGroup.transform.position.x, worldPosition.y, worldPosition.z);
            RectTransform highlightButtonRectTransform = _highlightButtonCanvasGroup.GetComponent<RectTransform>();
            Vector2 anchoredPosition = highlightButtonRectTransform.anchoredPosition;
            highlightButtonRectTransform.anchoredPosition = new Vector2(anchoredPosition.x,
                anchoredPosition.y + _highLightButtonYOffset);

            highlightButtonRectTransform.localPosition = new Vector3(highlightButtonRectTransform.localPosition.x,
                Mathf.Clamp(highlightButtonRectTransform.localPosition.y, _highlightButtonBottomAnchor.localPosition.y,
                    _highlightButtonTopAnchor.localPosition.y), highlightButtonRectTransform.localPosition.z);

            _tempStartIndex = _pageGroup.CurrentPageContent.BodyText.HighlightStartIndex;
            _tempEndIndex = _pageGroup.CurrentPageContent.BodyText.HighlightEndIndex;
            _tweener = DOTween.To(() => { return _highlightButtonCanvasGroup.alpha; },
                (newValue) => { _highlightButtonCanvasGroup.alpha = newValue; }, 1f, 0.2f);
        }

        private void HideHighlightPanel()
        {
            _isHighlightMode = false;
            BaseLogger.Log(nameof(WordPanel), $"Hide HighlightPanel");
            _tweener?.Kill();
            _tweener = DOTween.To(() => { return _highlightButtonCanvasGroup.alpha; },
                (newValue) => { _highlightButtonCanvasGroup.alpha = newValue; }, 0, 0.2f).OnComplete(() =>
            {
                _highlightButtonCanvasGroup.ToggleEnable(false);
            });
        }

        private void RefreshHighlight()
        {
            BookHighLightData currentBookHighlightData = _accountData.readMarks.Find((readmark) =>
            {
                return readmark.id == _bookContentData.id;
            });

            if (currentBookHighlightData != null)
            {
                currentBookHighlightData.marks.ForEach(mark =>
                {
                    string[] markSplit = mark.Split('-');
                    int pageIndex = int.Parse(markSplit[0]);
                    int startIndex = int.Parse(markSplit[1]);
                    int endIndex = int.Parse(markSplit[2]);

                    _pageGroup.BookPages[pageIndex].Highlight(startIndex, endIndex);
                });
            }
        }

        private void IsPad()
        {
            float physicscreen = 1.0f * Screen.width / Screen.height;
            if (physicscreen > 3.0f / 5)
            {
                _isPad = true;
            }
        }

        private void HandleOnChildPageBeginDrag()
        {
            HideHighlightPanel();
        }

        private void ToggleClick(bool enable)
        {
            _allowClick = enable;
        }

        private void TurnPage(bool value)
        {
            if (_isLocked)
            {
                if (!value)
                    _pageGroup.CurrentPageNumber += 1;
                else
                {
                    _pageGroup.CurrentPageNumber -= 1;
                }

                _refreshPageCallback.Invoke(_pageGroup.CurrentPageNumber - 1,0);
                return;
            }

            if (!value)
            {
                if (_pageGroup.CurrentPageNumber != _pageGroup.TotalPageNumber)
                {
                    TrackEvent(BookwavesAnalytics.Event_BookContent_ClickReadNext);

                    Vector3 endValue = _pagesParent.transform.position - _pageGroup.ScreenSize;
                    HideHighlightPanel();
                    _pageGroup.CloseSelectMode();
                    _pagesParent.transform.DOMoveX(endValue.x, 0.5f).OnComplete((() =>
                    {
                        _pageGroup.CurrentPageNumber += 1;
                        _refreshPageCallback.Invoke(_pageGroup.CurrentPageNumber - 1,0);
                    }));
                }
            }
            else
            {
                if (_pageGroup.CurrentPageNumber != 1)
                {
                    TrackEvent(BookwavesAnalytics.Event_BookContent_ClickReadPrevious);

                    Vector3 endValue = _pagesParent.transform.position + _pageGroup.ScreenSize;
                    HideHighlightPanel();
                    _pageGroup.CloseSelectMode();
                    _pagesParent.transform.DOMoveX(endValue.x, 0.5f).OnComplete((() =>
                    {
                        _pageGroup.CurrentPageNumber -= 1;
                        _refreshPageCallback.Invoke(_pageGroup.CurrentPageNumber - 1,0);
                    }));
                }
            }
        }

        private void HandleOnCurrentPageScrollStopMove()
        {
            if (_pageGroup.CurrentPageContent.IsAnyContentBeSelected())
            {
                Vector3? worldPosition = _pageGroup.CurrentPageContent.GetFirstHighlightCharWorldPosition();
                if (worldPosition != null)
                {
                    ShowHighlightPanel(worldPosition.Value);
                }
            }
        }

        private void OnScrollPage(float value)
        {
            _scrollTime += Time.deltaTime;
            if (value > 0)
            {
                Vector3 tmp = new Vector3(value, 0, 0);
                if (_pageGroup.CurrentPageNumber != 1 || _totalDistance < 0)
                {
                    _totalDistance += value;
                    _pagesParent.GetComponent<RectTransform>().localPosition += tmp;
                }
            }
            else
            {
                Vector3 tmp = new Vector3(value, 0, 0);
                if (_pageGroup.CurrentPageNumber != _pageGroup.TotalPageNumber || _totalDistance > 0)
                {
                    _totalDistance += value;
                    _pagesParent.GetComponent<RectTransform>().localPosition += tmp;
                }
            }
        }

        private void OnEndScrollPage()
        {
            float tmpL = _pagesParent.rect.width;
            float scrollL = _totalDistance / tmpL;
            
            if (_totalDistance > tmpL / 3 || (scrollL / _scrollTime > _scrollSpeed))
            {
                if (_pageGroup.CurrentPageNumber == 1)
                    return;
                _pageGroup.CurrentPageNumber -= 1;
                _pagesParent.transform.DOMoveX(-(_pageGroup.CurrentPageNumber - 1) * _pageGroup.ScreenSize.x, 0.5f);
                SetBookUIMessage();

                TrackEvent(BookwavesAnalytics.Event_BookContent_SwapReadPage);

                _refreshPageCallback.Invoke(_pageGroup.CurrentPageNumber - 1,0);
            }
            else if (-_totalDistance > tmpL / 3 || (scrollL / _scrollTime < -_scrollSpeed))
            {
                if (_pageGroup.CurrentPageNumber == _pageGroup.TotalPageNumber)
                    return;
                _pageGroup.CurrentPageNumber += 1;
                _pagesParent.transform.DOMoveX(-(_pageGroup.CurrentPageNumber - 1) * _pageGroup.ScreenSize.x, 0.5f);
                SetBookUIMessage();

                TrackEvent(BookwavesAnalytics.Event_BookContent_SwapReadPage);

                _refreshPageCallback.Invoke(_pageGroup.CurrentPageNumber - 1,0);
            }

            else
            {
                _pagesParent.transform.DOMoveX(-(_pageGroup.CurrentPageNumber - 1) * _pageGroup.ScreenSize.x, 0.5f);
            }

            _totalDistance = 0;
            _scrollTime = 0;
        }

        private void DarkMode()
        {
            TrackEvent(BookwavesAnalytics.Event_BookContent_ClickReadNightMode);

            if (_isDarkMode)
            {
                _hightLightButton.ChangeMode(false);
                _bookUI.NormalMode();
                _pageGroup.NormalMode();
            }
            else
            {
                _hightLightButton.ChangeMode(true);
                _bookUI.DarkMode();
                _pageGroup.DarkMode();
            }

            _isDarkMode = !_isDarkMode;
            DelayInvoker.Instance.Invoke(this, () => { RefreshHighlight(); }, Time.deltaTime);
        }

        private void ShowFontSizePanel()
        {
            if (!_isShowFontSize)
            {
                TrackEvent(BookwavesAnalytics.Event_BookContent_ClickReadFont);
            }

            _isShowFontSize = !_isShowFontSize;
            _bookUI.FontSizePanelToggle(_isShowFontSize);
        }

        private void SetFontSize(float value)
        {
            _pageGroup.SetFontSize(value * fontSizeFactors);
            if (_setTextSizeCoroutine != null)
            {
                DelayInvoker.Instance.StopCoroutine(_setTextSizeCoroutine);
                _setTextSizeCoroutine = null;
            }

            _setTextSizeCoroutine = DelayInvoker.Instance.Invoke(this, () =>
            {
                RefreshHighlight();
                _pageGroup.RefreshSelectArea();
            }, Time.deltaTime);
        }


        private void SetBookUIMessage()
        {
            _bookUI.SetTitleWord(_pageGroup.CurrentPageNumber);
            if (_pageGroup.CurrentPageNumber == _pageGroup.TotalPageNumber)
                _bookUI.FinishButtonToggle(true);
            else
            {
                _bookUI.FinishButtonToggle(false);
            }

            _bookUI.SetProgressBar((float)_pageGroup.CurrentPageNumber / _pageGroup.TotalPageNumber);
        }


        private void HandleOnSelectFinish(Vector3 position)
        {
            ShowHighlightPanel(position);
        }

        private void HandleOnHighlightButtonTap()
        {
            if (_pageGroup.CurrentPageContent.IsAnyContentBeSelected())
            {
                TrackEvent(BookwavesAnalytics.Event_BookContent_MarkReadHighlight);

                GlobalEvent.GetEvent<RecordBookHighlightEvent>().Publish(_bookContentData.id,
                    $"{_pageGroup.CurrentPageNumber - 1}-{_pageGroup.CurrentPageContent.BodyText.HighlightStartIndex.Value}-{_pageGroup.CurrentPageContent.BodyText.HighlightEndIndex.Value}");
                
                _pageGroup.CloseSelectMode();
                HideHighlightPanel();
            }
        }

        private void ClickFinishButtonEvent(bool on)
        {
            if (on)
            {
                TrackEvent(BookwavesAnalytics.Event_BookContent_ClickReadFinish);

                _clickFinishButtonEvent.Invoke();
            }
        }

        public void FullScreenMode()
        {
            if (!_allowClick)
            {
                _allowClick = true;
                return;
            }

            if (_isHighlightMode)
            {
                _pageGroup.CloseSelectMode();
                HideHighlightPanel();
                return;
            }
            
            if (_isFullScreen)
            {
                _pageGroup.BorderMode();
            }
            else
            {
                if (_isShowFontSize)
                {
                    ShowFontSizePanel();
                    return;
                }

                _pageGroup.FullScreenMode();
            }

            _bookUI.TopUIToggle(_isFullScreen);
            _isFullScreen = !_isFullScreen;
        }

        private void TrackEvent(string eventName)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(eventName);
        }
    }
}