using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Library
{
    public class LibraryHistoryBookPopup : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button[] button;

        [SerializeField] private Color selectedColor;
        [SerializeField] private Color unSelectColor;

        public void SetUp(Action finishCallback, Action shareCallback, Action deleteCallback,Action downLoadCallback)
        {
           
            button[0].onClick.AddListener(() =>
            {
                button[0].GetComponent<Image>().color = selectedColor;
                finishCallback.Invoke();
                ToggleVisual(false);
            });
            button[1].onClick.AddListener(() =>
            {
                button[1].GetComponent<Image>().color = selectedColor;
                shareCallback.Invoke();
                ToggleVisual(false);
            });
            button[2].onClick.AddListener(() =>
            {
                button[2].GetComponent<Image>().color = selectedColor;
                deleteCallback.Invoke();
                ToggleVisual(false);
            });
            button[3].onClick.AddListener(() =>
            {
                button[3].GetComponent<Image>().color = selectedColor;
                downLoadCallback.Invoke();
                ToggleVisual(false);
            });
            
        }

        public void ToggleFinishedButton(bool on)
        {
            button[0].gameObject.SetActive(on);
        }
        
        public void ToggleDownloadButton(bool on)
        {
            button[3].gameObject.SetActive(on);
        }

        public void ToggleVisual(bool on)
        {
            if (!on)
            {
                for (int i = 0; i < button.Length; i++)
                {
                    button[i].GetComponent<Image>().color = unSelectColor;
                }
            }
            canvasGroup.alpha = on ? 1f : 0f;
            canvasGroup.interactable = on;
            canvasGroup.blocksRaycasts = on;
        }
    }
}