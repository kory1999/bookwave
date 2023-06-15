using System;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BW.Framework.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public abstract class HomePageCategoryBase : MonoBehaviour
    {
        [SerializeField] private HomePageCategory categoryPrefab;

        protected List<HomePageCategory> Categories;
        private Action<int> _tapCallback;

        public void Initialize(Action<int> callback)
        {
            _tapCallback = callback;

            categoryPrefab.gameObject.SetActive(false);
            
            GlobalEvent.GetEvent<LanguageUpdateEvent>().Subscribe(HandleOnLanguageUpdate);
        }

        public void ShowCategory([NotNull] List<CategoryData> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (Categories == null)
            {
                Categories = new List<HomePageCategory>();

                foreach (CategoryData categoryData in data)
                {
                    GameObject newInstance = Instantiate(categoryPrefab.gameObject, categoryPrefab.transform.parent);
                    newInstance.SetActive(true);
                    HomePageCategory newCategory = newInstance.GetComponent<HomePageCategory>();
                    newCategory.Initialize(categoryData, (ID) => { _tapCallback?.Invoke(ID); });
                    Categories.Add(newCategory);
                }

                DelayInvoker.Instance.Invoke(this, Refresh, Time.deltaTime);
            }
            else
            {
                int len = Categories.Count;
                int c = data.Count;
                int a = c < len ? c : len;
                int i = 0;
                for (; i < a; i++)
                {
                    Categories[i].Initialize(data[i], (ID) => { _tapCallback?.Invoke(ID); });                    
                }

                if (len < c)
                {
                    for (; i < c; i++)
                    {
                        GameObject newInstance = Instantiate(categoryPrefab.gameObject, categoryPrefab.transform.parent);
                        newInstance.SetActive(true);
                        HomePageCategory newCategory = newInstance.GetComponent<HomePageCategory>();
                        newCategory.Initialize(data[i], (ID) => { _tapCallback?.Invoke(ID); });
                        Categories.Add(newCategory);
                    }
                }
                else
                {
                    for (; i < len; i++)
                    {
                        Categories[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        private void HandleOnLanguageUpdate()
        {
            if (Categories != null)
            {
                foreach (var c in Categories)
                {
                    Destroy(c.gameObject);
                }

                Categories = null;
            }
        }

        protected abstract void Refresh();
    }
}