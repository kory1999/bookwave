using System;
using System.Text;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage;
using BeWild.Framework.Runtime.Analytics;
using BeWild.Framework.Runtime.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent
{
    public class BookFinishPage : MonoBehaviour
    {
        [SerializeField] private RawImage _bookCover;
        [SerializeField] private TMP_Text _bookReadNumber;
        [SerializeField] private HomeBookGroup _bookGroup;
        [SerializeField] private Button _backButton, _shareButton;

        private BookListData _bookListData;
        private Action _closeContentPageEvent;
        private string _bookName, _author;

        private int _readNumber;

        public void Initialize(Action callback)
        {
            _bookListData = new BookListData();

            _bookGroup.Initialize(null, HandleOnBookTap);
            _backButton.onClick.AddListener((HandleOnCloseButton));
            _shareButton.onClick.AddListener(HandleOnShareButton);

            gameObject.SetActive(false);
            _closeContentPageEvent = callback;
        }

        public void Refresh(string bookName, string author, string coverUrl)
        {
            _bookName = bookName;
            _author = author;
            GlobalEvent.GetEvent<GetHomePageDataEvent>().Publish(false, HandleOnFinishDataReceived);
            _bookGroup.SetData(_bookListData);
            GlobalEvent.GetEvent<GetImageEvent>().Publish(coverUrl, HandleOnBookCoverReceived);
        }

        public void ShowFinishPage(int bookReadNumber)
        {
            TrackEvent(BookwavesAnalytics.Event_BookContent_FinishShow);
            AdjustAnalytics.PublishEvent(AdjustAnalytics.ADEvent_BookConent_FinishShow);

            gameObject.SetActive(true);
            
            string tmpString = "";
            if (bookReadNumber > 1)
            {
                GlobalEvent.GetEvent<GetLocalizationEvent>().Publish(
                    "You have read <size=80><color=#3658FF>000</color></size> books!",
                    s => { tmpString = s; });
            }
            else
            {
                GlobalEvent.GetEvent<GetLocalizationEvent>().Publish(
                    "You have read <size=80><color=#3658FF>000</color></size> book!",
                    s => { tmpString = s; });
            }

            _bookReadNumber.text = tmpString.Replace("000", bookReadNumber.ToString());
            MobileKeyboardManager.Instance.AddBackListener(HandleOnCloseButton,
                BookwavesConstants.BackButtonPriority_BookContentFinishPage);
        }


        private void HandleOnFinishDataReceived(HomePageData data)
        {
            if (data != null)
            {
                _bookListData = data.booksList[1];
            }
        }

        private void HandleOnBookCoverReceived(Texture2D texture2D)
        {
            _bookCover.texture = texture2D;
        }

        private void HandleOnBookTap(int value)
        {
            CloseMyPage();
            CloseBookContent();

            MainScene.Event.GetEvent<OpenBookEvent>().Publish(value);
        }

        private void HandleOnCloseButton()
        {
            CloseMyPage();
            CloseBookContent();

            BookwavesNativeUtility.TryWeeklyRateUs();
        }

        private void HandleOnShareButton()
        {
            TrackEvent(BookwavesAnalytics.Event_BookContent_FinishShare);

            BookwavesNativeUtility.ShareBook(_bookName, _author);
        }

        private void CloseBookContent()
        {
            _closeContentPageEvent.Invoke();
        }

        private void CloseMyPage()
        {
            MobileKeyboardManager.Instance.RemoveBackListener(HandleOnCloseButton);

            gameObject.SetActive(false);
        }

        private void TrackEvent(string eventName)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(eventName);
        }
    }
}