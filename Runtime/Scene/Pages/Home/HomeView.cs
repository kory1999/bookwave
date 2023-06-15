using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home
{
    public abstract class HomeView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        
        public abstract void Initialize();

        public virtual void ToggleVisual(bool on)
        {
            canvasGroup.alpha = on ? 1f : 0f;
            canvasGroup.interactable = on;
            canvasGroup.blocksRaycasts = on;
        }

        public virtual void ToggleInteract(bool on)
        {
            
        }
    }
}