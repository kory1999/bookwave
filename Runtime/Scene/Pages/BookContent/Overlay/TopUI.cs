using System;
using System.Collections;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Global;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.Overlay
{
    public class TopUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private FontSizePanel fontSizePanel;
        [SerializeField] private RectTransform topPanel;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image safeAreaImage;
        [SerializeField] private Button quitButton;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button[] buttons;

        [SerializeField] private Sprite audioPlaySprite;
        [SerializeField] private Sprite audioPauseSprite;
        [SerializeField] private Image[] buttonImages;

        [SerializeField] private Color backgroundNormalColor;
        [SerializeField] private Color textNormalColor;
        [SerializeField] private Color buttonNormalColor;

        [SerializeField] private Color backgroundDarkColor;
        [SerializeField] private Color textDarkColor;
        [SerializeField] private Color buttonDarkColor;

        [SerializeField] private GameObject topButtonArrayObject;

        public void Initialize(Action quitCallback, Action audioPlayCallback, Action soundPageCallback,
            Action showFontSizePanelCallback, Action darkModeCallback,
            Action<float> fontSizeCallback, bool isPad)
        {
            quitButton.onClick.AddListener(quitCallback.Invoke);
            buttons[0].onClick.AddListener(audioPlayCallback.Invoke);
            buttons[1].onClick.AddListener(soundPageCallback.Invoke);

            fontSizePanel.Initialize(fontSizeCallback);

            buttons[2].onClick.AddListener(showFontSizePanelCallback.Invoke);
            buttons[3].onClick.AddListener(darkModeCallback.Invoke);

            if (isPad)
            {
                GetComponent<RectTransform>().DOAnchorPosY(-50, 0.5f);
            }
        }

        public void CanvasGroupToggle(bool on)
        {
            canvasGroup.alpha = on ? 1f : 0f;
            canvasGroup.interactable = on;
            canvasGroup.blocksRaycasts = on;
        }

        public void SetTitleWord(int value)
        {
            GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("Wave", s => { titleText.text = $"{s} {value}"; });
        }

        public void SetAudioPlayButtonUI(bool on)
        {
            if (on)
            {
                buttonImages[0].sprite = audioPauseSprite;
            }
            else
            {
                buttonImages[0].sprite = audioPlaySprite;
            }
        }

        public void FontSizePanelToggle(bool on)
        {
            if (topButtonArrayObject)
            {
                if (!on)
                {
                    topButtonArrayObject.SetActive(false);    
                }
            }

            fontSizePanel.FontSizePanelToggle(on, () =>
            {
                if (topButtonArrayObject)
                {
                    if (on)
                    {
                        topButtonArrayObject.SetActive(true);    
                    }
                }
            });
        }

        public void DarkMode()
        {
            fontSizePanel.DarkMode();
            fontSizePanel.DarkModeBackground(topButtonArrayObject.GetComponent<Image>());
            backgroundImage.color = backgroundDarkColor;
            safeAreaImage.color = backgroundDarkColor;
            titleText.color = textDarkColor;
            for (int i = 0; i < buttonImages.Length; i++)
            {
                buttonImages[i].color = buttonDarkColor;
            }
        }

        public void NormalMode()
        {
            fontSizePanel.NormalMode();
            fontSizePanel.NormalModeBackground(topButtonArrayObject.GetComponent<Image>());
            backgroundImage.color = backgroundNormalColor;
            safeAreaImage.color = backgroundNormalColor;
            titleText.color = textNormalColor;
            for (int i = 0; i < buttons.Length; i++)
            {
                buttonImages[i].color = buttonNormalColor;
            }
        }
    }
}