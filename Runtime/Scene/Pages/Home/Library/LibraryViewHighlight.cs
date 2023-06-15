using System.Collections.Generic;
using System.Text;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using BeWild.Framework.Runtime.Utils;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Library
{
    public class LibraryViewHighlight : LibraryView
    {
        [SerializeField] private LibraryViewHighlightDetailsPage libraryViewHighlightDetailsPage;

        private int _bookId;

        public override void Initialize()
        {
            base.Initialize();
            libraryViewHighlightDetailsPage.Initialize(HandleOnQuiteButton, HandleOnBookShare, HandleOnBookDelete,
                HandleOnBookTap);
        }


        protected override List<int> GetBooksToRequire()
        {
            List<int> idList = new List<int>();
            if (Data.readMarks.Count > 0)
            {
                Data.readMarks.ForEach(mark => { idList.Add(mark.id); });
            }

            _libraryType = LibraryType.Highlight;
            return idList;
        }

        protected override void UpdateVisual()
        {
            base.UpdateVisual();
            
            bookList.UpdateBooks(Books);

            foreach (BookListBook book in bookList.GetAllBooks())
            {
                if (book is LibraryHighlightBook historyBook)
                {
                    historyBook.SetBookHighlightData(Data.readMarks.Find(b => b.id == book.Id));
                }
            }
        }

        protected override void HandleOnBookTap(int id)
        {
            TrackEvent(BookwavesAnalytics.Event_Library_ClickHighlightBook);

            _bookId = id;
            libraryViewHighlightDetailsPage.ToggleVisual(true);
            libraryViewHighlightDetailsPage.SetData(Books.Find(b => b.id == id).id, Books.Find(b => b.id == id).name,
                Data.readMarks.Find(b => b.id == id));
        }

        protected override string GetMoreBookButtonTapTrackingName()
        {
            return BookwavesAnalytics.Event_Library_ClickHighlightMoreBook;
        }

        private void HandleOnQuiteButton()
        {
            base.ToggleVisual(true);
        }

        private void HandleOnBookShare(int id, string text)
        {
            TrackEvent(BookwavesAnalytics.Event_Library_ClickHighlightShare);

            GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("Share you a book", result =>
            {
                new NativeShareHelper().Share(result, GetShareBody(id, text));
            });
        }

        private void HandleOnBookDelete(int value)
        {
            TrackEvent(BookwavesAnalytics.Event_Library_ClickHighlightDelete);

            GlobalEvent.GetEvent<GetAccountDataEvent>().Publish(accountData =>
            {
                for (int i = 0; i < Data.readMarks.Count; i++)
                {
                    if (Data.readMarks[i].id == _bookId)
                    {
                        GlobalEvent.GetEvent<BookHighlightChangedEvent>()
                            .Publish(_bookId, Data.readMarks[i].marks[value], true);
                        Data.readMarks[i].marks.RemoveAt(value);
                    }
                }

                if (Data.readMarks.Find(b => b.id == _bookId).marks.Count == 0)
                {
                    Data.readMarks.Remove(Data.readMarks.Find(b => b.id == _bookId));
                }

                libraryViewHighlightDetailsPage.DeleteHighlightText(value);
            });
        }

        private void HandleOnBookTap(HighlightTextData highlightData)
        {
            BookBriefData data = Books.Find(b => b.id == highlightData.BookID);
            GameManager.RuntimeDataManager.TextPageStartCharacterIndex = highlightData.TextpageStartCharacterindex;
            MainScene.Event.GetEvent<OpenBookContentEvent>().Publish(data, highlightData.CurrentSelectChapter, true,0);
        }

        private string GetShareBody(int id, string text)
        {
            StringBuilder sb = new StringBuilder(text);
            BookBriefData book = Books.Find(b => b.id == id);
            if (book != null)
            {
                sb.AppendLine();
                sb.Append(book.name);
                sb.Append(", ");
                sb.Append(book.author);
                sb.Append(".");
            }

            return sb.ToString();
        }

        private void TrackEvent(string eventName)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(eventName);
        }
    }
}