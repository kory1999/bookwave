using System;
using BeWild.AIBook.Runtime.Global;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Library
{
    public class LibraryFilterPanelSelection : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _selectionText;
        [SerializeField] private Button _button;

        [SerializeField] private Color selectColor;
        [SerializeField] private Color unSelectColor;
        [SerializeField] private Color textSelectColor;
        [SerializeField] private Color textUnSelectColor;

        private int _index;
        private LibraryViewHistory.FilterType _type;
        public LibraryViewHistory.FilterType FType
        {
            get { return _type; }
        }
        public void Setup(Action<int,LibraryViewHistory.FilterType> tapCallback)
        {
            _button.onClick.AddListener(() =>
            {
                tapCallback?.Invoke(_index,_type);
            });
        }

        public void Refresh(int index,LibraryViewHistory.FilterType type)
        {
            _index = index;
            _type = type;
        }

        public void ToggleEnable(bool enable)
        {
            _button.GetComponent<Image>().color = enable ? selectColor : unSelectColor;
            _selectionText.color = enable ? textSelectColor : textUnSelectColor;
        }
    }
}