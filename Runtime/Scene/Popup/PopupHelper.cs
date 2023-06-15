using System;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Global;
using BW.Framework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Popup
{
    public class PopupHelper : MonoBehaviour
    {
        private struct ToastRequest
        {
            public string Text;
            public float Duration;

            public ToastRequest(string text, float duration)
            {
                Text = text;
                Duration = duration;
            }
        }
        
        [SerializeField] private GeneralPopup popupPrefab;
        [SerializeField] private GeneralToast toast;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image mask;
        
        private List<GeneralPopup> _popups = new List<GeneralPopup>();
        private Queue<ToastRequest> _toastRequests = new Queue<ToastRequest>();

        private int _index;

        public void Initialize()
        {
            popupPrefab.Close();
            
            GlobalEvent.GetEvent<ShowPopupEvent>().Subscribe(HandleOnRequiresToShowPopup);
            GlobalEvent.GetEvent<ClosePopupEvent>().Subscribe(HandleOnRequiresToClosePopup);
            GlobalEvent.GetEvent<ShowToastEvent>().Subscribe(HandleOnRequiresToShowToast);
            
            UpdateCanvasGroup();
            
            toast.SetAlpha(0f);
        }

        private void HandleOnRequiresToShowPopup(PopupConfigurations configs, Action<int> callback)
        {
            int myIndex = _index;
            configs.ButtonCallback += success =>
            {
                HandleOnRequiresToClosePopup(myIndex);
            };
            
            GeneralPopup newPrefab = Instantiate(popupPrefab.gameObject, popupPrefab.transform.parent)
                .GetComponent<GeneralPopup>();
            _popups.Add(newPrefab);
            _index++;
            newPrefab.Initialize(myIndex);
            newPrefab.Show(configs);
            
            UpdateCanvasGroup();
            
            callback?.Invoke(myIndex);
        }

        private void HandleOnRequiresToClosePopup(int id)
        {
            for (int i = 0; i < _popups.Count; i++)
            {
                if (_popups[i].Id == id)
                {
                    _popups[i].Close();
                    
                    Destroy(_popups[i].gameObject);
                    
                    _popups.RemoveAt(i);
                }
            }

            UpdateCanvasGroup();
        }

        private void HandleOnRequiresToShowToast(string text, float duration)
        {
            _toastRequests.Enqueue(new ToastRequest(text, duration));
            
            if (!toast.IsShowing())
            {
                UpdateCanvasGroup();

                TryToStartNextToastRequest();
            }
        }

        private void TryToStartNextToastRequest()
        {
            if (_toastRequests.Count > 0)
            {
                ToastRequest request = _toastRequests.Peek();
                toast.Show(request.Text, request.Duration, () =>
                {
                    _toastRequests.Dequeue();

                    TryToStartNextToastRequest();
                });
            }
            else
            {
                UpdateCanvasGroup();
            }
        }

        private void UpdateCanvasGroup()
        {
            bool enableMask = _popups.Count > 0;
            bool toEnable = enableMask || _toastRequests.Count > 0;
            if (canvasGroup.interactable != toEnable)
            {
                canvasGroup.ToggleEnable(toEnable);
            }
            
            mask.color = Color.black * (enableMask ? 0.7f : 0f);
        }
    }
}