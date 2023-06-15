using System;
using BeWild.AIBook.Runtime.Global;
using BeWild.Framework.Runtime.Utils;
using BW.Framework.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Popup
{
    public class GeneralPopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text title, text, buttonText;
        [SerializeField] private Button confirmButton, cancelButton;
        [SerializeField] private GameObject gift;

        public int Id { get; private set; }
        
        private Action<bool> _closeCallback;

        public void Initialize(int id)
        {
            Id = id;
            
            confirmButton.onClick.AddListener(HandleOnConfirmButton);
            cancelButton.onClick.AddListener(HandleOnCancelButton);
        }
        
        public void Show(PopupConfigurations configs)
        {
            title.text = configs.Title;
            text.text = configs.Text;
            buttonText.text = configs.ButtonText;
            cancelButton.gameObject.SetActive(configs.ShowCloseButton);
            gift.gameObject.SetActive(configs.ShowGift);

            _closeCallback = configs.ButtonCallback;
            
            ToggleVisual(true);
        }

        public void Close()
        {
            ToggleVisual(false);
        }

        private void ToggleVisual(bool on)
        {
            GetComponent<CanvasGroup>().ToggleEnable(on);
            
            if (on)
            {
                MobileKeyboardManager.Instance.AddBackListener(HandleOnCancelButton, BookwavesConstants.BackButtonPriority_GameStore + 1);
            }
            else
            {
                MobileKeyboardManager.Instance.RemoveBackListener(HandleOnCancelButton);
            }
        }

        private void HandleOnConfirmButton()
        {
            _closeCallback?.Invoke(true);

            Close();
        }

        private void HandleOnCancelButton()
        {
            _closeCallback?.Invoke(false);

            Close();
        }
    }
}