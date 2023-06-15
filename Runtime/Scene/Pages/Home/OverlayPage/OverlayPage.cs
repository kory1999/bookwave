using System;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Global;
using BeWild.Framework.Runtime.Utils;
using BW.Framework.Utils;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage
{
    public class OverlayPage : MonoBehaviour
    {
        private static OverlayPage _instance;

        public static OverlayPage Instance => _instance;

        private List<OverlayUI> _allOverlayUis;
        private const string _overlayUIPath = "OverlayUI";
        private Canvas _canvas;

        private void Awake()
        {
            _allOverlayUis = new List<OverlayUI>();
            _instance = this;
            _canvas = GetComponent<Canvas>();
        }

        public void SetSortingOrder(int order)
        {
            if (_canvas.sortingOrder != order)
            {
                _canvas.sortingOrder = order;
            }
        }

        public void Show<T>(object parameters = null, int pageIndex = -1, int overlayOrder = 40) where T : OverlayUI
        {
            BaseLogger.Log(nameof(OverlayPage),$"try to show {typeof(T).Name}");
            SetSortingOrder(overlayOrder);
            
            T ui;
            if (!TryGetUI<T>(out ui))
            {
                ui = Instantiate(Resources.Load<GameObject>($"{_overlayUIPath}/{typeof(T).Name}"), transform)
                    .GetComponent<T>();
                ui.transform.localScale = Vector3.one;
                if (pageIndex > -1)
                {
                    ui.transform.SetSiblingIndex(pageIndex);
                }
                
                _allOverlayUis.Add(ui);
            }
            
            ui.ToggleEnable(true);
            ui.Initialize(parameters);
            ui.Show(null);

            RefreshMobileBackButton();
        }

        public void Hide<T>(Action callback = null) where T : OverlayUI
        {
            T ui;
            if (TryGetUI(out ui))
            {
                HideUI(ui, callback);
            }
        }

        public bool TryGetUI<T>(out T ui) where T : OverlayUI
        {
            for (int i = 0; i < _allOverlayUis.Count; i++)
            {
                if (_allOverlayUis[i] is T)
                {
                    ui = _allOverlayUis[i] as T;
                    return true;
                }
            }

            ui = null;
            return false;
        }

        private void RefreshMobileBackButton()
        {
            if (_allOverlayUis.Count > 0)
            {
                MobileKeyboardManager.Instance.AddBackListener(HandleOnMobileBackButton, BookwavesConstants.BackButtonPriority_Overlay);
            }
            else
            {
                MobileKeyboardManager.Instance.RemoveBackListener(HandleOnMobileBackButton);
            }
        }

        private void HandleOnMobileBackButton()
        {
            HideUI(_allOverlayUis[_allOverlayUis.Count - 1], null);
        }

        private void HideUI(OverlayUI ui, Action callback)
        {
            ui.Hide(() =>
            {
                ui.ToggleEnable(false);
                
                _allOverlayUis.Remove(ui);
                Destroy(ui.gameObject);
                
                callback?.Invoke();
            });

            RefreshMobileBackButton();
        }
    }
}