using System;
using BW.Framework.Utils;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage
{
    public class OverlayUI : MonoBehaviour,IOverlayUI
    {
        public CanvasGroup MCanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup =GetComponent<CanvasGroup>();
                }

                return _canvasGroup;
            }
        }
        protected bool _inited = false;
        [SerializeField] protected CanvasGroup _canvasGroup;

        public virtual void Initialize(object parameters)
        {
            if (_inited)
            {
                Refresh();
                return;
            }
            _inited = true;
            
            
        }

        public void ToggleEnable(bool enable)
        {
            MCanvasGroup?.ToggleEnable(enable);
        }

        public virtual void Show(Action callback)
        {
            
        }

        public virtual void Hide(Action callback)
        {
            
        }

        protected virtual void Refresh()
        {
            
        }
    }
    
    public interface IOverlayUI
    {
        void Initialize(object parameters);
    }
}