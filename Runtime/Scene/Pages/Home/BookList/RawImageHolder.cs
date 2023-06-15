using System;
using BeWild.AIBook.Runtime.Global;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.BookList
{
    public class RawImageHolder : MonoBehaviour
    {
        private const string FallbackTextureLoadPath = "FunLearning/RawImageHolderFallbackTexture";
        private static Texture2D _fallbackTexture;
        
        protected RawImage RawImage;
        private string _currentUrl = String.Empty;
        private bool _usingFallbackTexture;

        public void SetTexture(string url, bool resizeImage = false, Action<bool> textureLoadedCallback = null)
        {
            if (RawImage == null)
            {
                RawImage = GetComponentInChildren<RawImage>(true);
            }

            if (RawImage != null)
            {
                GlobalEvent.GetEvent<GetImageEvent>().Publish(url, texture =>
                {
                    if (this != null && texture != null)
                    {
                        if (url != _currentUrl)
                        {
                            ClearTexture(_currentUrl);
                        }

                        if (IsValidTexture(texture))
                        {
                            _usingFallbackTexture = false;
                            
                            _currentUrl = url;

                            DoSetTextureLogic(texture, resizeImage);
                        }
                        else
                        {
                            LoadFallbackTexture();
                        }
                            
                        textureLoadedCallback?.Invoke(true);
                    }
                    else
                    {
                        textureLoadedCallback?.Invoke(false);
                        
                        GlobalEvent.GetEvent<ReleaseImageEvent>().Publish(url);
                    }
                });   
            }
        }

        protected virtual void DoSetTextureLogic(Texture2D texture, bool resizeImage)
        {
            RawImage.texture = texture;

            RawImage.color = RawImage.texture != null ? Color.white : Color.clear;

            if (resizeImage)
            {
                float widthHeightRatio = (float)texture.width / texture.height;
                RawImage.rectTransform.sizeDelta = new Vector2(
                    RawImage.rectTransform.sizeDelta.y * widthHeightRatio,
                    RawImage.rectTransform.sizeDelta.y);
            }
        }

        protected virtual void OnDestroy()
        {
            ClearTexture(_currentUrl);
        }

        protected virtual void ClearTexture(string url)
        {
            if (!_usingFallbackTexture && !string.IsNullOrEmpty(url))
            {
                GlobalEvent.GetEvent<ReleaseImageEvent>().Publish(url);
            }
        }

        private bool IsValidTexture(Texture2D texture)
        {
            return texture.width != 8 || texture.height != 8;
        }

        private void LoadFallbackTexture()
        {
            if (_fallbackTexture == null)
            {
                _fallbackTexture = Resources.Load<Texture2D>(FallbackTextureLoadPath);
            }

            _usingFallbackTexture = true;

            DoSetTextureLogic(_fallbackTexture, _fallbackTexture);
        }
    }
}