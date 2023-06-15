using System;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage.UserGuide
{
    public class CategoryPage : OverlayUI
    {
        public class CategoryPageParams
        {
            public int categoryID;
            public Action BookTapCallback;
        }
        
        [SerializeField] private BookListUI _bookListUI;
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _categoryText;
        [SerializeField] private RawImageHolder _categoryIcon;

        private CategoryData categoryData;
        private BookListData _data;

        public override void Initialize(object parameters)
        {
            base.Initialize(parameters);
            
            _backButton.onClick.AddListener(()=>
            {
                OverlayPage.Instance.Hide<CategoryPage>();
            });
            
            if (parameters is CategoryPageParams categoryPageParams)
            {
                _data = new BookListData
                {
                    books = new List<BookBriefData>(),
                    currentPageIndex = 0
                };
                _bookListUI.Initialize((id) =>
                {
                    MainScene.Event.GetEvent<OpenBookEvent>().Publish(id);
                    categoryPageParams.BookTapCallback?.Invoke();
                    OverlayPage.Instance.Hide<CategoryPage>();
                },HandleOnListReachEnd);
                GlobalEvent.GetEvent<GetHomePageDataEvent>().Publish(false, homePageData =>
                {
                    if (homePageData != null)
                    {
                        categoryData = homePageData.FindCategory(categoryPageParams.categoryID);

                        if (categoryData != null)
                        {
                            HandleOnListReachEnd();
                        
                            _categoryText.text = categoryData.name;
                            _categoryIcon.SetTexture(categoryData.iconUrl);
                        }
                    }
                });
            }
        }
        
        private void HandleOnListReachEnd()
        {
            GlobalEvent.GetEvent<GetBooksInCategoryDataEvent>().Publish(categoryData.id, _data.currentPageIndex+1, data =>
            {
                _data.books.AddRange(data.books);
                _data.totalCount = data.totalCount;
                _data.currentPageIndex +=1;
                
                _bookListUI.AddBooks(new BookListData()
                {
                    books = data.books,
                    totalCount = data.totalCount,
                    currentPageIndex = _data.currentPageIndex
                });
            });
        }

        public override void Hide(Action callback)
        {
            base.Hide(callback);
            callback?.Invoke();
        }
    }
}
