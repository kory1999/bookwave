using System;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Global;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Library
{
    public class LibraryFilterButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _panelObject;
        [SerializeField] private LibraryFilterPanel _panel;
        [SerializeField] private Button _interceptTouch;

        private void Awake()
        {
            _interceptTouch.onClick.AddListener(() =>
            {
                VisibleFilterPanel(false);
            });
        }

        public void Setup()
        {
            _button.onClick.AddListener(() =>
            {
                GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_Library_ClickFilter);

                VisibleFilterPanel(!_panelObject.activeSelf);
            });
        }

        public void Refresh(List<LibraryViewHistory.FilterType> selectionTexts,Action<LibraryViewHistory.FilterType> onSelectionTapCallback)
        {
            _panel.Refresh(selectionTexts, type =>
            {
                VisibleFilterPanel(false);
                
                onSelectionTapCallback?.Invoke(type);
            });
            VisibleFilterPanel(false);
        }

        public void ToggleSelectionVisual(int index)
        {
            _panel.ToggleSelectionVisual(index);
        }

        public void ToggleVisual(bool enable)
        {
            _button.gameObject.SetActive(enable);
            if (!enable)
            {
                //_panel.gameObject.SetActive(false);
                VisibleFilterPanel(false);
            }
        }

        public void ResetByType(LibraryViewHistory.FilterType type)
        {
            _panel.ToggleSelectVisualByType(type);
        }

        public void VisibleFilterPanel(bool bVisible)
        {
            _panelObject.SetActive(bVisible);
            _interceptTouch.gameObject.SetActive((bVisible));
        }
    }
}