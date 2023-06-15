using System;
using System.Collections;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BW.Framework.Utils;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.Content
{
    public class PageGroup : MonoBehaviour, IPointerClickHandler, IPointerUpHandler
    {
        [SerializeField] private PageContent _pagePrefab;
        [SerializeField] private Image _background;
        [SerializeField] private Color _darkColor;
        [SerializeField] private Color _normalColor;

        private Action<bool> _satisfactionEvent;
        private Action<bool> _turnPageEvent;
        private Action _fullScreenEvent;
        private Action<float> _onScrollPageEvent;
        private Action _onEndScrollPageEvent;
        private Action<Vector3> _onSelectFinishEvent;
        private Action _onSelectCloseCallback;
        private Action<bool> _toggleClickCallback;
        private Action _onChildPageBeginDragCallback;
        private Action _onChildScrollMoveStopCallback;

        public int CurrentPageNumber { get; set; }
        public int TotalPageNumber { get; private set; }
        public Vector3 ScreenSize { get; private set; }

        public PageContent CurrentPageContent => _bookPages[CurrentPageNumber - 1];

        private List<PageContent> _bookPages;
        public List<PageContent> BookPages => _bookPages;

        private RectTransform _parentTransform;
        private Vector3 _marginPosition;
        private float _startPositionX;

        private float _topDistance;
        private BookContentData _bookContentData;
        private bool _isPad;
        private bool _dragging = false;
        private bool _isFullScreen;

        public void Initialize(Action<bool> satisfactionCallback, Action<bool> turnPageCallback,
            Action fullScreenCallback,
            Action<float> scrollPageCallback, Action endScrollPageCallback,
            Action<Vector3> onSelectFinishEvent, Action onSelectCloseCallback, Action<bool> toggleClickCallback,
            Action onChildPageBeginDragCallback, Action onChildScrollMoveStopCallback,
            bool isPad)
        {
            _isPad = isPad;
            _isFullScreen = false;
            SetInitSizeDeltaY();
            _onChildScrollMoveStopCallback = onChildScrollMoveStopCallback;
            _onChildPageBeginDragCallback = onChildPageBeginDragCallback;
            _onSelectCloseCallback = onSelectCloseCallback;
            _bookPages = new List<PageContent>();
            _onSelectFinishEvent = onSelectFinishEvent;
            _satisfactionEvent = satisfactionCallback;
            _turnPageEvent = turnPageCallback;
            _fullScreenEvent = fullScreenCallback;
            _onScrollPageEvent = scrollPageCallback;
            _onEndScrollPageEvent = endScrollPageCallback;
            _toggleClickCallback = toggleClickCallback;
            GlobalEvent.GetEvent<BookHighlightChangedEvent>().Subscribe(HandleOnHighlightDataChanged);
            _parentTransform = GetComponent<RectTransform>();
            _startPositionX = _parentTransform.position.x;

            CalculateScreenSize();
        }

        public void Refresh(BookContentData data)
        {
            CalculateScreenSize();
            _bookContentData = data;
            if (data.pages.Length > _bookPages.Count)
            {
                for (int i = 0; i < data.pages.Length; i++)
                {
                    if (i < _bookPages.Count)
                    {
                        _bookPages[i].Refresh(data.pages[i], i, data.pages.Length);
                    }
                    else
                    {
                        PageContent page = Instantiate(_pagePrefab, _parentTransform, false);
                        page.Initialize(_turnPageEvent, _onScrollPageEvent, _satisfactionEvent,
                            HandleOnSelectFinish, _onSelectCloseCallback, HandleOnSelectModeChanged, HandleOnBeginDrag,
                            HandleOnEndDrag, HandleOnChildScrollMoveStop, _isPad);

                        page.Refresh(data.pages[i], i, data.pages.Length);
                        _bookPages.Add(page);
                    }
                }
            }
            else
            {
                for (int i = 0; i < _bookPages.Count; i++)
                {
                    if (i < data.pages.Length)
                    {
                        _bookPages[i].Refresh(data.pages[i], i, data.pages.Length);
                    }
                    else
                    {
                        _bookPages[i].Hide();
                    }
                }
            }


            CurrentPageNumber = 1;
            TotalPageNumber = data.pages.Length;
            _parentTransform.position = new Vector3(_startPositionX, _parentTransform.position.y);
        }
        
        public void ToggleVIPLock(bool locked)
        {
            for (int i = 0; i < _bookPages.Count; i++)
            {
                _bookPages[i].ToggleVIPLock(locked && !GameManager.IsFreeChapter(i));
            }
        }

        public void SetFontSize(float size)
        {
            foreach (PageContent page in _bookPages)
            {
                page.SetFontSize(size);
            }
        }

        public void DarkMode()
        {
            _background.color = _darkColor;
            foreach (PageContent page in _bookPages)
            {
                page.DarkMode();
            }
        }

        public void NormalMode()
        {
            _background.color = _normalColor;
            foreach (PageContent page in _bookPages)
            {
                page.NormalMode();
            }
        }

        public void BorderMode()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, _topDistance);
            rectTransform.anchoredPosition = new Vector2(0, _topDistance / 2);
        }

        public void FullScreenMode()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 0);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        public void CloseSelectMode()
        {
            for (int i = 0; i < _bookPages.Count; i++)
            {
                _bookPages[i].ClearHighlightSelect();
            }
        }

        public void RefreshSelectArea()
        {
            for (int i = 0; i < _bookPages.Count; i++)
            {
                _bookPages[i].RefreshSelectArea();
            }
        }

        private void HandleOnHighlightDataChanged(int id, string mark,bool remove)
        {
            if (_bookContentData!=null && _bookContentData.id == id)
            {
                string[] markSplit = mark.Split('-');
                int pageIndex = int.Parse(markSplit[0]);
                int startIndex = int.Parse(markSplit[1]);
                int endIndex = int.Parse(markSplit[2]);

                if (remove)
                {
                    _bookPages[pageIndex].CancelHighlight(startIndex,endIndex);
                }
                else
                {
                    _bookPages[pageIndex].Highlight(startIndex,endIndex);
                }
            }
        }

        private void HandleOnChildScrollMoveStop()
        {
            _onChildScrollMoveStopCallback?.Invoke();
        }

        private void HandleOnBeginDrag()
        {
            _onChildPageBeginDragCallback?.Invoke();
            _toggleClickCallback?.Invoke(false);
            _dragging = true;
        }

        private void HandleOnEndDrag()
        {
            _dragging = false;
            _onEndScrollPageEvent?.Invoke();
            _toggleClickCallback?.Invoke(true);
        }

        private void HandleOnSelectModeChanged(bool inSelectMode)
        {
            if (inSelectMode)
            {
                _toggleClickCallback?.Invoke(false);
            }
        }


        private void SetInitSizeDeltaY()
        {
            _topDistance = GetComponent<RectTransform>().sizeDelta.y;
        }

        private void HandleOnSelectFinish(Vector3 position)
        {
            _onSelectFinishEvent?.Invoke(position);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _fullScreenEvent.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }

        private void CalculateScreenSize()
        {
            _marginPosition = Camera.main.GetLeftBottomWorldPosition();
            ScreenSize = new Vector3(-_marginPosition.x * 2, 0, 0);
        }

        private void OnDestroy()
        {
            GlobalEvent.GetEvent<BookHighlightChangedEvent>().Unsubscribe(HandleOnHighlightDataChanged);
        }
    }
}