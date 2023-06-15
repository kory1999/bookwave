using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BW.Framework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Today
{
    public class TodayTabGroup : MonoBehaviour
    {
        [SerializeField] private TodayTabButton tabButtonPrefab;
        [SerializeField] private Transform buttonParent;
        [SerializeField] private ScrollRect scrollRect;
        private List<TodayTabButton> _tabButtonList;

        private Action<int> _buttonClickEvent;
        private bool _isFirst;

        public void Initialize(Action<int> callback)
        {
            _tabButtonList = new List<TodayTabButton>();
            _buttonClickEvent = callback;
            _isFirst = true;
        }

        //
        public virtual void RefreshButton(List<string> _buttonName)
        {
            for (int i = 0; i < _tabButtonList.Count; i++)
            {
                _tabButtonList[i].Refresh(i, _buttonName[i]);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(buttonParent.GetComponent<RectTransform>());
        }

        public void Clear()
        {
            foreach (TodayTabButton button in _tabButtonList)
            {
                Destroy(button.gameObject);
            }
            
            _tabButtonList.Clear();
        }

        public void InitButton(int number)
        {
            if (number > _tabButtonList.Count)
            {
                int index = number-_tabButtonList.Count;
                for (int i=0;i<index;i++)
                {
                    TodayTabButton tabButton = Instantiate(tabButtonPrefab, buttonParent, false);
                    tabButton.Initialize(HandleOnTabButtonTap);
                    _tabButtonList.Add(tabButton);
                }
            }
            else
            {
                int index = _tabButtonList.Count - number;
                for (int i = 0; i < index; i++)
                {
                    Destroy(_tabButtonList[_tabButtonList.Count-1].gameObject);
                    _tabButtonList.RemoveAt(_tabButtonList.Count - 1);
                }
            }
            _tabButtonList[0].Selected();
        }

        public void SelectTabByIndex(int index)
        {
            for (int i = 0; i < _tabButtonList.Count; i++)
            {
                _tabButtonList[i].CancelSelected();
            }
            _tabButtonList[index].Selected();
            if (!_isFirst)
            {
                scrollRect.Navigate(_tabButtonList[index].GetComponent<RectTransform>(), 0f, null);
            }
            else
            {
                _isFirst = false;
            }
                 
        }
        
        private void HandleOnTabButtonTap(int index)
        {
            for (int i = 0; i < _tabButtonList.Count; i++)
            {
                _tabButtonList[i].CancelSelected();
            }

            _buttonClickEvent.Invoke(index);
        }
    }
}