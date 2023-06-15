using System;
using System.Collections.Generic;
using System.Linq;
using BeWild.AIBook.Runtime.Data;
using TMPro;
using UnityEngine;
using BeWild.AIBook.Runtime.Global;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Library
{
    public class HighlightTextData
    {
        public int BookID;
        public int CurrentSelectChapter;
        public int TextpageStartCharacterindex;
    }

    public class LibraryViewHighlightDetailsPage : HomeView
    {
        [SerializeField] private Button quitButton;
        [SerializeField] private TMP_Text bookNameText;
        [SerializeField] private LibraryHighlightBookDetails prefab;

        [SerializeField] private RectTransform _parentRectTransform;

        private List<LibraryHighlightBookDetails> _waves;

        private Action<int, string> _shareEvent;
        private Action<int> _deleteEvent;
        private Action<HighlightTextData> _tapEvent;
        private List<BookPage> _pages;


        public void Initialize(Action quitCallback, Action<int, string> shareCallback, Action<int> deleteCallback,
            Action<HighlightTextData> tapCallback)
        {
            _waves = new List<LibraryHighlightBookDetails>();
            _shareEvent = shareCallback;
            _deleteEvent = deleteCallback;
            _tapEvent = tapCallback;
            _pages = new List<BookPage>();

            quitButton.onClick.AddListener(() =>
            {
                quitCallback.Invoke();
                DestroyAllBook();
                ToggleVisual(false);
            });
        }

        public override void Initialize()
        {
        }

        public void SetData(int bookID, string bookName, BookHighLightData data)
        {
            bookNameText.text = bookName;
            
            if (data.marks.Count > _waves.Count)
            {
                int tmp = _waves.Count;
                for (int i = 0; i < data.marks.Count; i++)
                {
                    if (i < tmp)
                    {
                        _waves[i].SetDetailData(bookID, i, data.marks[i]);
                    }

                    LibraryHighlightBookDetails book = Instantiate(prefab, _parentRectTransform.transform,
                        false);
                    book.Initialize(HandleOnBookTapShare, HandleOnBookTapDelete, HandeleOnBookTap);
                    book.SetDetailData(bookID, i, data.marks[i]);
                    _waves.Add(book);
                }
            }
            else
            {
                for (int i = 0; i < _waves.Count; i++)
                {
                    if (i < data.marks.Count)
                    {
                        _waves[i].SetDetailData(bookID, i, data.marks[i]);
                    }
                    else
                    {
                        LibraryHighlightBookDetails waveTMp = _waves[i];
                        _waves.RemoveAt(i);
                        Destroy(waveTMp.gameObject);
                    }
                }
            }

            prefab.gameObject.SetActive(false);
        }

        public void DeleteHighlightText(int index)
        {
            for (int i = index + 1; i < _waves.Count; i++)
            {
                _waves[i].SetTitleText(i - 1);
            }

            LibraryHighlightBookDetails waveTMp = _waves[index];
            _waves.RemoveAt(index);
            Destroy(waveTMp.gameObject);
        }


        //点击分享
        private void HandleOnBookTapShare(int id, string text)
        {
            _shareEvent.Invoke(id, text);
        }

        //点击删除
        private void HandleOnBookTapDelete(int id)
        {
            _deleteEvent.Invoke(id);
        }

        private void HandeleOnBookTap(HighlightTextData data)
        {
            _tapEvent.Invoke(data);
        }

        private void DestroyAllBook()
        {
            for (int i = 0; i < _waves.Count; i++)
            {
                Destroy(_waves[i].gameObject);
            }

            _waves.Clear();
        }
    }
}