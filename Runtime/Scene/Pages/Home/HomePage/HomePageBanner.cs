using System;
using System.Collections;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using BW.Framework.Utils;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class HomePageBanner : ScrollRect
    {
        private const float BannerOffset = 860f;
        private const float ReleaseSpeedFactor = 0.3f;
        private const float ReleaseSpeedMin = 1.3f;
        private const float ReleaseSpeedMax = 12f;
        private const float Acceleration = 4f;
        
        private class Banner
        {
            public RawImageHolder image;
            public BannerData data;
        }
        
        private RawImageHolder bannerImagePrefab;
        private List<BannerData> _data;
        private List<Banner> _banners = new List<Banner>();
        private int _bannerCount;
        private int _dataCount;
        private float _pointer;             // the scroller value of banners, 0f means in center, -1f to most left, 1f to most right
        private Coroutine _autoSlide;
        private Action<int> _bannerTapCallback;

        private DataStatistics<float> _dragDeltaData = new DataStatistics<float>(5);
        private Tweener _slideTweener;

        private float _dragFactor;
        private float _bannerWidth;

        #region interface

        public void Initialize(Action<int> tapCallback)
        {
            _bannerTapCallback = tapCallback;

            bannerImagePrefab = GetComponentInChildren<RawImageHolder>();
            
            _banners.Add(new Banner(){image = bannerImagePrefab});
            _bannerCount = CalculateBannerCount();
            for (int i = 1; i < _bannerCount; i++)
            {
                _banners.Add(new Banner(){image = Instantiate(bannerImagePrefab.gameObject, bannerImagePrefab.transform.parent).GetComponent<RawImageHolder>()});
            }

            for (int i = 0; i < _bannerCount; i++)
            {
                int index = i;
                _banners[i].image.GetComponent<Button>().onClick.AddListener(() => HandleOnBannerTap(_banners[index].data.BookId));
            }

            _dragFactor = 1080f / Screen.width;
            _bannerWidth = bannerImagePrefab.GetComponent<RectTransform>().rect.width;
        }

        public void SetData(List<BannerData> data)
        {
            _data = new List<BannerData>();
            while (_data.Count < _bannerCount)
            {
                _data.AddRange(data);
            }

            _dataCount = _data.Count;

            UpdateVisual();

            ToggleAutoSlide(true);
        }

        #endregion

        #region scroll rect

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            
            KillSlideTweener();
            
            _dragDeltaData.Clear();

            ToggleAutoSlide(false);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);

            MoveBanners(eventData.delta.x * _dragFactor);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);

            (float, float) param = CalculateReleaseEndParameters();
            SlideTo(param.Item1, param.Item2, Ease.OutQuad, () =>
            {
                ToggleAutoSlide(true);
            });
            
            normalizedPosition = Vector2.one;
        }

        #endregion

        #region private

        private int CalculateBannerCount()
        {
            return 3;
        }

        private void HandleOnBannerTap(int id)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Prefix_Home_ClickBanner + id);
            
            _bannerTapCallback?.Invoke(id);
        }

        private void UpdateVisual()
        {
            UpdateBannersPosition();
        }

        private void ToggleAutoSlide(bool on)
        {
            if (!this.gameObject.activeSelf)
                return;
            
            if (_autoSlide != null)
            {
                StopCoroutine(_autoSlide);
            }
            
            if (on)
            {
                _autoSlide = StartCoroutine(AutoSlide());
            }
        }

        private IEnumerator AutoSlide()
        {
            WaitForSeconds delay = new WaitForSeconds(3f);
            while (true)
            {
                yield return delay;
                
                SlideTo(_pointer - 1, 0.5f, Ease.InOutQuad, null);
            }
        }

        // move in local position
        private void MoveBanners(float delta)
        {
            delta /= BannerOffset;
            
            _dragDeltaData.Append(delta);
            
            MovePointer(delta);
        }

        private void MovePointer(float delta)
        {
            if (delta != 0f)
            {
                SetPointerValue(_pointer + delta);
            }
        }

        private void SetPointerValue(float value)
        {
            bool leftToRight = value - _pointer >= 0f;
            
            _pointer = value;
            
            _pointer %= _dataCount;
            if (_pointer < 0f)
            {
                _pointer += _dataCount;
            }
            
            UpdateBannersPosition(leftToRight);
        }

        private void UpdateBannersPosition(bool leftToRight = true)
        {
            // find most in center data
            int mostInCenterIndex = Mathf.RoundToInt(_dataCount -_pointer) % _dataCount;

            // update position based on data pointer value, notice that we need to refresh banner in order of move to avoid texture reload of cache.
            if (leftToRight)
            {
                for (int i = _bannerCount-1; i >= 0; i--)
                {
                    int dataIndex = (- _bannerCount / 2 + i + mostInCenterIndex + _dataCount) % _dataCount;
                    UpdateBannerPosition(_banners[i], dataIndex);
                }
            }
            else
            {
                for (int i = 0; i < _bannerCount; i++)
                {
                    int dataIndex = (- _bannerCount / 2 + i + mostInCenterIndex + _dataCount) % _dataCount;
                    UpdateBannerPosition(_banners[i], dataIndex);
                }
            }
        }

        private void UpdateBannerPosition(Banner banner, int dataIndex)
        {
            BannerData data = _data[dataIndex];
            if (banner.data == null || banner.data.BookId != data.BookId)
            {
                banner.data = data;
                banner.image.SetTexture(data.ImageUrl);
            }
            
            var t = banner.image.transform;
            Vector3 position = t.localPosition;
            float offset = dataIndex + _pointer;
            offset %= _dataCount;
            if (offset > _bannerCount / 2f)
            {
                offset -= _dataCount;
            }

            float scaleFactor = Mathf.Lerp(1f, 0.85f, Mathf.Abs(offset));
            t.localScale = scaleFactor * Vector3.one;
            position.x = offset * BannerOffset - Mathf.Sign(offset) * _bannerWidth / 2f * (1f - scaleFactor);
            t.localPosition = position;
        }

        private (float, float) CalculateReleaseEndParameters()
        {
            float releaseVelocity = _dragDeltaData.Average() / Time.deltaTime * ReleaseSpeedFactor; // release speed is too large, so we multiply a factor

            // make sure it's easy to drag
            if (Mathf.Abs(releaseVelocity) < ReleaseSpeedMin)
            {
                releaseVelocity = Mathf.Sign(releaseVelocity) * ReleaseSpeedMin;
            }
            
            // mae sure it won't move a lot
            releaseVelocity = Mathf.Clamp(releaseVelocity, -ReleaseSpeedMax, ReleaseSpeedMax);

            float endValue = _pointer + releaseVelocity * Mathf.Abs(releaseVelocity) / Acceleration / 2f;
            endValue = Mathf.Round(endValue);
            float duration = Mathf.Abs(releaseVelocity) / Acceleration;
            
            return (endValue, duration);
        }

        private void SlideTo(float pointer, float duration, Ease ease, Action callback)
        {
            KillSlideTweener();

            _slideTweener = DOTween.To(SetPointerValue, _pointer, pointer, duration).SetEase(ease);
            _slideTweener.onComplete += () => callback?.Invoke();
        }

        private void KillSlideTweener()
        {
            if (_slideTweener != null && _slideTweener.IsPlaying())
            {
                _slideTweener.Kill();
            }
        }

        #endregion
    }
}