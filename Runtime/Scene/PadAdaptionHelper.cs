using BeWild.AIBook.Runtime.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene
{
    public class PadAdaptionHelper : MonoBehaviour
    {
        [SerializeField] private float scaleFactor = 0.75f;
        [SerializeField] private int layoutGroupTopPadding = 0;
        
        private void Start()
        {
            if (GameManager.IsPadDevice)
            {
                transform.localScale *= scaleFactor;

                VerticalLayoutGroup group = GetComponent<VerticalLayoutGroup>();
                if (group != null)
                {
                    group.padding.top = layoutGroupTopPadding;
                }
            }
        }
    }
}