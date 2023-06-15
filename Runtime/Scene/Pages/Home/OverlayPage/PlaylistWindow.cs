using System;
using System.Collections;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Scene;
using BeWild.AIBook.Runtime.Scene.Pages.BookBrief;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.FunLearning.Runtime.Scene.Pages.Home.OverlayPage
{
    public class PlaylistWindow : MonoBehaviour
    {

        [SerializeField] private Image _backgroundImage;

        [SerializeField] private Transform _window;
        [SerializeField] private Button _allButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Button _closeButton;

        private Action<int> _onItemTapEvent;
        private List<int> _deleteList = new List<int>();
        private bool _isShow = false;


        public void Initialize(Action<int> itemTapCallback)
        {
            _onItemTapEvent=itemTapCallback;
            _deleteList = new List<int>();
            _isShow = false;
            _backgroundImage.gameObject.SetActive(false);
            _allButton.onClick.AddListener(HandleOnAllButtonTap);
            _deleteButton.onClick.AddListener(HandleOnDeleteButtonTap);
            _closeButton.onClick.AddListener(HandleOnCloseButtonTap);

            // _listView.Initialize(HandleOnItemDeleteTap);
        }

        public void Refresh(List<BookBriefPageUI.BookBriefPageData> briefPageDatas)
        {
            // _listView.Refresh();
            //循环链表数据注入
        }

        public void SetCurrentItemPage(int value)
        {
            // _listView.SetCurrentItemPage(value);
        }

        public void ToggleOnVisual(bool on)
        {
            if(on==_isShow)
                return;
            _isShow = on;
            
            _backgroundImage.gameObject.SetActive(on);
            if (on)
            {
                _window.DOLocalMoveY(_window.localPosition.y + _window.GetComponent<RectTransform>().sizeDelta.y, 0.5f);
            }
            else
            {
                _window.DOLocalMoveY(_window.localPosition.y - _window.GetComponent<RectTransform>().sizeDelta.y, 0.5f);
            }
            
        }
        
        private void HandleOnItemTap(int id)
        {
           
            //Play book sound
            
            //Jump to book content sound page
          
            
        }

        private void HandleOnItemDeleteTap(int id)
        {
            //将id添加到_deleteList
            _deleteList.Add(id);
        }

        private void HandleOnAllButtonTap()
        {
           //将所以的Item选中
        }

        private void HandleOnDeleteButtonTap()
        {
            //进入选择的页面
            //_listView.ToggleOnDelete(true)
            if (_deleteList.Count > 0)
            {
                //删除
            }
            //删除
            // _listView.DeleteItems(_deleteList);
            //根据_deleteList删除
        }

        private void HandleOnCloseButtonTap()
        {
            ToggleOnVisual(false);
            //关闭
        }
    }
}