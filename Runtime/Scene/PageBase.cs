using System;
using BW.Framework.Utils;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene
{
    public abstract class PageBase : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private bool _initialized;
        private bool _uiDisplayed;
        private int _myPageIndex;
        private Action<int> _closeCallback;
        
        // will be called once when game launch.
        protected abstract void Initialize();
        
        // will be called once when first time show this page.
        protected virtual void DisplayUI(){}

        // will be called when this page is add or remove from ui stack.
        protected abstract void DoToggleVisual(bool on, Action finishCallback);

        // will be called when this page is enable or disabled in ui stack. notice that only the highest page in stack is enable.
        protected abstract void DoToggleInteract(bool on);

        public void TryToInitialize(int index, Action<int> closeCallback)
        {
            if (!_initialized)
            {
                _initialized = true;

                _myPageIndex = index;

                _closeCallback = closeCallback;
                
                Initialize();
            }
        }

        public void TryToDisplayUI()
        {
            if (!_uiDisplayed)
            {
                _uiDisplayed = true;
                
                DisplayUI();
            }
        }

        public virtual void RefreshPage(bool isGameUnlocked)
        {
            
        }

        public void ToggleVisual(bool on, Action finishCallback)
        {
            Log($"toggle {gameObject.name} visual to {on}");
            
            DoToggleVisual(on, finishCallback);
        }

        public void ToggleInteract(bool on)
        {
            Log($"toggle {gameObject.name} interact to {on}");
            
            DoToggleInteract(on);
        }

        public virtual void ToggleVIPState(bool unlock)
        {
            Log($"toggle {gameObject.name} VIP state to {(unlock ? "unlock" : "lock")}");
        }

        protected void DoClose()
        {
            _closeCallback?.Invoke(_myPageIndex);
        }

        private void Log(string info)
        {
            BaseLogger.Log(MainSceneUI.LogHeader, info);
        }
    }
}