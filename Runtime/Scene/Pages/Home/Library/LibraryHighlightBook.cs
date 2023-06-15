using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using TMPro;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Library
{
    public class LibraryHighlightBook : BookListBook
    {
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private TMP_Text _noteText;

        public void SetBookHighlightData(BookHighLightData data)
        {
            buttonText.text = data.marks.Count.ToString();
            
            if (data.marks.Count > 1)
            {
                GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("Notes",s=>_noteText.text=s);
            }
            else
            {
                GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("Note",s=>_noteText.text=s);
            }
        }
    }
}