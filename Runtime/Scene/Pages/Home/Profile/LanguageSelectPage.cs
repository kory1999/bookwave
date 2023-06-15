using System;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Profile
{
    public class LanguageSelectPage : OverlayPage.OverlayUI
    {
        [SerializeField] private Button closeButton, confirmButton;
        [SerializeField] private ToggleGroup toggleGroup;
        
        private AppLanguage _startLanguage;

        public override void Initialize(object param)
        {
            base.Initialize(param);
            
            GlobalEvent.GetEvent<GetLanguageEvent>().Publish(language =>
            {
                _startLanguage = language;

                toggleGroup.transform.GetChild((int)language).GetComponent<Toggle>().isOn = true;
            });

            closeButton.onClick.AddListener(RequiresToClose);
            
            confirmButton.onClick.AddListener(() =>
            {
                ApplyLanguageSetting();

                RequiresToClose();
            });
        }

        public override void Hide(Action callback)
        {
            callback?.Invoke();
        }

        private AppLanguage GetCurrentLanguage()
        {
            int index = 0;
            for (int i = 0; i < toggleGroup.transform.childCount; i++)
            {
                if (toggleGroup.transform.GetChild(i).GetComponent<Toggle>().isOn)
                {
                    index = i;
                    break;
                }
            }
            return (AppLanguage)index;
        }

        private void ApplyLanguageSetting()
        {
            AppLanguage selectedLanguage = GetCurrentLanguage();
            
            if (_startLanguage != selectedLanguage)
            {
                if (selectedLanguage == AppLanguage.English)
                {
                    GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_Profile_ClickLanguageSettingsEnglish);
                }
                else if(selectedLanguage == AppLanguage.Spanish)
                {
                    GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_Profile_ClickLanguageSettingsSpanish);
                }
                
                GlobalEvent.GetEvent<SetLanguageEvent>().Publish(selectedLanguage);
            }
        }

        private void RequiresToClose()
        {
            OverlayPage.OverlayPage.Instance.Hide<LanguageSelectPage>();
        }
    }
}