using System;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage.Logic;
using BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage.AddWishBook;
using BeWild.Framework.Runtime.Analytics;
using BeWild.Framework.Runtime.Utils;
using BW.Framework.Utils;
using DP.Base;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class SearchPage : MonoBehaviour
    {
        [SerializeField] private HomeTopBar topBar;
        [SerializeField] private SearchPageCategoryBar categoryBar;
        [SerializeField] private RectTransform layout;
        [SerializeField] private BookListUI bookList;
        [SerializeField] private SearchPageDefaultPage defaultPage;
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject addBookPage, endMark;
        [SerializeField] private Button addBookButton;
        [SerializeField] private LoadHint loadingHint;
        [SerializeField] private SearchPageCategoryScroll searchPageCategory;
        [SerializeField] private float _categoryBarOnLayoutOffset = -405f;
        [SerializeField] private float _categoryBarOffLayoutOffset = -220f;

        private ISearchPageLogic _logic;

        private bool _initialized;
        private string _currentSearchValue;
        private int _currentCategoryId = -1;
        private int _currentCategoryIndex = -1;

        #region public
        
        public void Initialize()
        {
            if (!_initialized)
            {
                _initialized = true;

                DoInitialize();
            
                MainScene.Event.GetEvent<OpenSearchPageEvent>().Subscribe(OpenPage);
                MainScene.Event.GetEvent<SetSearchPageSortingOrderEvent>().Subscribe(SetSortingOrder);

                GlobalEvent.GetEvent<LanguageUpdateEvent>().Subscribe(HandleOnLanguageUpdate);
            }
        }

        public void ShowBookPage()
        {
            defaultPage.Hide();
            bookList.gameObject.SetActive(true);
        }

        public void ToggleLoadingHint(bool on)
        {
            loadingHint.ToggleVisual(on, bookList.IsEmpty());
        }

        public void AddBooks(BookListData data, bool checkIsEmpty = true)
        {
            bookList.AddBooks(data);

            if (checkIsEmpty)
            {
                ToggleAddBook(bookList.IsEmpty());
            }
        }

        public void ClearBooks()
        {
            bookList.ClearBooks();
        }

        public void ToggleEndMark(bool on)
        {
            endMark.SetActive(on);
        }

        public void ShowCategory(int id)
        {
            GlobalEvent.GetEvent<GetLocalizationEvent>()
                .Publish("Categories", result => { topBar.ShowTitleText(result); });

            ToggleCategoryBar(true);
            categoryBar.ShowCategory(id);
        }

        public void ShowTitle(string title)
        {
            ToggleCategoryBar(false);
            topBar.ShowTitleText(title);
        }

        public void SetCurrentCategoryId(int id)
        {
            _currentCategoryId = id;
        }

        #endregion

        private void DoInitialize()
        {
            topBar.Initialize(HandleOnSearch, HandleOnBackButton);
            topBar.ToggleBackButton(true);

            bookList.Initialize(HandleOnBookTap, HandleOnScrollReachEnd);
            bookList.gameObject.SetActive(false);

            defaultPage.Initialize(HandleOnSearchTrendingTap, HandleOnBookTap);
            categoryBar.Initialize(HandleOnCategoryTap);

            addBookButton.onClick.AddListener(HandleOnAddBookButtonTap);

            ToggleLoadingHint(false);
            ToggleEndMark(false);

            searchPageCategory.Initialize(ScrollPage, IsEdgePage);
            
            ToggleVisual(false);
        }
        
        private void OpenPage(ISearchPageLogic logic, int sortOrder)
        {
            SetSortingOrder(sortOrder);
            
            ToggleVisual(true);
            
            _logic = logic;
            if (_logic != null)
            {
                _logic.SetSearchPage(this);
                _logic.StartLogic();
            }
            else
            {
                _logic = new SearchLogic();
                ShowDefaultPage();
            }
            
            GetComponent<DPSafeAreaScaler>().ForceUpdate();
        }

        private void SetSortingOrder(int sortOrder)
        {
            canvas.sortingOrder = sortOrder;
        }

        private void ShowDefaultPage()
        {
            defaultPage.Show();
            ToggleCategoryBar(false);
            bookList.gameObject.SetActive(false);
            topBar.ShowSearch();
            ToggleAddBook(false);
        }

        private void ToggleCategoryBar(bool on)
        {
            categoryBar.ToggleVisual(on);
            searchPageCategory.ToggleScroll(on);
            layout.offsetMax = new Vector2(0f, on ? _categoryBarOnLayoutOffset : _categoryBarOffLayoutOffset);
        }

        private void ToggleVisual(bool on)
        {
            canvasGroup.ToggleEnable(on);

            if (on)
            {
                MobileKeyboardManager.Instance.AddBackListener(HandleOnMobileBackButton, canvas.sortingOrder);
            }
            else
            {
                MobileKeyboardManager.Instance.RemoveBackListener(HandleOnMobileBackButton);
            }
        }

        #region event

        private void HandleOnLanguageUpdate()
        {
            defaultPage.SetForceUpdateFlag();
            categoryBar.SetForceUpdateFlag();
        }

        private void HandleOnAddBookButtonTap()
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_Home_SearchClickAddBook);

            OverlayPage.OverlayPage.Instance.Show<AddWishBookPage>();
        }

        private void HandleOnMobileBackButton()
        {
            HandleOnBackButton();
        }

        private void HandleOnSearch(string value)
        {
            if (_currentSearchValue != value)
            {
                _currentSearchValue = value;

                if (_logic != null)
                {
                    _logic.Stop();
                }

                _logic = new SearchLogic();
                _logic.Initialize(new BookListData(), value);
                _logic.SetSearchPage(this);
                _logic.StartLogic();
            }
        }

        private void HandleOnBackButton()
        {
            DoClose();
        }

        private void HandleOnScrollReachEnd()
        {
            _logic?.TryFetchMore();
        }

        private void HandleOnSearchTrendingTap(string value)
        {
            HandleOnSearch(value);
        }

        private void HandleOnBookTap(int id)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(_logic.GetBookTapTrackingEvent());

            if (_logic is CategoryLogic)
            {
                AdjustAnalytics.PublishEvent(AdjustAnalytics.ADEvent_Tag_ClickBook);
            }
            else if (_logic is SearchLogic)
            {
                AdjustAnalytics.PublishEvent(AdjustAnalytics.ADEvent_Search_ResultClickBook);    
            }
            
            MainScene.Event.GetEvent<OpenBookEvent>().Publish(id);
        }

        private void HandleOnCategoryTap(int categoryId)
        {
            if (_currentCategoryId != categoryId)
            {
                _currentCategoryId = categoryId;

                if (_logic != null)
                {
                    _logic.Stop();
                }

                _logic = new CategoryLogic();
                _logic.Initialize(new BookListData(), categoryId);
                _logic.SetSearchPage(this);
                _logic.StartLogic();
            }
        }

        private void ToggleAddBook(bool on)
        {
            addBookPage.SetActive(on);

            if (on)
            {
                GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_Home_SearchShowAddBook);
            }
        }

        #endregion

        private void DoClose()
        {
            _currentSearchValue = String.Empty;
            _currentCategoryId = -1;
            ToggleVisual(false);
            topBar.ShowSearch();
            topBar.ClearSearchContent();

            ToggleVisual(false);
        }

        private bool IsEdgePage(bool on)
        {
            int tmpIndex = 0;
            if (on)
            {
                _currentCategoryIndex = categoryBar.GetIndexByID(_currentCategoryId);
                tmpIndex = _currentCategoryIndex;
                return categoryBar.IsEdgeNumber(++tmpIndex);
            }
            else
            {
                _currentCategoryIndex = categoryBar.GetIndexByID(_currentCategoryId);
                tmpIndex = _currentCategoryIndex;
                return categoryBar.IsEdgeNumber(--tmpIndex);
            }
        }

        private void ScrollPage(bool on)
        {
            if (on)
            {
                bool value = categoryBar.IsEdgeNumber(++_currentCategoryIndex);
                if (!value)
                    HandleOnCategoryTap(categoryBar.GetIDByIndex(_currentCategoryIndex));
            }
            else
            {
                bool value = categoryBar.IsEdgeNumber(--_currentCategoryIndex);
                if (!value)
                    HandleOnCategoryTap(categoryBar.GetIDByIndex(_currentCategoryIndex));
            }
            
        }
    }
}