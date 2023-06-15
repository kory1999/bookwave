using System;
using BW.Framework.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookBrief
{
    public class SelectButton : MonoBehaviour
    {
        [FormerlySerializedAs("_notCollectVisual")] [SerializeField] private CanvasGroup _notSelectVisual;
        [FormerlySerializedAs("_collectedVisual")] [SerializeField] private CanvasGroup _selectedVisual;
        [SerializeField] private Button _button;

        public void Setup(Action tapCallback)
        {
            _button.onClick.AddListener(() =>
            {
                tapCallback?.Invoke();
            });
        }

        public void SetVisual(bool collect)
        {
            _notSelectVisual.ToggleEnable(!collect);
            _selectedVisual.ToggleEnable(collect);
        }
        
        public void ToggleOnButton(bool on)
        {
            _button.interactable = on;
        }
    }
}