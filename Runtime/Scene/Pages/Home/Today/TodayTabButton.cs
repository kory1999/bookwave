using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Today
{
    public class TodayTabButton : MonoBehaviour
    {
        [SerializeField] private Button tabButton;
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private Image tabBar;

        [SerializeField] private Color selectedColor;
        [SerializeField] private Color unSelectedColor;
        
        private int _tabID;

        private Action<int> _buttonClickEvent;

        public void Initialize(Action<int> callback)
        {
            _buttonClickEvent = callback;
            tabButton.onClick.AddListener(HandleOnButtonTap);
        }

        public void Refresh(int tabID,string tabName)
        {
            _tabID = tabID;
            buttonText.text = tabName;
        }

        public void Selected()
        {
            tabBar.gameObject.SetActive(true);
            buttonText.color = selectedColor;
            buttonText.fontStyle = FontStyles.Bold;
        }

        public void CancelSelected()
        {
            tabBar.gameObject.SetActive(false);
            buttonText.color = unSelectedColor;
            buttonText.fontStyle = FontStyles.Normal;
        }

        protected virtual void HandleOnButtonTap()
        {
            _buttonClickEvent?.Invoke(_tabID);
            tabBar.gameObject.SetActive(true);
        }
    }
}

