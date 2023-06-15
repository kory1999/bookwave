using System;
using BeWild.AIBook.Runtime.Global;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Library
{
    public class LibraryHighlightBookDetails : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text hightlightText;
        [SerializeField] private Button shareButton, deleteButton;
        [SerializeField] private Button clickButton;
        private HighlightTextData _highlightTextData;

        private int _id;
        private int _number;

        public void Initialize(Action<int, string> shareCallback, Action<int> deleteCallback,
            Action<HighlightTextData> clickButtonCallback)
        {
            shareButton.onClick.AddListener((() => shareCallback.Invoke(_number, hightlightText.text)));
            deleteButton.onClick.AddListener((() => deleteCallback.Invoke(_number)));
            clickButton.onClick.AddListener(() => clickButtonCallback.Invoke(_highlightTextData));
            _highlightTextData = new HighlightTextData();
        }

        public void SetDetailData(int bookID, int number, string highlight)
        {
            GlobalEvent.GetEvent<GetBookContentEvent>().Publish(bookID, bookContentData =>
            {
                string[] tmpStrings = highlight.Split('-');
                int pageIndex = int.Parse(tmpStrings[0]);
                int startIndex = int.Parse(tmpStrings[1]);
                int lenght = int.Parse(tmpStrings[2]) - startIndex;
                string wordText = bookContentData.pages[pageIndex].content.Substring(startIndex, lenght + 1);

                _highlightTextData.BookID = bookID;
                _highlightTextData.CurrentSelectChapter = pageIndex;
                _highlightTextData.TextpageStartCharacterindex = startIndex;

                gameObject.SetActive(true);
                _number = number;
                GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("Note",s=>titleText.text=$"{s} {number+1}");
                hightlightText.text = wordText;
            });
        }

        public void SetTitleText(int index)
        {
            _number = index;
            GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("Note",s=>titleText.text=$"{s} {index+1}");
        }
        
    }
}