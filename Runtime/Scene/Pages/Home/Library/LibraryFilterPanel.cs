using System;
using System.Collections.Generic;
using BW.Framework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Library
{
    public class LibraryFilterPanel : MonoBehaviour
    {
        [SerializeField] private LibraryFilterPanelSelection _selectionPrefab;
        [SerializeField] private Transform _selectionParent;
        [SerializeField] private ContentSizeFitter _contentSizeFitter;

        [SerializeField] private List<LibraryFilterPanelSelection> _selections;
        private int _currentSelectionIndex= -1;
        private Action<LibraryViewHistory.FilterType> _onSelectionTapCallback;

        public void Refresh(List<LibraryViewHistory.FilterType> selectionTexts,Action<LibraryViewHistory.FilterType> onSelectionTapCallback)
        {
            _onSelectionTapCallback = onSelectionTapCallback;
            // if (_selections == null)
            // {
            //     _selections = new List<LibraryFilterPanelSelection>();
            // }
            
            
            
            // if (_selections.Count < selectionTexts.Count)
            // {
            //     for (int i = _selections.Count; i < selectionTexts.Count; i++)
            //     {
            //         // LibraryFilterPanelSelection newSelection = Instantiate(_selectionPrefab.gameObject,_selectionParent)
            //         //     .GetComponent<LibraryFilterPanelSelection>();
            //         // newSelection.transform.localScale = Vector3.one;
            //         // newSelection.Setup(HandleOnSelectionTap);
            //         // _selections.Add(newSelection);
            //     }
            // }
            // else if (_selections.Count > selectionTexts.Count)
            // {
            //     for (int i = _selections.Count - 1; i >= selectionTexts.Count; i--)
            //     {
            //         Destroy(_selections[i].gameObject);
            //     }
            // }
            for (int i = 0; i < _selections.Count; i++)
            {
                _selections[i].Setup(HandleOnSelectionTap);
                if (i > selectionTexts.Count)
                {
                    _selections[i].gameObject.SetActive(false);
                }
                else
                {
                    _selections[i].Refresh(i,selectionTexts[i]);
                }
                
            }

            _contentSizeFitter.Refresh(this,true,null,false);
        }

        void EnableToggle(int idx)
        {
            bool ok = false;
            for (int i = 0; i < _selections.Count; i++)
            {
                if (idx == i)
                {
                    ok = true;
                }
                else
                {
                    _selections[i].ToggleEnable(false);   
                }
            }

            if (ok)
            {
                _selections[idx].ToggleEnable(true);
            }
        }

        public void ToggleSelectionVisual(int index)
        {
            if (_currentSelectionIndex != index)
            {
                ToggleAll(false);
                _selections[index].ToggleEnable(true);
                _currentSelectionIndex = index;
            }
        }

        public void ToggleSelectVisualByType(LibraryViewHistory.FilterType type)
        {
            ToggleAll(false);
            for (int i = 0; i < _selections.Count; i++)
            {
                if (_selections[i].FType == type)
                {
                    _selections[i].ToggleEnable(true);
                    _currentSelectionIndex = i;
                }
            }
        }

        private void HandleOnSelectionTap(int index,LibraryViewHistory.FilterType type)
        {
            if (_currentSelectionIndex != index)
            {
                ToggleAll(false);
                _selections[index].ToggleEnable(true);
                _onSelectionTapCallback?.Invoke(type);
                _currentSelectionIndex = index;
            }
            // gameObject.SetActive(false);
        }

        private void ToggleAll(bool enable)
        {
            for (int i = 0; i < _selections.Count; i++)
            {
                _selections[i].ToggleEnable(enable);
            }
        }

        private void OnEnable()
        {
            _contentSizeFitter.Refresh(this,true,null,false);
        }
    }
}