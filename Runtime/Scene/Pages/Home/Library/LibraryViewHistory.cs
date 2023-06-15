using System;
using System.Collections.Generic;
using System.Text;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using UnityEngine;
using UnityEngine.EventSystems;


namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Library
{
    public class LibraryViewHistory : LibraryView, IPointerClickHandler
    {
        private bool _isPopupShow = false;
        private HistoryBookListUI _historyBookListUI;
        private FilterType _libraryFilter;

        public enum FilterType
        {
            All,
            Finished,
            Continue
        }

        public override void Initialize()
        {
            base.Initialize();
            _historyBookListUI = bookList as HistoryBookListUI;
            _historyBookListUI.SetCallbackForHistoryBook(HandleOnHistoryClose, HandleOnHistoryBookFinish,
                HandleOnHistoryBookShare, HandleOnHistoryBookDelete);
        }
        
        public void SetFilter(FilterType filterType)
        {
            _libraryFilter= filterType;
            UpdateVisualByFilter(_libraryFilter);
        }

        public void UpdateVisualByFilter(FilterType filterType)
        {
            List<BookBriefData> _bookBriefDatas;
            if (filterType == FilterType.All)
            {
                TrackEvent(BookwavesAnalytics.Event_Library_ClickFilterAll);

                _bookBriefDatas = Books;
            }
            else if (filterType == FilterType.Continue)
            {
                TrackEvent(BookwavesAnalytics.Event_Library_ClickFilterContinue);
                
                _bookBriefDatas = Books.FindAll(book =>
                {
                    var historyData = Data.history.Find(b => b.id == book.id);
                    return !historyData.isFinished;
                });
            }
            else
            {
                TrackEvent(BookwavesAnalytics.Event_Library_ClickFilterFinished);
                
                _bookBriefDatas = Books.FindAll(book =>
                {
                    var historyData = Data.history.Find(b => b.id == book.id);
                    return historyData.isFinished;
                });
            }

            bookList.UpdateBooks(_bookBriefDatas);
            SetNullPage(_bookBriefDatas.Count);
            foreach (BookListBook book in bookList.GetAllBooks())
            {
                if (book is LibraryHistoryBook historyBook)
                {
                    historyBook.SetReadData(Data.history.Find(b => b.id == book.Id));
                }
            }
        }

        protected override List<int> GetBooksToRequire()
        {
            List<int> bookIdList = new List<int>();

            foreach (BookReadHistoryData readData in Data.history)
            {
                bookIdList.Add(readData.id);
            }

            _libraryType = LibraryType.History;
            return bookIdList;
        }

        protected override void UpdateVisual()
        {
            base.UpdateVisual();
            UpdateVisualByFilter(_libraryFilter);
        }

        protected override void HandleOnBookTap(int id)
        {
            if (_isPopupShow)
            {
                _historyBookListUI.SetAllClosePopup();
                _isPopupShow = false;
                return;
            }

            
            TrackEvent(BookwavesAnalytics.Event_Library_ClickHistoryBook);


            GlobalEvent.GetEvent<GetBookEvent>().Publish(id, bookBriefData =>
            {
                if (bookBriefData != null)
                {
                    GlobalEvent.GetEvent<GetBookHistoryData>().Publish(id, historyData =>
                    {
                        if (historyData != null)
                        {
                            MainScene.Event.GetEvent<OpenBookContentEvent>().Publish(bookBriefData, historyData.chapter,
                                historyData.fromTextPage, historyData.currentSeconds);
                        }
                    });
                }
            });
        }

        protected override string GetMoreBookButtonTapTrackingName()
        {
            return BookwavesAnalytics.Event_Library_ClickHistoryMoreBook;
        }
        
        private void HandleOnHistoryClose()
        {
            if (_isPopupShow)
            {
                _historyBookListUI.SetAllClosePopup();
                _isPopupShow = false;
            }
            else
            {
                _isPopupShow = true;
            }
            
        }

        private void HandleOnHistoryBookFinish(int id)
        {
            TrackEvent(BookwavesAnalytics.Event_Library_ClickHistorySubMenuFinish);
            
            GlobalEvent.GetEvent<GetAccountDataEvent>().Publish(data =>
            {
                data.history.Find(b => b.id == id).isFinished = true;
            });
            RefreshUI();
        }

        private void HandleOnHistoryBookShare(int id)
        {

            TrackEvent(BookwavesAnalytics.Event_Library_ClickHistorySubMenuShare);
            
            BookBriefData book = Books.Find(b => b.id == id);
            BookwavesNativeUtility.ShareBook(book.name, book.author);
        }

        private void HandleOnHistoryBookDelete(int id)
        {
            TrackEvent(BookwavesAnalytics.Event_Library_ClickHistorySubMenuDelete);
            
            GlobalEvent.GetEvent<GetAccountDataEvent>().Publish(data =>
            {
                var tmp = data.history.Find(b => b.id == id);
                data.history.Remove(tmp);
            });
            RefreshUI();
        }
        
        private void TrackEvent(string eventName)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(eventName);
        }

        [ContextMenu("ClearAll")]
        private void ClearAll()
        {
            GlobalEvent.GetEvent<DeleteAllHistoryData>().Publish();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isPopupShow)
            {
                _historyBookListUI.SetAllClosePopup();
                _isPopupShow = false;
            }
        }
    }
}