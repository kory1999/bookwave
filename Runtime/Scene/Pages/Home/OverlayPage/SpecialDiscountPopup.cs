using System;
using BeWild.AIBook.Runtime.Global;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage
{
    public class SpecialDiscountPopup : OverlayUI
    {
        [SerializeField] private Button _showButton;
        [SerializeField] private Button _closeButton;

        private Action _hideCallback;
        private const string gift_discount_show = "gift_discount_show";
        private const string gift_discount_click = "gift_discount_click";
        
        public override void Initialize(object parameters)
        {
            base.Initialize(parameters);
            GlobalEvent.GetEvent<TrackingEvent>().Publish(gift_discount_show);
            if (parameters is Action)
            {
                _hideCallback = parameters as Action;
            }
            _showButton.onClick.AddListener(() =>
            {
                GlobalEvent.GetEvent<TrackingEvent>().Publish(gift_discount_click);
                OverlayPage.Instance.Hide<SpecialDiscountPopup>(() =>
                {
                    OverlayPage.Instance.Show<SpecialDiscountPage>(_hideCallback);
                });
            });
            
            _closeButton.onClick.AddListener(() =>
            {
                OverlayPage.Instance.Hide<SpecialDiscountPopup>(_hideCallback);
            });
        }

        public override void Hide(Action callback)
        {
            base.Hide(callback);
            callback?.Invoke();
            Destroy(gameObject);
        }
    }
}
