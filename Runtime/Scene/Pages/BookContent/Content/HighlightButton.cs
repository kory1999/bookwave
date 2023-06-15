using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.Content
{
    public class HighlightButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Sprite _darkBackground;
        [SerializeField] private Sprite _whiteBackground;
        [SerializeField] private Color _darkTextColor;
        [SerializeField] private Color _whiteTextColor;

        public void Setup(Action tapCallback)
        {
            _button.onClick.AddListener(()=>{tapCallback?.Invoke();});
        }

        public void ChangeMode(bool darkMode)
        {
            _background.sprite = darkMode ? _darkBackground : _whiteBackground;
            _text.color = darkMode ? _darkTextColor : _whiteTextColor;
        }
    }
}