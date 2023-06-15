using System;
using System.Collections;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BW.Framework.Utils;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.Content
{
    public class PageContent : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public HighlightText BodyText => _bodyText;

        [SerializeField] private HighlightText _titleText;
        [SerializeField] private HighlightText _bodyText;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _prefabParent;
        [SerializeField] private PageButton _pageButtonPrefab;
        [SerializeField] private Transform _flipPageButtonPlaceHolder;
        [SerializeField] private Transform _satisfiedButtonPlaceHolder;
        [SerializeField] private Transform _finalPagePlaceHolder;
        [SerializeField] private Transform _notFinalPagePlaceHolder;
        [SerializeField] private PageButton _satisfactionButtonPrefab;
        [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;
        [SerializeField] private Color _titleDarkColor;
        [SerializeField] private Color _bodyDarkColor;
        [SerializeField] private Color _caretDarkModeColor;
        [SerializeField] private Color _caretNormalModeColor;
        [SerializeField] private Color _highlightTextDarkModeColor;
        [SerializeField] private Color _highlightTextNormalModeColor;
        [SerializeField] private Sprite _caretStartMarkDarkModeSprite;
        [SerializeField] private Sprite _caretStartMarkNormalModeSprite;
        [SerializeField] private Sprite _caretEndMarkDarkModeSprite;
        [SerializeField] private Sprite _caretEndMarkNormalModeSprite;

        private Color _titleNormalColor;
        private Color _bodyNormalColor;

        private PageButton _pageButton;
        private PageButton _satisfactionButton;
        private bool _isDarkMode;

        private Action<bool> _turnPageEvent;
        private Action<bool> _satisfactionEvent;
        private Action<float> _onScrollPageEvent;
        private Action _onEndScrollPageEvent;
        private Action<Vector3> _showPanelCallback;
        private Action<bool> _selectModeCallback;
        private Action _beginDragCallback;
        private Action _onScrollRectStopCallback;
        private Action _onSelectModeCloseCallback;

        private Coroutine _waitForStopCoroutine;

        private bool _enableHorizontalDrag = true;

        #region Interface

        public void Initialize(Action<bool> turnPageCallback, Action<float> scrollPageCallback,
            Action<bool> satisficatonCallback, Action<Vector3> showPanelCallback, Action OnSelectCloseCallback,
            Action<bool> selectModeCallback, Action beginDragCallback,
            Action endScrollPageCallback, Action onScrollRectStopCallback,
            bool isPad)
        {
            _finalPagePlaceHolder.gameObject.SetActive(false);
            _satisfiedButtonPlaceHolder.gameObject.SetActive(false);
            _flipPageButtonPlaceHolder.gameObject.SetActive(false);
            _notFinalPagePlaceHolder.gameObject.SetActive(false);
            _onScrollRectStopCallback = onScrollRectStopCallback;
            _selectModeCallback = selectModeCallback;
            _showPanelCallback = showPanelCallback;
            _turnPageEvent = turnPageCallback;
            _satisfactionEvent = satisficatonCallback;
            _onScrollPageEvent = scrollPageCallback;
            _onEndScrollPageEvent = endScrollPageCallback;
            _beginDragCallback = beginDragCallback;
            _titleNormalColor = _titleText.color;
            _bodyNormalColor = _bodyText.color;
            _onSelectModeCloseCallback = OnSelectCloseCallback;
            _bodyText.OnTriggerSelectModeFail += HandleOnSelectFail;
            _titleText.OnTriggerSelectModeFail += HandleOnSelectFail;
            _bodyText.OnTriggerSelectModeSuccess += HandleOnSelectSuccess;
            _titleText.OnTriggerSelectModeSuccess += HandleOnSelectSuccess;
            _bodyText.OnSelectModeFinishEvent += HandleOnHighlightFinish;
            _titleText.OnSelectModeFinishEvent += HandleOnHighlightFinish;
            _bodyText.OnSelectModeCloseEvent += HandleOnSelectModeClose;
            _titleText.OnSelectModeCloseEvent += HandleOnSelectModeClose;
            SetPadLayout(isPad);

            ListenToHighlightTextDrag();
        }

        public bool IsAnyContentBeSelected()
        {
            return _bodyText.IsAnyContentBeSelected();
        }

        public void Refresh(BookPage pageData, int currentIndex, int totalIndex)
        {
            gameObject.SetActive(true);
            _titleText.text = pageData.heading;
            _bodyText.text = pageData.content;


            if (_pageButton == null)
            {
                _flipPageButtonPlaceHolder.gameObject.SetActive(true);
                _flipPageButtonPlaceHolder.SetAsLastSibling();
                _pageButton = Instantiate(_pageButtonPrefab, _prefabParent, false);
                _pageButton.SetUp(_turnPageEvent);
            }

            _pageButton.Refresh(currentIndex + 1, totalIndex);


            //在最后一页显示满意度调查和完成按钮
            if (currentIndex + 1 == totalIndex)
            {
                if (_satisfactionButton == null)
                {
                    _satisfiedButtonPlaceHolder.gameObject.SetActive(true);
                    _satisfiedButtonPlaceHolder.SetAsLastSibling();
                    _satisfactionButton =
                        Instantiate(_satisfactionButtonPrefab, _prefabParent, false);
                    _satisfactionButton.SetUp(_satisfactionEvent);
                }

                _satisfactionButton.Refresh();
                _finalPagePlaceHolder.gameObject.SetActive(true);
                _finalPagePlaceHolder.SetAsLastSibling();
            }
            else
            {
                //如果不在最后一页，且两个按钮存在，则直接销毁
                if (_satisfactionButton != null)
                {
                    Destroy(_satisfactionButton.gameObject);
                }
                _notFinalPagePlaceHolder.gameObject.SetActive(true);
                _notFinalPagePlaceHolder.SetAsLastSibling();
            }
        }

        public void RefreshSelectArea()
        {
            _bodyText.RefreshSelectArea();
        }

        public void ToggleVIPLock(bool locked)
        {
            _pageButton.ToggleVIPLock(locked);

            _enableHorizontalDrag = !locked;
        }

        public void Highlight(int startIndex, int endIndex)
        {
            _bodyText.SetPartialTextColor(_isDarkMode ? _highlightTextDarkModeColor : _highlightTextNormalModeColor,
                startIndex, endIndex);
        }

        public void CancelHighlight(int startIndex, int endIndex)
        {
            _bodyText.SetPartialTextColor(_isDarkMode ? _bodyDarkColor : _bodyNormalColor, startIndex, endIndex);
        }

        public void MoveScrollerToCharacterIndex(int index)
        {
            Vector2 startCharacterWorldPosition = _bodyText.GetCharWorldPosition(index, true, 1);
            Vector3[] bodyWorldCorners = new Vector3[4];
            _bodyText.rectTransform.GetWorldCorners(bodyWorldCorners);
            Vector2 bodyTopPosition = bodyWorldCorners[1];
            float worldOffset = bodyTopPosition.y - startCharacterWorldPosition.y;
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(new Vector3(0f, worldOffset, 0f));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_scrollRect.viewport, screenPoint, Camera.main,
                out Vector2 rectOffset);
            Vector2 position = _scrollRect.content.localPosition;
            position.y = rectOffset.y - _scrollRect.viewport.rect.height / 2f;
            _scrollRect.content.localPosition = position;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetFontSize(float size)
        {
            _bodyText.fontSize = 50 + size;
        }

        public void DarkMode()
        {
            _isDarkMode = true;
            _titleText.SelectColor = _caretDarkModeColor;
            _bodyText.SelectColor = _caretDarkModeColor;
            _titleText.StartMarkSprite = _caretStartMarkDarkModeSprite;
            _bodyText.StartMarkSprite = _caretStartMarkDarkModeSprite;
            _titleText.EndMarkSprite = _caretEndMarkDarkModeSprite;
            _bodyText.EndMarkSprite = _caretEndMarkDarkModeSprite;

            _titleText.color = _titleDarkColor;
            _bodyText.color = _bodyDarkColor;
            _pageButton.DarkMode();
            if (_satisfactionButton != null)
            {
                _satisfactionButton.DarkMode();
            }
        }

        public void NormalMode()
        {
            _isDarkMode = false;
            _titleText.SelectColor = _caretNormalModeColor;
            _bodyText.SelectColor = _caretNormalModeColor;
            _titleText.StartMarkSprite = _caretStartMarkNormalModeSprite;
            _bodyText.StartMarkSprite = _caretStartMarkNormalModeSprite;
            _titleText.EndMarkSprite = _caretEndMarkNormalModeSprite;
            _bodyText.EndMarkSprite = _caretEndMarkNormalModeSprite;

            _titleText.color = _titleNormalColor;
            _bodyText.color = _bodyNormalColor;
            _pageButton.NormalMode();
            if (_satisfactionButton != null)
            {
                _satisfactionButton.NormalMode();
            }
        }

        public void ClearHighlightSelect()
        {
            _titleText.CloseSelectMode();
            _bodyText.CloseSelectMode();
        }

        public Vector3? GetFirstHighlightCharWorldPosition()
        {
            if (_bodyText.HighlightStartIndex != null && _bodyText.HighlightEndIndex != null &&
                _bodyText.HighlightStartIndex != _bodyText.HighlightEndIndex)
            {
                Vector3? position = _bodyText.GetCharWorldPosition(_bodyText.HighlightStartIndex.Value, true, 0);
                BaseLogger.Log(nameof(PageContent),
                    $"find body text first char {_bodyText.HighlightStartIndex.Value} position {position}");
                return position;
            }
            else if (_titleText.HighlightStartIndex != null && _titleText.HighlightEndIndex != null &&
                     _titleText.HighlightStartIndex != _titleText.HighlightEndIndex)
            {
                Vector3? position = _titleText.GetCharWorldPosition(_titleText.HighlightStartIndex.Value, true, 0);
                BaseLogger.Log(nameof(PageContent),
                    $"find title text first char {_titleText.HighlightStartIndex.Value} position {position}");
                return position;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region SystemInterface

        public void OnDrag(PointerEventData eventData)
        {
            if (_enableHorizontalDrag)
            {
                if (Mathf.Abs(eventData.delta.y * 2) < Mathf.Abs(eventData.delta.x))
                {
                    _scrollRect.enabled = false;

                    _onScrollPageEvent.Invoke(eventData.delta.x);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _beginDragCallback?.Invoke();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_enableHorizontalDrag)
            {
                _onEndScrollPageEvent.Invoke();
            }

            _scrollRect.enabled = true;
            StartWaitDragStop();
        }

        #endregion

        private void SetPadLayout(bool value)
        {
            if (value)
            {
                _verticalLayoutGroup.padding.top = 160;
            }
        }

        private void HandleOnSelectModeClose()
        {
            ListenToHighlightTextDrag();
            _onSelectModeCloseCallback?.Invoke();
        }

        private void HandleOnSelectFail()
        {
            _selectModeCallback?.Invoke(false);
            ListenToHighlightTextDrag();
        }

        private void HandleOnSelectSuccess()
        {
            _selectModeCallback?.Invoke(true);
            UnsubscribeHighlightTextDrag();
        }

        private void HandleOnHighlightTextDrag(PointerEventData pointerEventData)
        {
            _scrollRect.OnDrag(pointerEventData);
            OnDrag(pointerEventData);
        }

        private void HandleOnHighlightTextBeginDrag(PointerEventData pointerEventData)
        {
            _scrollRect.OnBeginDrag(pointerEventData);
            OnBeginDrag(pointerEventData);
        }

        private void HandleOnHighlightTextEndDrag(PointerEventData pointerEventData)
        {
            _scrollRect.OnEndDrag(pointerEventData);
            OnEndDrag(pointerEventData);
        }

        private void ListenToHighlightTextDrag()
        {
            UnsubscribeHighlightTextDrag();
            _bodyText.OnDragEvent += HandleOnHighlightTextDrag;
            _bodyText.OnBeginDragEvent += HandleOnHighlightTextBeginDrag;
            _bodyText.OnEndDragEvent += HandleOnHighlightTextEndDrag;
            _titleText.OnDragEvent += HandleOnHighlightTextDrag;
            _titleText.OnBeginDragEvent += HandleOnHighlightTextBeginDrag;
            _titleText.OnEndDragEvent += HandleOnHighlightTextEndDrag;
        }

        private void UnsubscribeHighlightTextDrag()
        {
            _bodyText.OnDragEvent -= HandleOnHighlightTextDrag;
            _bodyText.OnBeginDragEvent -= HandleOnHighlightTextBeginDrag;
            _bodyText.OnEndDragEvent -= HandleOnHighlightTextEndDrag;
            _titleText.OnDragEvent -= HandleOnHighlightTextDrag;
            _titleText.OnBeginDragEvent -= HandleOnHighlightTextBeginDrag;
            _titleText.OnEndDragEvent -= HandleOnHighlightTextEndDrag;
        }

        private void HandleOnHighlightFinish()
        {
            Vector3? worldPosition = GetFirstHighlightCharWorldPosition();
            if (worldPosition != null)
            {
                _showPanelCallback?.Invoke(worldPosition.Value);
            }
        }

        private IEnumerator WaitDragStopCoroutine()
        {
            yield return new WaitUntil(() => { return _scrollRect.velocity.y == 0; });
            _onScrollRectStopCallback?.Invoke();
        }

        private void StopCheckCoroutine()
        {
            if (_waitForStopCoroutine != null)
            {
                StopCoroutine(_waitForStopCoroutine);
                _waitForStopCoroutine = null;
            }
        }

        private void StartWaitDragStop()
        {
            StopCheckCoroutine();
            _waitForStopCoroutine = StartCoroutine(WaitDragStopCoroutine());
        }
    }
}