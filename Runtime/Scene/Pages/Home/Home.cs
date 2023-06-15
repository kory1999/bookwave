using System;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Global;
using BW.Framework.Utils;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home
{
    public class Home : PageBase
    {
        [SerializeField] private OverlayUI overlayUI;
        [SerializeField] private HomeView[] views;

        private HomeView _currentView;

        private readonly string[] _trackingNames =
        {
            BookwavesAnalytics.Event_Home_ClickForYouTab,
            BookwavesAnalytics.Event_Home_ClickTodayTab,
            BookwavesAnalytics.Event_Home_ClickLibraryTab,
            BookwavesAnalytics.Event_Home_ClickProfileTab
        };

        protected override void Initialize()
        {
            ToggleCanvasGroup(false);
            
            overlayUI.Initialize(ToggleToView);

            foreach (HomeView view in views)
            {
                view.Initialize();
            }
        }

        protected override void DisplayUI()
        {
            ToggleCanvasGroup(true);
            
            overlayUI.ToggleTo(0);
            
            _currentView = views[0];
        }

        protected override void DoToggleVisual(bool on, Action finishCallback)
        {
            _currentView.ToggleVisual(on);
            
            finishCallback?.Invoke();
        }

        protected override void DoToggleInteract(bool on)
        {
            _currentView.ToggleInteract(on);
            
            GetComponent<CanvasGroup>().ToggleInteract(on);
        }

        private void ToggleToView(int index)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(_trackingNames[index]);

            overlayUI.ToggleTo(index);

            if (_currentView != null)
            {
                _currentView.ToggleVisual(false);
            }

            _currentView = views[index];

            _currentView.ToggleVisual(true);
        }

        private void ToggleCanvasGroup(bool on)
        {
            GetComponent<CanvasGroup>().ToggleEnable(on);
        }
    }
}