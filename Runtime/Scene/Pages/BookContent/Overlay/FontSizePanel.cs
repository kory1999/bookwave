using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.Overlay
{
    public class FontSizePanel : MonoBehaviour
    {
        [SerializeField] private RectTransform fontSizePanel;
        [SerializeField] private Slider fontSizeSlider;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image sliderImage;
        [SerializeField] private Image[] sizeImages;

        [SerializeField] private Color backgroundNormalColor;
        [SerializeField] private Color backgroundDarkColor;
        [SerializeField] private Color sliderNormalColor;
        [SerializeField] private Color sliderDarkColor;
        [SerializeField] private Color sizeImageNormalColor;
        [SerializeField] private Color sizeImageDarkColor;

        public void Initialize(Action<float> fontSizeCallback)
        {
            fontSizeSlider.onValueChanged.AddListener(value => fontSizeCallback.Invoke(value));
        }

        public void FontSizePanelToggle(bool on,Action callback)
        {
            if (on)
            {
                fontSizePanel.DOAnchorPosY(-(fontSizePanel.sizeDelta.y/2f + fontSizePanel.parent.GetComponent<RectTransform>().sizeDelta.y), 0.3f).onComplete = ()=>callback?.Invoke();;
            }
            else
            {
                fontSizePanel.DOAnchorPosY(0, 0.3f).onComplete = ()=>callback?.Invoke();
                
            }
        }

        public void DarkMode()
        {
            DarkModeBackground(backgroundImage);
            sliderImage.color = sliderDarkColor;
            for (int i = 0; i < sizeImages.Length; i++)
            {
                sizeImages[i].color = sizeImageDarkColor;
            }
        }

        public void NormalMode()
        {
            NormalModeBackground(backgroundImage);
            sliderImage.color = sliderNormalColor;
            for (int i = 0; i < sizeImages.Length; i++)
            {
                sizeImages[i].color = sizeImageNormalColor;
            }
        }
        
        public void DarkModeBackground(Image backgroundImage)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundDarkColor;    
            }
        }

        public void NormalModeBackground(Image backgroundImage)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundNormalColor;
            }
        }
    }
}