using System;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class HomePageShelfBook : RawImageHolder
    {
        [SerializeField] private Button button;
        [SerializeField] private Transform flipTransform;

        private int _id;
        private string _author;
        private float _startTime;
        private float _desiredDelay;
        private Action _flipCallback;
        private string _urlToClear;

        public void Initialize(Action<int> callback)
        {
            button.onClick.AddListener(() => callback?.Invoke(_id));
        }
        
        public void PlayFlipAnimation(int id, string author, string url, float desiredDelay, Action callback)
        {
            _id = id;
            _author = author;
            _startTime = Time.realtimeSinceStartup;
            _desiredDelay = desiredDelay;
            _flipCallback = callback;
            
            SetTexture(url, false, HandleOnLoadTextureResult);
        }

        public void ToggleVisual(bool on)
        {
            gameObject.SetActive(on);
        }

        protected override void ClearTexture(string url)
        {
            _urlToClear = url;
        }

        protected override void DoSetTextureLogic(Texture2D texture, bool resizeImage)
        {
            if (RawImage.texture != null)
            {
                float delay = _desiredDelay - Time.realtimeSinceStartup + _startTime;
                delay = Mathf.Clamp(delay, 0f, float.MaxValue);
            
                flipTransform.DOScaleX(0f, 0.2f).SetEase(Ease.InQuad).SetDelay(delay).onComplete += () =>
                {
                    base.ClearTexture(_urlToClear);
                    UpdateVisual(texture, resizeImage, _author);
                    
                    flipTransform.DOScaleX(1f, 0.2f).SetEase(Ease.OutQuad).onComplete += () => _flipCallback?.Invoke();
                };
            }
            else
            {
                base.DoSetTextureLogic(texture, resizeImage);
                UpdateVisual(texture, resizeImage, _author);

                _flipCallback?.Invoke();
            }
        }

        private void UpdateVisual(Texture2D texture, bool resizeImage, string authorName)
        {
            base.DoSetTextureLogic(texture, resizeImage);
            GetComponentInChildren<TMP_Text>().text = authorName;
        }

        private void HandleOnLoadTextureResult(bool success)
        {
            if (!success)
            {
                _flipCallback?.Invoke();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            flipTransform.DOKill();
        }
    }
}