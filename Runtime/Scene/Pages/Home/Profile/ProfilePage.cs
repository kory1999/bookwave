using System.Text;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage.AddWishBook;
using BeWild.Framework.Runtime.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Profile
{
    public class ProfilePage : HomeView
    {
        [SerializeField] private Button subscribe, restore, language, addBook, pp, tos, rateUs, feedback, study;
        [SerializeField] private TMP_Text version;
        [SerializeField] private GameObject vipState;
        
        public override void Initialize()
        {
            subscribe.onClick.AddListener(HandleOnSubscribeButton);
            restore.onClick.AddListener(HandleOnRestoreButton);
            language.onClick.AddListener(HandleOnLanguageSettingsButton);
            addBook.onClick.AddListener(HandleOnAddBooksButton);
            pp.onClick.AddListener(HandleOnPPButton);
            tos.onClick.AddListener(HandleOnToSButton);
            rateUs.onClick.AddListener(HandleOnRateUsButton);
            feedback.onClick.AddListener(HandleOnFeedbackButton);
            study.onClick.AddListener(HandleOnStudyButton);

            version.text = Application.version;

            if (GameManager.IsGameUnlocked)
            {
                ToggleVIPState(false);
            }
            else
            {
                GameManager.OnGameUnlockStateChanged += unlock =>
                {
                    vipState.SetActive(!unlock);
                };
            }
        }

        private void HandleOnStudyButton()
        {
            TrackEvent(BookwavesAnalytics.Event_Profile_ClickStudyButton);
            
            BookwavesNativeUtility.OpenUrl(BookwavesConstants.StudyWithBookwavesUrl);
        }

        private void ToggleVIPState(bool on)
        {
            vipState.SetActive(on);
        }

        private void HandleOnSubscribeButton()
        {
            TrackEvent(BookwavesAnalytics.Event_Profile_ClickPurchase);
            
            GlobalEvent.GetEvent<OpenStoreEvent>().Publish(BookwavesAnalytics.Prefix_Profile);
        }

        private void HandleOnRestoreButton()
        {
            TrackEvent(BookwavesAnalytics.Event_Profile_ClickRestore);
            
            GlobalEvent.GetEvent<RestorePurchaseEvent>().Publish();
        }

        private void HandleOnLanguageSettingsButton()
        {
            TrackEvent(BookwavesAnalytics.Event_Profile_ClickLanguageSettings);

            OverlayPage.OverlayPage.Instance.Show<LanguageSelectPage>();
        }

        private void HandleOnAddBooksButton()
        {
            TrackEvent(BookwavesAnalytics.Event_Profile_ClickAddBook);

            OverlayPage.OverlayPage.Instance.Show<AddWishBookPage>();
        }

        private void HandleOnPPButton()
        {
            TrackEvent(BookwavesAnalytics.Event_Profile_ClickPP);
            
            BookwavesNativeUtility.OpenUrl(BookwavesConstants.PrivacyPolicyUrl);
        }

        private void HandleOnToSButton()
        {
            TrackEvent(BookwavesAnalytics.Event_Profile_ClickTOS);
            
            BookwavesNativeUtility.OpenUrl(BookwavesConstants.TermsOfServiceUrl);
        }

        private void HandleOnRateUsButton()
        {
            TrackEvent(BookwavesAnalytics.Event_Profile_ClickRateUs);
            
            BookwavesNativeUtility.RateUs();
        }

        private void HandleOnFeedbackButton()
        {
            TrackEvent(BookwavesAnalytics.Event_Profile_ClickFeedback);

            string[] texts = new[] { "Feedback for Bookwaves", "Give some advises, or report bugs to us please.", "App version", "OS version", "Device model"};
            GlobalEvent.GetEvent<GetLocalizationArrayEvent>().Publish(texts, results =>
            {
                StringBuilder sb = new StringBuilder("\n\n\n");
                sb.AppendLine(results[1]);
                sb.AppendLine("========================");
                sb.AppendLine($"{results[2]}: {Application.version}");
                sb.AppendLine($"{results[3]}: {SystemInfo.operatingSystem}");
                sb.AppendLine($"{results[4]}: {SystemInfo.deviceModel}");
                sb.AppendLine("========================");
                new EmailSender().SendEmail(results[0], sb.ToString(), BookwavesConstants.FeedbackEmail);
            });
        }

        private void TrackEvent(string eventName)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(eventName);
        }
    }
}