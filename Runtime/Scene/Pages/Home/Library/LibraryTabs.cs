using System;
using System.Collections.Generic;
using BW.Framework.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Library
{
    public class LibraryTabs : MonoBehaviour
    {
        [SerializeField] private GameObject[] buttonSelectedVisuals;
        [SerializeField] private GameObject[] buttonDeselectVisuals;
        [SerializeField] private Button[] buttons;
        [SerializeField] private Image[] textImages;
        [SerializeField] private RectTransform buttonParent;

        private List<int> _bookNumbers;
        private bool needRefreshLayout = false;
        public void Initialize(Action<int> onTabTap)
        {
            _bookNumbers = new List<int>(){0,0,0};
            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;
                buttons[i].onClick.AddListener(() =>
                {
                    onTabTap?.Invoke(index);
                });
            }
        }

        public void ToggleTo(int index)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                int depth = Mathf.Abs(index - i);
                buttonSelectedVisuals[i].GetComponent<CanvasGroup>().ToggleEnable(index == i);
                buttonSelectedVisuals[i].GetComponent<LayoutElement>().ignoreLayout = index != i;
                buttonDeselectVisuals[i].GetComponent<CanvasGroup>().ToggleEnable(index!= i);
                buttonDeselectVisuals[i].GetComponent<LayoutElement>().ignoreLayout = index == i;
                textImages[i].gameObject.SetActive(false);
            }
            
            textImages[index].gameObject.SetActive(_bookNumbers[index]!=0);
            needRefreshLayout = true;
        }

        public void SetBookNumberByIndex(int index, int number)
        {
            _bookNumbers[index] = number;
            if (_bookNumbers[index] == 0)
            {
                textImages[index].gameObject.SetActive(false);
            }
            textImages[index].GetComponentInChildren<TMP_Text>().text = number.ToString();
        }

        private void LateUpdate()
        {
            if (needRefreshLayout)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(buttonParent);
                needRefreshLayout = false;
            }
        }
    }
}