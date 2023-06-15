using System;
using System.Collections;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.Content
{
    public class SatisfactionButton : PageButton
    {
        [SerializeField] private Sprite _darkBackground;
        [SerializeField] private Sprite _whiteBackground;

        [SerializeField] private Image leftImage;
        [SerializeField] private Image rightImage;
        [SerializeField] private TMP_Text leftText;
        [SerializeField] private TMP_Text rightText;

        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color backgroundNormalColor;
        [SerializeField] private Color backgroundDarkColor;
        [SerializeField] private GameObject _yesSelectedVisual;
        [SerializeField] private GameObject _noSelectedVisual;
        [SerializeField] private GameObject _yesUnSelectedVisual;
        [SerializeField] private GameObject _noUnSelectedVisual;

        private Action<bool> _clickButtonEvent;
        private bool _isSelected;

        public override void SetUp(Action<bool> callback)
        {
            textNormalColor = _pageText[0].color;
            _clickButtonEvent = callback;

            _leftButton.onClick.AddListener(() => ClickButton(true));
            _rightButton.onClick.AddListener(() => ClickButton(false));
        }

        public override void Refresh(int currentIndex = 0, int totalIndex = 0)
        {
            _isSelected = false;

            // _leftButton.image.sprite = leftSprite;
            // _rightButton.image.sprite = rightSprite;
            // leftImage.gameObject.SetActive(true);
            // leftText.gameObject.SetActive(true);
            // rightImage.gameObject.SetActive(true);
            // rightText.gameObject.SetActive(true);
            _yesUnSelectedVisual.SetActive(true);
            _yesSelectedVisual.SetActive(false);
            _noUnSelectedVisual.SetActive(true);
            _noSelectedVisual.SetActive(false);
        }

        public override void DarkMode()
        {
            base.DarkMode();
            backgroundImage.color = backgroundDarkColor;
            backgroundImage.sprite = _darkBackground;
        }

        public override void NormalMode()
        {
            base.NormalMode();
            backgroundImage.color = backgroundNormalColor;
            backgroundImage.sprite = _whiteBackground;
        }


        private void ClickButton(bool leftButton)
        {
            if (!_isSelected)
            {
                if (leftButton)
                {
                    GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Prefix_BookBrief_ClickSatisfied + GameManager.RuntimeDataManager.BookBriefData.id);
                    
                    _clickButtonEvent?.Invoke(true);
                    _yesUnSelectedVisual.SetActive(false);
                    _yesSelectedVisual.SetActive(true);
                }
                else
                {
                    GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Prefix_BookBrief_ClickUnSatisfied + GameManager.RuntimeDataManager.BookBriefData.id);
                    
                    _clickButtonEvent?.Invoke(false);
                    _noUnSelectedVisual.SetActive(false);
                    _noSelectedVisual.SetActive(true);
                }

                _isSelected = true;
            }
        }
    }
}