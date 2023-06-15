using System;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using BeWild.AIBook.Runtime.Scene.Pages.Home.PlayList;
using BeWild.Framework.Runtime.Utils.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent
{
    public class BookFloatWindow : MonoBehaviour
    {
        [SerializeField] private DraggableImage _draggableImage;
        [SerializeField] private RawImageHolder _rawImage;
        [SerializeField] private Button _openContainerButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _playlistButton;

        private float _leftAnchoredPositionX;
        private float _rightAnchoredPositionX;
        private float _leftAttachedPositionX;
        private float _rightAttachedPositionX;
        private float _leftHidePositionX;
        private float _rightHidePositionX;
        private Tweener _tweener;
        private RectTransform _myRectTransform;
        private Action<bool> _tapCallback;
        private bool _isInScreen;
        public bool IsInScreen => _isInScreen;

        public void Setup(RectTransform canvasTransform, Vector3 leftBorder, Vector3 rightBorder,
            Action<bool> tapCallback)
        {
            _myRectTransform = GetComponent<RectTransform>();
            _tapCallback = tapCallback;
            Vector2 leftScreenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, leftBorder);
            Vector2 rightScreenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, rightBorder);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, leftScreenPoint, Camera.main,
                out Vector2 leftAnchoredPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, rightScreenPoint, Camera.main,
                out Vector2 rightAnchoredPosition);
            _leftAnchoredPositionX = leftAnchoredPosition.x;
            _leftAttachedPositionX = leftAnchoredPosition.x + _myRectTransform.sizeDelta.x * _myRectTransform.pivot.x;
            _leftHidePositionX = leftAnchoredPosition.x - _myRectTransform.sizeDelta.x * (1 - _myRectTransform.pivot.x);
            _rightAnchoredPositionX = rightAnchoredPosition.x;
            _rightAttachedPositionX =
                rightAnchoredPosition.x - _myRectTransform.sizeDelta.x * (1 - _myRectTransform.pivot.x);
            _rightHidePositionX = rightAnchoredPosition.x + _myRectTransform.sizeDelta.x * _myRectTransform.pivot.x;
            _draggableImage.OnEndDragEvent += HandleOnEndDrag;
            _openContainerButton.onClick.AddListener(HandleOnTap);
            _closeButton.onClick.AddListener(() =>
            {
                TrackEvent(BookwavesAnalytics.Event_FloatWindow_Close);

                _tapCallback?.Invoke(false);
            });

            _playlistButton.onClick.AddListener(() =>
            {
                TrackEvent(BookwavesAnalytics.Event_FloatWindow_ClickPlayList);
                
                PlayListCenter.Instance.TriggerEvent(PlayCenterEvent.kOpenPage, null);
                Debug.Log("<debug> show playlist");
            });

            GlobalEvent.GetEvent<LanguageUpdateEvent>().Subscribe(HandleOnLanguageUpdate);

            if (GameManager.IsPadDevice)
            {
                Vector3 position = _myRectTransform.localPosition;
                position.y = -300f;
                _myRectTransform.localPosition = position;
            }
        }

        public void Refresh(string url)
        {
            _rawImage.SetTexture(url);
        }

        private void OnDestroy()
        {
            _tweener?.Kill();
        }

        private void HandleOnEndDrag(PointerEventData eventData, DraggableImage draggableImage)
        {
            Show();
        }

        private void HandleOnTap()
        {
            if (!_draggableImage.IsDraging)
            {
                TrackEvent(BookwavesAnalytics.Event_FloatWindow_Click);

                _tapCallback?.Invoke(true);
            }
        }

        public void Show()
        {
            _isInScreen = true;

            TrackEvent(BookwavesAnalytics.Event_FloatWindow_Show);

            _tweener?.Kill();
            _draggableImage.AllowHorizontalDrag = false;
            _draggableImage.AllowVerticalDrag = false;
            float leftBorderDist = Mathf.Abs(_myRectTransform.anchoredPosition.x - _leftAnchoredPositionX);
            float rightBorderDist = Mathf.Abs(_myRectTransform.anchoredPosition.x - _rightAnchoredPositionX);
            RectTransform myRectTransform = GetComponent<RectTransform>();
            if (leftBorderDist < rightBorderDist)
            {
                _tweener = myRectTransform.DOAnchorPosX(_leftAttachedPositionX, 0.2f).SetEase(Ease.InSine).OnComplete(
                    () =>
                    {
                        _draggableImage.AllowHorizontalDrag = true;
                        _draggableImage.AllowVerticalDrag = true;
                    });
                ;
            }
            else
            {
                _tweener = myRectTransform.DOAnchorPosX(_rightAttachedPositionX, 0.2f).SetEase(Ease.InSine).OnComplete(
                    () =>
                    {
                        _draggableImage.AllowHorizontalDrag = true;
                        _draggableImage.AllowVerticalDrag = true;
                    });
                ;
            }
        }

        public void Hide()
        {
            _isInScreen = false;

            _tweener?.Kill();
            _draggableImage.AllowHorizontalDrag = false;
            _draggableImage.AllowVerticalDrag = false;
            float leftBorderDist = Mathf.Abs(_myRectTransform.anchoredPosition.x - _leftAnchoredPositionX);
            float rightBorderDist = Mathf.Abs(_myRectTransform.anchoredPosition.x - _rightAnchoredPositionX);
            RectTransform myRectTransform = GetComponent<RectTransform>();
            if (leftBorderDist < rightBorderDist)
            {
                _tweener = myRectTransform.DOAnchorPosX(_leftHidePositionX, 0.2f).SetEase(Ease.InSine).OnComplete(
                    () =>
                    {
                        _draggableImage.AllowHorizontalDrag = true;
                        _draggableImage.AllowVerticalDrag = true;
                    });
            }
            else
            {
                _tweener = myRectTransform.DOAnchorPosX(_rightHidePositionX, 0.2f).SetEase(Ease.InSine).OnComplete(
                    () =>
                    {
                        _draggableImage.AllowHorizontalDrag = true;
                        _draggableImage.AllowVerticalDrag = true;
                    });
            }
        }

        private void HandleOnLanguageUpdate()
        {
            // close float window if language is changed, because books are different between languages.
            if (_isInScreen)
            {
                _tapCallback?.Invoke(false);
            }
        }

        private void TrackEvent(string eventName)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(eventName);
        }
    }
}