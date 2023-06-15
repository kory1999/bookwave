using System;
using System.Collections;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BW.Framework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class SearchPageCategoryBar : MonoBehaviour
    {
        [SerializeField] private HomePageCategoryColor prefab;
        [SerializeField] private ScrollRect scroller;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroup;

        private List<HomePageCategoryColor> _categories;

        private Action<int> _tapCallback;
        private bool _forceUpdateMark;
        private CanvasGroup _canvasGroup;

        public void Initialize(Action<int> tapCallback)
        {
            _tapCallback = tapCallback;
            
            prefab.gameObject.SetActive(false);
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void ToggleVisual(bool on)
        {
            _canvasGroup.ToggleEnable(on);
        }

        public void SetForceUpdateFlag()
        {
            _forceUpdateMark = true;
        }

        public void ShowCategory(int id)
        {
            if (_categories == null || _forceUpdateMark)
            {
                _forceUpdateMark = false;
                
                GlobalEvent.GetEvent<GetHomePageDataEvent>().Publish(false, data =>
                {
                    if (data != null)
                    {
                        UpdateVisual(id, data.GetCategoryList(GameManager.Language));
                    }
                });
            }
            else
            {
                FocusOn(id);
            }
        }

        public int GetIDByIndex(int index)
        {
            return _categories[index].ID;
        }

        public int GetIndexByID(int id)
        {
            for (int i = 0; i < _categories.Count; i++)
            {
                if (_categories[i].ID == id)
                    return i;
            }

            return -1;
        }

        public bool IsEdgeNumber(int index)
        {
            if (index < 0 || index >= _categories.Count)
                return true;
            return false;
        }
        
       

        private void UpdateVisual(int focusId, List<CategoryData> data)
        {
            _categories ??= new List<HomePageCategoryColor>();
            
            for (int i = _categories.Count; i > data.Count; i--)
            {
                Destroy(_categories[i - 1].gameObject);
                _categories.RemoveAt(i - 1);
            }
            
            for (int i = 0; i < data.Count; i++)
            {
                if (_categories.Count - 1 < i)
                {
                    HomePageCategoryColor category = Instantiate(prefab.gameObject, prefab.transform.parent)
                        .GetComponent<HomePageCategoryColor>();
                    category.gameObject.SetActive(true);
                    _categories.Add(category);
                }

                _categories[i].Initialize(data[i], HandleOnCategoryTap);
            }

            StartCoroutine(DelayFocusOnId(focusId));
        }

        private IEnumerator DelayFocusOnId(int id)
        {
            yield return null;

            float width = 50f;

            foreach (HomePageCategoryColor c in _categories)
            {
                width += c.GetComponent<RectTransform>().rect.width + 20;
            }
            
            scroller.content.sizeDelta = new Vector2(width, scroller.content.rect.height);

            yield return null;

            FocusOn(id);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_horizontalLayoutGroup.GetComponent<RectTransform>());
        }

        private void FocusOn(int id)
        {
            float totalWidth = scroller.content.rect.width;
            float screenWidth = scroller.GetComponent<RectTransform>().rect.width;
            if (totalWidth > screenWidth)
            {
                HomePageCategoryColor category = _categories.Find(c => c.ID == id);
                if (category != null)
                {
                    RectTransform rect = category.GetComponent<RectTransform>();
                    float x = rect.localPosition.x + rect.rect.width / 2f;
                    float percentage = (x - screenWidth / 2f) / (totalWidth - screenWidth);
                    scroller.normalizedPosition = new Vector2(percentage, 1f);
                }
            }

            foreach (HomePageCategoryColor categoryColor in _categories)
            {
                categoryColor.ToggleActiveState(categoryColor.ID == id);
            }
        }

        private void HandleOnCategoryTap(int id)
        {
            _tapCallback?.Invoke(id);
        }
    }
}