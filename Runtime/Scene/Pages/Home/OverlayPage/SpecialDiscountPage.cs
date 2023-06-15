using System;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage
{
    public class SpecialDiscountPage : OverlayUI
    {
        [SerializeField] private Text _originalPriceText;
        [SerializeField] private Text _discountPriceText;
        [SerializeField] private Button _subscribeButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _ternsAndCoonditionButton;
        [SerializeField] private Button _privacyPolicyButton;

        private Action _hideCallback;
        private const string _priceSignal = "[price]";
        private const string discount_sub_show = "discount_sub_show";
        private const string discount_sub_click = "discount_sub_click";
        private const string discount_sub_suc = "discount_sub_suc";

        public override void Initialize(object parameters)
        {
            base.Initialize(parameters);
            GlobalEvent.GetEvent<TrackingEvent>().Publish(discount_sub_show);
            if (parameters is Action)
            {
                _hideCallback = parameters as Action;
            }

            GlobalEvent.GetEvent<GetLocalizationEvent>().Publish(_discountPriceText.text, (text) =>
            {
                _discountPriceText.text = text;
                GlobalEvent.GetEvent<GetProductPriceEvent>().Publish(PurchaseManager.Product_Year_Discount,
                    (price) =>
                    {
                        _discountPriceText.text = _discountPriceText.text.Replace(_priceSignal, price);
                    });
            });
            
            GlobalEvent.GetEvent<GetLocalizationEvent>().Publish(_originalPriceText.text, (text) =>
            {
                _originalPriceText.text = text;
                GlobalEvent.GetEvent<GetProductPriceEvent>().Publish(PurchaseManager.Product_Year,
                    (price) =>
                    {
                        _originalPriceText.text = _originalPriceText.text.Replace(_priceSignal, price);
                    });
            });
            
            _subscribeButton.onClick.AddListener(HandleOnSubscribeButton);
            _closeButton.onClick.AddListener(HandleOnCloseButton);
            _ternsAndCoonditionButton.onClick.AddListener(HandleOnTermsAndConditionButton);
            _privacyPolicyButton.onClick.AddListener(HandleOnPrivacyPolicyButton);
        }

        public override void Hide(Action callback)
        {
            base.Hide(callback);
            callback?.Invoke();
            _hideCallback?.Invoke();
            Destroy(gameObject);
        }

        private void HandleOnSubscribeButton()
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(discount_sub_click);
            GlobalEvent.GetEvent<DoPurchaseEvent>().Publish(PurchaseManager.Product_Year_Discount,HandleOnPurchaseResult);
        }

        private void HandleOnPurchaseResult(bool success)
        {
            if (success)
            {
                GlobalEvent.GetEvent<TrackingEvent>().Publish(discount_sub_suc);
            }
        }

        private void HandleOnCloseButton()
        {
            OverlayPage.Instance.Hide<SpecialDiscountPage>();
        }

        private void HandleOnTermsAndConditionButton()
        {
            Application.OpenURL(BookwavesConstants.TermsOfServiceUrl);
        }

        private void HandleOnPrivacyPolicyButton()
        {
            Application.OpenURL(BookwavesConstants.PrivacyPolicyUrl);
        }
    }
}