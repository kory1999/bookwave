using System.Collections.Generic;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Global;
using BeWild.Framework.Runtime.Utils.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Library
{
    public class LibraryPage : HomeView
    {
        [SerializeField] private LibraryView[] libraryViews;
        [SerializeField] private LibraryTabs tabs;

        [SerializeField] private VerticalScrollPageGroup _verticalScrollPageGroup;
        [SerializeField] private LibraryFilterButton _libraryFilterButton;
        [SerializeField] private Button searchButton;

        private int _currentViewIndex = -1;

        private readonly string[] _trackingNames =
        {
            BookwavesAnalytics.Event_Library_ClickHistoryTab,
            BookwavesAnalytics.Event_Library_ClickBookMarkTab,
            BookwavesAnalytics.Event_Library_ClickHighlightTab
        };

        public override void Initialize()
        {
            foreach (LibraryView view in libraryViews)
            {
                view.Initialize();
                view.NumberChannelEvent(BookNumberEvent);
            }

            tabs.Initialize(index => SwitchToPage(index, false));

            _verticalScrollPageGroup.Initialize(ScrollToPage);

            _libraryFilterButton.Setup();
            LibraryViewHistory libraryViewHistory = GetTargetView<LibraryViewHistory>();

            _libraryFilterButton.Refresh(new List<LibraryViewHistory.FilterType>
            {
                LibraryViewHistory.FilterType.All,
                LibraryViewHistory.FilterType.Finished,
                LibraryViewHistory.FilterType.Continue,
            }, libraryViewHistory.SetFilter);
            _libraryFilterButton.ToggleSelectionVisual(0);

            searchButton.onClick.AddListener(HandleOnSearchButton);
        }

        public override void ToggleVisual(bool on)
        {
            base.ToggleVisual(on);
            for (int i = 0; i < libraryViews.Length; i++)
            {
                libraryViews[i].ToggleVisual(on);
            }

            if (on)
            {
                SwitchToPage(_currentViewIndex == -1 ? 0 : _currentViewIndex, true);

                TryToShowRateUs();
            }
        }

        public override void ToggleInteract(bool on)
        {
            base.ToggleInteract(on);

            if (on && _currentViewIndex >= 0)
            {
                libraryViews[_currentViewIndex].RefreshUI();
            }
        }

        private void SwitchToPage(int index, bool forceUpdate)
        {
            if (_currentViewIndex == index && !forceUpdate)
            {
                return;
            }

            _libraryFilterButton.ResetByType(LibraryViewHistory.FilterType.All);

            GlobalEvent.GetEvent<TrackingEvent>().Publish(_trackingNames[index]);

            _currentViewIndex = index;

            _verticalScrollPageGroup.TurnToTargetPage(index, false);

            if (index == 0)
            {
                _libraryFilterButton.ToggleVisual(true);
            }
            else
            {
                _libraryFilterButton.ToggleVisual(false);
            }

            libraryViews[_currentViewIndex].RefreshUI();
        }

        private void ScrollToPage(int index)
        {
            tabs.ToggleTo(index);
            _currentViewIndex = index;
            if (index == 0)
            {
                _libraryFilterButton.ToggleVisual(true);
            }
            else
            {
                _libraryFilterButton.ToggleVisual(false);
            }

            libraryViews[_currentViewIndex].RefreshUI();
        }

        private void BookNumberEvent(int viewIndex, int bookNumber)
        {
            tabs.SetBookNumberByIndex(viewIndex / 10, bookNumber);
        }

        private T GetTargetView<T>() where T : LibraryView
        {
            for (int i = 0; i < libraryViews.Length; i++)
            {
                if (libraryViews[i] is T)
                {
                    return libraryViews[i] as T;
                }
            }

            return null;
        }

        private void HandleOnSearchButton()
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_Library_ClickSearch);

            MainScene.Event.GetEvent<OpenSearchPageEvent>().Publish(null, BookwavesConstants.BackButtonPriority_SearchPage);
        }

        private void TryToShowRateUs()
        {
            int count = PlayerPrefs.GetInt(PlayerPrefsHelper.Key_EnterLibraryCount, 0) + 1;

            if (count == 4)
            {
                BookwavesNativeUtility.TryWeeklyRateUs();
            }

            PlayerPrefs.SetInt(PlayerPrefsHelper.Key_EnterLibraryCount, count);
        }
    }
}