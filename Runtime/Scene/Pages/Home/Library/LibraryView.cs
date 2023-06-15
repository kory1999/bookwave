using System;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Library
{
    public abstract class LibraryView : HomeView
    {

        public enum LibraryType
        {
            History=0,
            Mark=10,
            Highlight=20
        }
        [SerializeField] protected BookListUI bookList;
        [SerializeField] protected Image nullImage;
        [SerializeField] protected TMP_Text nullText;
        [SerializeField] protected Button moreBookButton;
        
        protected LibraryType _libraryType;
        protected AccountData Data;
        protected List<BookBriefData> Books;

        private Action<int,int> _numberChangeEvent;

        public override void Initialize()
        {
            bookList.Initialize(HandleOnBookTap, null);
            moreBookButton.onClick.AddListener(HandleOnMoreButtonTap);
        }

        public override void ToggleVisual(bool on)
        {
            base.ToggleVisual(on);

            if (on)
            {
                RefreshUI();
            }
        }

        public void RefreshUI()
        {
            GlobalEvent.GetEvent<GetAccountDataEvent>().Publish(HandleOnAccountDataReceived);
        }

        public void NumberChannelEvent(Action<int,int> callback)
        {
            _numberChangeEvent = callback;
        }

        private void HandleOnAccountDataReceived(AccountData data)
        {
            Data = data;
            
            GlobalEvent.GetEvent<GetBooksEvent>().Publish(GetBooksToRequire(), HandleOnBooksBriefDataReceived);
        }

        protected abstract List<int> GetBooksToRequire();

        private void HandleOnBooksBriefDataReceived(List<BookBriefData> books)
        {
            if (books != null)
            {
                Books = books;

                _numberChangeEvent.Invoke((int)_libraryType,Books.Count);
            
                UpdateVisual();
            }
        }

        protected virtual void UpdateVisual()
        {
            Books.Reverse();
            SetNullPage(Books.Count);
        }

        protected virtual void SetNullPage(int number)
        {
            if (number == 0)
            {
                nullImage.gameObject.SetActive(true);
                nullText.gameObject.SetActive(true);
                moreBookButton.gameObject.SetActive(true);
                switch (_libraryType)
                {
                    case LibraryType.History:
                        GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("Try to find your next read via the For you page!",s=>nullText.text=s);
                        break;
                    case LibraryType.Mark:
                        GlobalEvent.GetEvent<GetLocalizationEvent>().Publish( "No bookmark here-yet!",s=>nullText.text=s);
                        break;
                    case LibraryType.Highlight:
                        GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("No highlights here-yet!",s=>nullText.text=s);
                        break;
                    default:
                        return;
                }
            }
            else
            {
                nullImage.gameObject.SetActive(false);
                nullText.gameObject.SetActive(false);
                moreBookButton.gameObject.SetActive(false);
            }
        }

        protected virtual void HandleOnBookTap(int id)
        {
            MainScene.Event.GetEvent<OpenBookEvent>().Publish(id);
        }

        protected abstract string GetMoreBookButtonTapTrackingName();

        private void HandleOnMoreButtonTap()
        {
            GlobalEvent.GetEvent<GetHomePageDataEvent>().Publish(true, data =>
            {
                if (data != null)
                {
                    GlobalEvent.GetEvent<TrackingEvent>().Publish(GetMoreBookButtonTapTrackingName());
                    CategoryLogic logic = new CategoryLogic();
                    logic.Initialize(new BookListData(), data.GetCategoryList(GameManager.Language)[0].id);
                    MainScene.Event.GetEvent<OpenSearchPageEvent>().Publish(logic, BookwavesConstants.BackButtonPriority_SearchPage);
                }
            });
        }
    }
}