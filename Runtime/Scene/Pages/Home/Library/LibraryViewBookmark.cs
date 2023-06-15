using System.Collections.Generic;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using BeWild.AIBook.Runtime.Scene.Pages.Home.PlayList;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Library
{
    public class LibraryViewBookmark : LibraryView
    {
        [SerializeField] private Button _playlistButton;

        public override void Initialize()
        {
            base.Initialize();
            _playlistButton.onClick.AddListener((() =>
            {
                HandleOnPlaylistButtonTap();

                Debug.Log("<debug> show playlist");
            }));
        }

        protected override List<int> GetBooksToRequire()
        {
            List<int> bookIdList = new List<int>();

            foreach (var mark in Data.bookMarks)
            {
                bookIdList.Add(mark);
            }

            _libraryType = LibraryType.Mark;
            return bookIdList;
        }

        protected override void UpdateVisual()
        {
            base.UpdateVisual();
            bookList.UpdateBooks(Books);
            if (Books.Count > 0)
            {
                _playlistButton.gameObject.SetActive(true);
            }
            else
            {
                _playlistButton.gameObject.SetActive(false);
            }
        }

        protected override void HandleOnBookTap(int id)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_Library_ClickBookMarkBook);

            base.HandleOnBookTap(id);
        }

        protected override string GetMoreBookButtonTapTrackingName()
        {
            return BookwavesAnalytics.Event_Library_ClickBookMarkMoreBook;
        }

        private void HandleOnPlaylistButtonTap()
        {
            EventData eventData = new EventData();
            string bookName = "";
            int totalChapter = 0;
            int currentChapter = 0;
            bool isDownload = false;
            eventData.Items = new List<PlayItem>(Books.Count);
            for (int i = 0; i < Books.Count; i++)
            {
                totalChapter = 0;
                currentChapter = 0;
                GlobalEvent.GetEvent<GetBookContentEvent>().Publish(Books[i].id, data =>
                {
                    //章节数
                    bookName = data.bookName;
                    totalChapter = data.pages.Length;
                    Debug.Log($"<debug> total chapter: {data.pages.Length}");
                });
                List<BookReadHistoryData> history = Data.history;
                for (int j = 0; j < history.Count; j++)
                {
                    //当前章节
                    if (history[j].id == Books[i].id)
                    {
                        currentChapter = (int)(history[j].progress * history[j].chapter);
                        Debug.Log($"<debug> current chapter: {history[j].progress * history[j].chapter}");
                    }
                }

                GlobalEvent.GetEvent<GetBookSoundCacheEvent>().Publish(Books[i].id, tmpString =>
                {
                    if (tmpString == null)
                    {
                        isDownload = false;
                    }
                    else
                    {
                        isDownload = true;
                    }
                });
                PlayItem item = new PlayItem(Books[i].id, totalChapter, currentChapter + 1, bookName, isDownload);
                eventData.Items.Add(item);
            }
            
            GlobalEvent.GetEvent<TrackingEvent>().Publish(BookwavesAnalytics.Event_Library_ClickBookMarkListenAll);
            
            PlayListCenter.Instance.TriggerEvent(PlayCenterEvent.kAddItemAndPlay, eventData);
            PlayListCenter.Instance.TriggerEvent(PlayCenterEvent.kOpenPage, null);


            Debug.Log("<debug> show playlist");
        }
    }
}