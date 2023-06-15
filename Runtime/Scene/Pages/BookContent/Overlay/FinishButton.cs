using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.Overlay
{
    public class FinishButton : PageButton
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color backgroundNormalColor;
        [SerializeField] private Color backgroundDarkColor;

        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private Color buttonTextDarkColor;
        [SerializeField] private Color buttonTextNormalColor;

        public override void SetUp(Action<bool> callback)
        {
            if (_leftButton != null)
            {
                _leftButton.onClick.AddListener(() => { callback.Invoke(true); });
            }
        }

        public void FinishButtonToggle(bool on)
        {
            canvasGroup.alpha = on ? 1f : 0f;
            canvasGroup.interactable = on;
            canvasGroup.blocksRaycasts = on;
        }

        public void DarkMode()
        {
            _leftButton.image.color = buttonDarkColor;
            backgroundImage.color = backgroundDarkColor;
            buttonText.color = buttonTextDarkColor;
            foreach (TextMeshProUGUI text in _pageText)
            {
                text.color = textDarkColor;
            }
        }

        public void NormalMode()
        {
            _leftButton.image.color = buttonNormalColor;
            backgroundImage.color = backgroundNormalColor;
            buttonText.color = buttonTextNormalColor;
            foreach (TextMeshProUGUI text in _pageText)
            {
                text.color = textNormalColor;
            }
        }
    }
}