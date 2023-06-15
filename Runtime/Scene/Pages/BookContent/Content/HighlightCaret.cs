using System.Collections;
using System.Collections.Generic;
using BeWild.Framework.Runtime.Utils.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.Content
{
    public class HighlightCaret : MonoBehaviour
    {
        [SerializeField] private DraggableImage caret;

        public void SetCaretPosition(Vector2 position)
        {
            caret.transform.position = position;
        }

        public void CaretVisualToggle(bool value)
        {
            gameObject.SetActive(value);
        }
    }

}
