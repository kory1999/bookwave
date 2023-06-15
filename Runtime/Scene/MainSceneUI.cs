using System.Collections.Generic;
using BeWild.AIBook.Runtime.Manager;
using BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage;
using BW.Framework.Utils;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene
{
    public enum MainScenePage
    {
        Home,
        BookBrief,
        BookContent
    }
    
    public class MainSceneUI : MonoBehaviour
    {
        public const string LogHeader = nameof(MainSceneUI);
        
        [SerializeField] private List<PageBase> _pages;
        [SerializeField] private SearchPage _searchPage;

        private List<int> _openedPageId = new List<int>();
        private bool _isPageChanging;
        private bool _isListeningToUnlockEvent;

        public void Initialize()
        {
            for (int i = 0; i < _pages.Count; i++)
            {
                _pages[i].TryToInitialize(i, HandleOnPageClose);
            }
            
            _searchPage.Initialize();

            GameManager.OnGameUnlockStateChanged += RefreshVIPState;
        }

        public void DoStart()
        {
            OpenPage(MainScenePage.Home);
        }

        public void OpenPage(MainScenePage page)
        {
            int pageIndex = (int)page;
            
            if (_isPageChanging)
            {
                Log($"can't open {page} during another page changing.");
                
                return;
            }

            if (_openedPageId.Count > 0 && _openedPageId[_openedPageId.Count - 1] == pageIndex)
            {
                Log($"can't open {page} since it's already open and active.");

                _pages[pageIndex].RefreshPage(GameManager.IsGameUnlocked);
                
                return;
            }
            
            Log($"open {page}.");
            
            if (_openedPageId.Count > 0)
            {
                _pages[_openedPageId.Count-1].ToggleInteract(false);    // disable current active page
            }

            int indexInOpenPage = _openedPageId.IndexOf(pageIndex);
            if (indexInOpenPage >= 0)
            {
                _openedPageId.RemoveRange(indexInOpenPage+1, _openedPageId.Count - 1 - indexInOpenPage);    // remove pages after target page
            }
            else
            {
                _openedPageId.Add(pageIndex);   // add new page
            }

            _isPageChanging = true;
            _pages[pageIndex].TryToDisplayUI();
            _pages[pageIndex].ToggleVisual(true, () =>
            {
                _isPageChanging = false;
                _pages[pageIndex].ToggleInteract(true);
            });
            _pages[pageIndex].ToggleVIPState(GameManager.IsGameUnlocked);
        }

        private void HandleOnPageClose(int pageIndex)
        {
            int currentActivePageIndex = _openedPageId[_openedPageId.Count - 1];
            if (_isPageChanging || currentActivePageIndex != pageIndex)    // latest page isn't the one to close
            {
                return;
            }
            
            if (_openedPageId.Count > 0)
            {
                // close current active page
                _isPageChanging = true;
                _pages[currentActivePageIndex].ToggleInteract(false);
                _pages[currentActivePageIndex].ToggleVisual(false, () =>
                {
                    _isPageChanging = false;
                    
                    _openedPageId.RemoveAt(_openedPageId.Count-1);
                    
                    // try to active previous page
                    if (_openedPageId.Count > 0)
                    {
                        _pages[_openedPageId[_openedPageId.Count - 1]].ToggleInteract(true);
                        _pages[_openedPageId[_openedPageId.Count - 1]].ToggleVIPState(GameManager.IsGameUnlocked);
                    }
                });
            }
        }

        private void RefreshVIPState(bool unlock)
        {
            if (_openedPageId.Count > 0)
            {
                _pages[_openedPageId[_openedPageId.Count - 1]].ToggleVIPState(unlock);
            }
        }

        private void Log(string info)
        {
            BaseLogger.Log(LogHeader, info);
        }
    }
}