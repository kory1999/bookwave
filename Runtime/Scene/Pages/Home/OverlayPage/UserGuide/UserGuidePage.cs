using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage;
using BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage.UserGuide;
using BW.Framework.Utils;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage
{
    public class UserGuidePage : OverlayUI
    {
        public class Data
        {
            public Action FinishCallback;
        }
        
        [SerializeField] private List<GameObject> _pages;
        [SerializeField] private Button _continueButton;
        [SerializeField] private List<float> _pageProgress;
        [SerializeField] private List<Image> _progressPoint;
        [SerializeField] private Image _progressBar;
        [SerializeField] private UserGuideFinishPage _finishPage;
        [SerializeField] private List<DailyStudyTimeSelection> _dailyStudyTimeSelections;
        [SerializeField] private ContentSizeFitter _contentSizeFitter;
        [SerializeField] private HomePageCategoryGroup _categoryGroup;

        private Data _data;
        private int _currentPage = 0;
        private Tweener _tweener;
        private List<InterestSelection> _interestSelections;

        public override void Initialize(object parameters)
        {
            base.Initialize(parameters);
            
            _data = (Data)parameters;
            
            _continueButton.onClick.AddListener(HandleOnContinueTap);
            _dailyStudyTimeSelections.ForEach(selection => { selection.Initialize(HandleOnDailyTimeSelectionTap); });

            _interestSelections = GetComponentsInChildren<InterestSelection>().ToList();
            for (int i = 0; i < _interestSelections.Count; i++)
            {
                int index = i+1;
                _interestSelections[i].AddCallback((state) =>
                {
                    HandleOnCategoryTap(index);
                });
            }
            
            _contentSizeFitter?.Refresh(this,false,null,false);
        }

        public override void Hide(Action callback)
        {
            base.Hide(callback);
            callback?.Invoke();
        }

        private IEnumerator DelaySetCategories(List<HomePageCategory> categories)
        {
            yield return new WaitForSeconds(1f);
            
            _categoryGroup.SetCategories(categories);
        }

        private void HandleOnContinueTap()
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Prefix_Tutorial + (_currentPage+1) + BookwavesAnalytics.Suffix_Tutorial_ClickContinue);

            Flip();
        }

        private void ToggleAllPages(bool enable)
        {
            _pages.ForEach(page => { page.GetComponent<CanvasGroup>().ToggleEnable(enable); });
        }

        private CategoryData GetCategoryDataByName(List<CategoryData> categoryDatas, string name)
        {
            return categoryDatas.Find(data => { return data.name == name; });
        }

        private void TogglePage(int pageIndex, bool enable)
        {
            _pages[pageIndex].GetComponent<CanvasGroup>().ToggleEnable(enable);
        }

        private void Flip()
        {
            if (_currentPage < _pages.Count - 1)
            {
                ToggleAllPages(false);
                TogglePage(++_currentPage, true);
                
                _tweener?.Kill();
                _tweener = DOTween.To(() => { return _progressBar.fillAmount; },
                    (newValue) => { _progressBar.fillAmount = newValue; }, _pageProgress[_currentPage], 0.1f).OnComplete(
                    () =>
                    {
                        for (int i = 0; i <= _currentPage; i++)
                        {
                            _progressPoint[i].gameObject.SetActive(true);
                        }
                    });
            }
            else
            {
                ToggleAllPages(false);
                _finishPage.GetComponent<CanvasGroup>().ToggleEnable(true);
                _finishPage.DoFill(() =>
                {
                    OverlayPage.Instance.Hide<UserGuidePage>(() =>
                        {
                            _data.FinishCallback?.Invoke();
                        });
                });
            }
        }

        private void HandleOnDailyTimeSelectionTap(DailyStudyTimeSelection selection)
        {
            int number = _dailyStudyTimeSelections.IndexOf(selection) + 1;
            GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Prefix_Tutorial_ClickNumber + number);

            _dailyStudyTimeSelections.ForEach(m_selection =>
            {
                if (m_selection == selection)
                {
                    m_selection.SetSelected(true);
                }
                else
                {
                    m_selection.SetSelected(false);
                }
            });
        }

        private void HandleOnCategoryTap(int categoryId)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Prefix_Tutorial_ClickCategory + categoryId);
        }

        private void OnDestroy()
        {
            _tweener?.Kill();
        }
    }
}