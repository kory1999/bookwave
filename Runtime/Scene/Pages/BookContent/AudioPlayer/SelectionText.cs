using System;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Global;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.AudioPlayer
{
    public class SelectionText : MonoBehaviour
    {
        public string CurrentText => _text.text;
        public float CurrentSpeed => _speeds[_currentSelectionIndex];
        
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private List<string> _selections;
        [SerializeField] private List<float> _speeds;

        private int _currentSelectionIndex = 0;
        private Action<string, string> _onTapCallback;

        public void Setup(Action<string, string> onTapCallback)
        {
            _onTapCallback = onTapCallback;
            
            _button.onClick.AddListener(HandleOnButtonTap);
            _text.text = _selections[0];
            _text.GetComponent<LocalizationTextLoader>().TryToRefreshText();
        }

        private void HandleOnButtonTap()
        {
            string oldSelection = _selections[_currentSelectionIndex];
            ToNextSelection();
            string newSelection = _selections[_currentSelectionIndex];
            _text.text = newSelection;
            _text.GetComponent<LocalizationTextLoader>().TryToRefreshText();
            _onTapCallback?.Invoke(oldSelection,newSelection);
        }

        private void ToNextSelection()
        {
            if (_currentSelectionIndex >= _selections.Count - 1)
            {
                _currentSelectionIndex = 0;
            }
            else
            {
                _currentSelectionIndex++;
            }
        }
    }
}