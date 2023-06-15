using System;
using BeWild.Framework.Runtime.Utils.UI;
using BW.Framework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.AudioPlayer
{
    public class ChapterSelectButton : MonoBehaviour
    {
        public string ButtonName
        {
            get => _button.ButtonName;
            set => _button.ButtonName = value;
        }
        
        [SerializeField] private ButtonWithName _button;
        [SerializeField] private CanvasGroup _lockVisual;
        private Action<ButtonWithName> _onClickCallback;

        public void Setup(Action<ButtonWithName> clickCallback)
        {
            _onClickCallback = clickCallback;
            _button.OnClick += HandleOnTap;
        }

        public void ToggleLockVisual(bool locked)
        {
            _lockVisual.ToggleEnable(locked);
        }

        public void ToggleInteract(bool enable)
        {
            _button.interactable = enable;
        }

        private void HandleOnTap(ButtonWithName button)
        {
            _onClickCallback?.Invoke(button);
        }

        public void SetLockColor(Color color)
        {
            Image i = _lockVisual.GetComponent<Image>();
            if (i != null)
            {
                i.color = color;
            }
        }
    }
}