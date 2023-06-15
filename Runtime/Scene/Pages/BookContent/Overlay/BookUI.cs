using System;
using BeWild.AIBook.Runtime.Scene.Pages.BookContent.Overlay;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.Overlay
{
    public class BookUI : MonoBehaviour
    {
        [SerializeField] private TopUI topUI;
        [SerializeField] private FinishButton finishButton;
        [SerializeField] private Image progressBar;

        public void Initialize(Action quitCallback, Action audioPlayCallback, Action soundPageCallback,
            Action showFontSizePanelCallback,
            Action darkModeCallback, Action<float> fontSizeCallback, Action<bool> finishCallback, bool isPad)
        {
            topUI.Initialize(quitCallback, audioPlayCallback, soundPageCallback, showFontSizePanelCallback,
                darkModeCallback,
                fontSizeCallback,isPad);
            finishButton.SetUp(finishCallback);
        }
        
        


        //隐藏TopUI
        public void TopUIToggle(bool on)
        {
            topUI.CanvasGroupToggle(on);
        }

        public void FinishButtonToggle(bool on)
        {
            finishButton.FinishButtonToggle(on);
        }

        public void SetTitleWord(int value)
        {
            topUI.SetTitleWord(value);
        }

        public void SetAudioPlayUISprite(bool on)
        {
            topUI.SetAudioPlayButtonUI(on);
        }

        public void FontSizePanelToggle(bool on)
        {
            topUI.FontSizePanelToggle(on);
        }

        public void SetProgressBar(float value)
        {
            progressBar.fillAmount = value;
        }

        public void DarkMode()
        {
            topUI.DarkMode();
            finishButton.DarkMode();
        }

        public void NormalMode()
        {
            topUI.NormalMode();
            finishButton.NormalMode();
        }

    }
}