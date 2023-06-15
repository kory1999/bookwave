using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent
{
    public class PageButton : MonoBehaviour
    {
        [SerializeField] protected Button _leftButton;
        [SerializeField] protected Button _rightButton;
        [SerializeField] protected TextMeshProUGUI[] _pageText;

        [FormerlySerializedAs("darkColor")] [SerializeField]
        protected Color buttonDarkColor;

        [SerializeField] protected Color textDarkColor;

        [FormerlySerializedAs("normalColor")] [SerializeField]
        protected Color buttonNormalColor;

        [SerializeField] protected Color textNormalColor;

        private Action<bool> clickButtonEvent;
        private bool _isLocked;


        public virtual void SetUp(Action<bool> callback)
        {
            clickButtonEvent = callback;
            _leftButton.onClick.AddListener(() => { clickButtonEvent.Invoke(true); });
            _rightButton.onClick.AddListener(() => { clickButtonEvent.Invoke(false); });
        }

        public virtual void ToggleVIPLock(bool on)
        {
            _isLocked = on;
        }

        public virtual void Refresh(int currentIndex = 0, int totalIndex = 0)
        {
            _pageText[0].text = $"{currentIndex}/{totalIndex}";
            if (currentIndex == 1)
            {
                _leftButton.gameObject.SetActive(false);
            }
            else
            {
                _leftButton.gameObject.SetActive(true);
            }

            if (currentIndex == totalIndex)
            {
                _rightButton.gameObject.SetActive(false);
            }
            else
            {
                _rightButton.gameObject.SetActive(true);
            }
        }

        public virtual void DarkMode()
        {
            if (_isLocked)
            {
                buttonDarkColor.a = 0;
            }
            else
            {
                buttonDarkColor.a = 1;
            }

            if (_leftButton.image != null)
            {
                _leftButton.image.color = buttonDarkColor;
            }

            if (_rightButton.image != null)
            {
                _rightButton.image.color = buttonDarkColor;
            }
            
            foreach (TextMeshProUGUI text in _pageText)
            {
                text.color = textDarkColor;
            }
        }

        public virtual void NormalMode()
        {
            if (_isLocked)
            {
                buttonNormalColor.a = 0;
            }
            else
            {
                buttonNormalColor.a = 1;
            }

            if (_leftButton.image != null)
            {
                _leftButton.image.color = buttonNormalColor;
            }
            if (_rightButton.image != null)
            {
                _rightButton.image.color = buttonNormalColor;
            }
            
            foreach (TextMeshProUGUI text in _pageText)
            {
                text.color = textNormalColor;
            }
        }

        public virtual void SetDarkMode(bool value)
        {
            if (value)
            {
                _leftButton.image.color = buttonDarkColor;
                _rightButton.image.color = buttonDarkColor;
                foreach (TextMeshProUGUI text in _pageText)
                {
                    text.color = textDarkColor;
                }
            }
            else
            {
                _leftButton.image.color = buttonNormalColor;
                _rightButton.image.color = buttonNormalColor;
                foreach (TextMeshProUGUI text in _pageText)
                {
                    text.color = textNormalColor;
                }
            }
        }
    }
}