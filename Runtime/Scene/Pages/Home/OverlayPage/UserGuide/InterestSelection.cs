using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage.UserGuide
{
    public class InterestSelection : MonoBehaviour
    {
        
        [SerializeField] private Color _bgUnselectColor;
        [SerializeField] private Color _bgSelectedColor;
        [SerializeField] private Image _image;
        [SerializeField] private Button _button;

        private bool selected = false;
        private Action<bool> _callback;

        public void AddCallback(Action<bool> newCallback)
        {
            _callback += newCallback;
        }

        public void ToggleSelect(bool toSelect)
        {
            selected = toSelect;
            _image.color = toSelect ? _bgSelectedColor : _bgUnselectColor;
        }

        private void Awake()
        {
            _button.onClick.AddListener(() =>
            {
                ToggleSelect(!selected);
                _callback?.Invoke(selected);
            });
        }
    }
}
