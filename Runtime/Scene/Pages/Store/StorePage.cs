using System;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage;
using BeWild.Framework.Runtime.Utils;
using BW.Framework.Utils;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Store
{
    public class StorePage : MonoBehaviour
    {
        public class Data
        {
            public string button1Id;
            public string button1Price;
            public string button2Id;
            public string button2Price;
        }

        public const string PriceTag = "[price]";

        public enum EventType
        {
            Close,
            Restore,
            Purchase,
            ShowPersuade
        }

        public bool IsPersuadePageShown => persuade.GetComponent<PersuadePage>().Shown;

        [SerializeField] private Button backButton, restoreButton, subscribeButton1, subscribeButton2;
        [SerializeField] private CanvasGroup store, persuade;
        [SerializeField] private Transform visual, visualInPosition, visualOutPosition;
        [SerializeField] private Image inputBlock;
        [SerializeField] private Text priceText1, priceText2;
        [SerializeField] private TMP_Text readme;
        [SerializeField] private string iOSReadmeText, androidReadmeText;

        private Action<EventType, object> _eventCallback;
        private Action _closeCallback;
        private Data _data;
        private bool _interactEnabled;
        private bool _isOpened;

        private string _price1;
        private string _price2;
        private string _priceID1;
        private string _priceID2;

        public void Initialize(Action<EventType, object> callback)
        {
            _eventCallback = callback;

            backButton.onClick.AddListener(HandleOnBackButton);
            restoreButton.onClick.AddListener(HandleOnRestoreButton);

            if (subscribeButton1 != null)
            {
                subscribeButton1.onClick.AddListener(() => DoSubscribeButtonLogic(GetProductIdByButtonIndex(0)));
                _priceID1 = priceText1.text;
            }

            if (subscribeButton2 != null)
            {
                subscribeButton2.onClick.AddListener(() => DoSubscribeButtonLogic(GetProductIdByButtonIndex(1)));
                _priceID2 = priceText2.text;
            }

            ToggleInputBlock(false);
            TogglePagesInteract(false);
            visual.transform.localPosition = visualOutPosition.localPosition;

            UpdateReadme();
        }

        public void SetData(Data data)
        {
            _data = data;

            UpdatePriceDisplay(data);
        }

        public void Show(Action callback)
        {
            _closeCallback = callback;

            _isOpened = true;

            TogglePage(false);

            ToggleInputBlock(true);

            visual.transform.DOKill();
            visual.transform.DOLocalMove(visualInPosition.localPosition, 0.5f).SetEase(Ease.InOutQuad).onComplete +=
                () => { store.ToggleInteract(true); };
        }

        public void Close(bool checkPersuadePage = true,Action<Action> hideCallback = null)
        {
            if (!_isOpened)
            {
                return;
            }

            if (checkPersuadePage && !GameManager.IsGameUnlocked && ShouldShowPersuadePage())
            {
                _eventCallback?.Invoke(EventType.ShowPersuade, null);

                PlayerPrefs.SetInt(PlayerPrefsHelper.Key_PersuadePage, 1);

                TogglePage(true);

                persuade.GetComponent<PersuadePage>()
                    .Show(_data.button2Price, () => DoSubscribeButtonLogic(_data.button2Id));
            }
            else
            {
                _isOpened = false;

                TogglePagesInteract(false);

                visual.transform.DOKill();
                visual.transform.DOLocalMove(visualOutPosition.localPosition, 0.5f).SetEase(Ease.InOutQuad)
                    .onComplete += () =>
                {
                    ToggleInputBlock(false);
                    if(hideCallback!=null)
                        hideCallback(_closeCallback);
                    else
                        _closeCallback?.Invoke();
                    Destroy(gameObject);
                };
            }
        }

        private void HandleOnBackButton()
        {
            if (_interactEnabled)
            {
                _eventCallback?.Invoke(EventType.Close, null);
            }
        }

        private void HandleOnRestoreButton()
        {
            if (_interactEnabled)
            {
                _eventCallback?.Invoke(EventType.Restore, null);
            }
        }

        private void DoSubscribeButtonLogic(string productId)
        {
            if (_interactEnabled)
            {
                _eventCallback?.Invoke(EventType.Purchase, productId);
            }
        }

        private string GetProductIdByButtonIndex(int index)
        {
            return index == 0 ? _data.button1Id : _data.button2Id;
        }

        private void TogglePagesInteract(bool on)
        {
            store.ToggleInteract(on);
            persuade.ToggleInteract(on);
        }

        private void ToggleInputBlock(bool on)
        {
            _interactEnabled = on;

            inputBlock.raycastTarget = on;

            if (on)
            {
                MobileKeyboardManager.Instance.AddBackListener(HandleOnBackButton,
                    BookwavesConstants.BackButtonPriority_GameStore);
            }
            else
            {
                MobileKeyboardManager.Instance.RemoveBackListener(HandleOnBackButton);
            }
        }

        private void UpdateReadme()
        {
            GlobalEvent.GetEvent<LanguageUpdateEvent>().Subscribe(RefreshReadMeText);
            RefreshReadMeText();
        }

        private void RefreshReadMeText()
        {
            if (GameManager.IsiOSPlatform)
            {
                GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("IOSGameStore", s => readme.text = s);
            }
            else
            {
                GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("AndroidGameStore", s => readme.text = s);
            }
        }

        private void UpdatePriceDisplay(Data data)
        {
            _price1 = data.button1Price;
            _price2 = data.button2Price;
            GlobalEvent.GetEvent<LanguageUpdateEvent>().Subscribe(RefreshPriceText);
            RefreshPriceText();
        }

        private void RefreshPriceText()
        {
            if (priceText1 != null)
            {
                GlobalEvent.GetEvent<GetLocalizationEvent>().Publish(_priceID1,
                    s => { priceText1.text = s.Replace(PriceTag, _price1); });
            }

            if (priceText2 != null)
            {
                GlobalEvent.GetEvent<GetLocalizationEvent>().Publish(_priceID2,
                    s => { priceText2.text = s.Replace(PriceTag, _price2); });
            }
        }

        private void TogglePage(bool toPersuade)
        {
            store.ToggleEnable(!toPersuade);
            persuade.ToggleEnable(toPersuade);
        }

        private bool ShouldShowPersuadePage()
        {
            return !PlayerPrefs.HasKey(PlayerPrefsHelper.Key_PersuadePage);
        }
    }
}