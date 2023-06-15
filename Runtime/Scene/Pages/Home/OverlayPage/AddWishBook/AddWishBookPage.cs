using System;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Global;
using DG.Tweening;
using DP.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage.AddWishBook
{
    public class AddWishBookPage : OverlayUI
    {
        [SerializeField] private TMP_InputField _bookNameText;
        [SerializeField] private TMP_InputField _authorNameText;
        [SerializeField] private TMP_InputField _emailAddressText;
        [SerializeField] private Button _submitButton;
        [SerializeField] private TextMeshProUGUI _privacyText;
        [SerializeField] private Button _backButton;
        [SerializeField] private CanvasGroup _loadHint;
        [SerializeField] private TextMeshProUGUI _submitText;
        [SerializeField] private Vector3 _submitButtonInitSizeDelta;
        [SerializeField] private Vector3 _onSubmitSizeDelta;
        [SerializeField] private Sprite _selectRect;
        [SerializeField] private Sprite _deselectRect;

        private bool _bookNameHasContent = false;
        private bool _authorNameHasContent = false;
        private bool _emaliAddresshasContent = false;
        private Tweener _textTweener;
        private Tweener _loadHintTweener;
        private Tweener _buttonTweener;

        public override void Initialize(object parameters)
        {
            base.Initialize(parameters);
            _privacyText.text = $"Leave your email here so that we can contact you once this book is on shelf.<color=#2F6AF7><link=\"{BookwavesConstants.PrivacyPolicyUrl}\">Your privacy will be well protected.</link></color>";
            _privacyText.GetComponent<LocalizationTextLoader>().TryToRefreshText();
            _bookNameText.onEndEdit.AddListener(HandleOnBookNameEndEdit);
            _bookNameText.onSelect.AddListener(HandleOnBookNameSelect);
            _bookNameText.onDeselect.AddListener(HandleOnBookNameDeSelect);
            _authorNameText.onEndEdit.AddListener(HandleOnAuthorNameEndEdit);
            _authorNameText.onSelect.AddListener(HandleOnAuthorNameSelect);
            _authorNameText.onDeselect.AddListener(HandleOnAuthorNameDeSelect);
            _emailAddressText.onEndEdit.AddListener(HandOnEmailAddressEndEdit);
            _emailAddressText.onSelect.AddListener(HandleOnEmailAddressSelect);
            _emailAddressText.onDeselect.AddListener(HandleOnEmailAddressDeSelect);
            _submitButton.onClick.AddListener(HandleOnSubmitButtonTap);
            _backButton.onClick.AddListener(() => { OverlayPage.Instance.Hide<AddWishBookPage>(); });
        }

        public override void Hide(Action callback)
        {
            base.Hide(callback);
            callback?.Invoke();
        }

        private void HandleOnPrivacyPolicyTap()
        {
            Application.OpenURL(BookwavesConstants.PrivacyPolicyUrl);
        }

        private void HandleOnBookNameEndEdit(string content)
        {
            TrackEvent(BookwavesAnalytics.Event_Profile_ClickAddBookName);
            
            _bookNameHasContent = !string.IsNullOrWhiteSpace(content);
            _submitButton.interactable = _authorNameHasContent || _bookNameHasContent || _emaliAddresshasContent;
        }

        private void HandleOnAuthorNameEndEdit(string content)
        {
            TrackEvent(BookwavesAnalytics.Event_Profile_ClickAddBookAuthor);
            
            _authorNameHasContent = !string.IsNullOrWhiteSpace(content);

            _submitButton.interactable = _authorNameHasContent || _bookNameHasContent || _emaliAddresshasContent;
        }

        private void HandOnEmailAddressEndEdit(string content)
        {
            TrackEvent(BookwavesAnalytics.Event_Profile_ClickAddBookEmail);
            
            _emaliAddresshasContent = !string.IsNullOrWhiteSpace(content);
            _submitButton.interactable = _authorNameHasContent || _bookNameHasContent || _emaliAddresshasContent;
        }

        private void HandleOnSubmitButtonTap()
        {
            TrackEvent(BookwavesAnalytics.Event_Profile_ClickAddBookSubmit);
            
            _submitButton.targetGraphic.raycastTarget = false;
            _textTweener?.Kill();
            _textTweener = _submitText.DOFade(0, 0.2f);
            _loadHintTweener?.Kill();
            _loadHintTweener = _loadHint.DOFade(1f, 0.2f);
            _buttonTweener?.Kill();
            _buttonTweener = _submitButton.GetComponent<RectTransform>().DOSizeDelta(_onSubmitSizeDelta, 0.5f);

            GlobalEvent.GetEvent<AddBookEvent>().Publish(_bookNameText.text, _authorNameText.text,
                _emailAddressText.text,
                (result) =>
                {
                    string text = result ? "Submission Success" : "Submission Failed";
                    GlobalEvent.GetEvent<GetLocalizationEvent>().Publish(text, localizedText =>
                    {
                        GlobalEvent.GetEvent<ShowToastEvent>().Publish(localizedText, 1f);
                        OverlayPage.Instance.Hide<AddWishBookPage>();
                    });
                });
        }

        private void HandleOnBookNameSelect(string str)
        {
            _bookNameText.GetComponent<Image>().sprite = _selectRect;
        }

        private void HandleOnBookNameDeSelect(string str)
        {
            _bookNameText.GetComponent<Image>().sprite = _deselectRect;
        }

        private void HandleOnAuthorNameSelect(string str)
        {
            _authorNameText.GetComponent<Image>().sprite = _selectRect;
        }

        private void HandleOnAuthorNameDeSelect(string str)
        {
            _authorNameText.GetComponent<Image>().sprite = _deselectRect;
        }

        private void HandleOnEmailAddressSelect(string str)
        {
            _emailAddressText.GetComponent<Image>().sprite = _selectRect;
        }

        private void HandleOnEmailAddressDeSelect(string str)
        {
            _emailAddressText.GetComponent<Image>().sprite = _deselectRect;
        }

        private void OnDestroy()
        {
            _buttonTweener?.Kill();
            _textTweener?.Kill();
            _loadHintTweener?.Kill();
        }

        private void TrackEvent(string eventName)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(eventName);
        }
    }
}