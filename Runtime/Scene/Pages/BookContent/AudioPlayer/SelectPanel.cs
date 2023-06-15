using System;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Manager;
using BeWild.Framework.Runtime.Utils.UI;
using BW.Framework.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.AudioPlayer
{
    public class SelectPanel : MonoBehaviour
    {
        public bool Enabled => _canvasGroup.alpha != 0;

        [SerializeField] private TextMeshProUGUI _panelTitle;
        [SerializeField] private ChapterSelectButton _selectionButton;
        [SerializeField] private ContentSizeFitter _contentSizeFitter;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Color _selectColor;
        [SerializeField] private Color _unSelectColor;
        [SerializeField] private Color _SelectFontColor;
        [SerializeField] private Color _unelectFontColor;

        private Action<string> _selectionTapCallback;
        private List<ChapterSelectButton> _allButtons;
        private bool _isLocked = true;
        private bool _isLockedInit = false;

        public void Setup(Action<string> onSelectionButtonTap)
        {
            _selectionTapCallback = onSelectionButtonTap;
            _allButtons = new List<ChapterSelectButton>();
        }

        public void Refresh(List<string> buttonNames, string defaultSelectName = "", string titleName = "")
        {
            if (!string.IsNullOrEmpty(titleName))
            {
                _panelTitle.text = titleName;
            }

            GenerateButtons(buttonNames);
            SelectButton(defaultSelectName);
            _isLockedInit = false;
            _contentSizeFitter.Refresh(this, false);
        }

        public void ToggleEnable(bool enable)
        {
            _canvasGroup.ToggleEnable(enable);
        }

        public void ToggleLock(bool locked)
        {
            _isLocked = locked;
            _isLockedInit = true;
            for(int i = 0; i < _allButtons.Count; i++)
            {
                _allButtons[i].ToggleLockVisual(locked && !GameManager.IsFreeChapter(i));
            }
        }

        private void GenerateButtons(List<string> buttonNames)
        {
            for (int i = 1; i < _allButtons.Count; i++)
            {
                Destroy(_allButtons[i].gameObject);
            }

            _allButtons.Clear();
            if (_selectionButton != null)
            {
                _selectionButton.GetComponent<Image>().color = _unSelectColor;
                _allButtons.Add(_selectionButton);
            }

            bool initFirstElement = false;

            for (int i = 0; i < buttonNames.Count; i++)
            {
                if (!initFirstElement)
                {
                    _selectionButton.ButtonName = buttonNames[i];
                    initFirstElement = true;
                    _selectionButton.Setup(HandleOnButtonTap);
                }
                else
                {
                    ChapterSelectButton newButton =
                        Instantiate(_selectionButton.gameObject, _selectionButton.transform.parent)
                            .GetComponent<ChapterSelectButton>();

                    newButton.ButtonName = buttonNames[i];
                    newButton.Setup(HandleOnButtonTap);
                    newButton.ToggleLockVisual(_isLocked);
                    _allButtons.Add(newButton);
                }
            }
        }

        public void SelectButton(string name = "", bool sendCallback = false)
        {
            UnSelectAll();
            ChapterSelectButton targetButton = null;
            if (string.IsNullOrEmpty(name) && _allButtons.Count > 0)
            {
                targetButton = _allButtons[0];
            }
            else
            {
                targetButton =
                    _allButtons.Find(button => button.GetComponentInChildren<TextMeshProUGUI>().text == name);
            }

            if (targetButton != null)
            {
                targetButton.GetComponent<Image>().color = _selectColor;
                targetButton.GetComponentInChildren<TextMeshProUGUI>().color = _SelectFontColor;
                targetButton.SetLockColor(_SelectFontColor);
                targetButton.ToggleInteract(false);
                if (sendCallback)
                {
                    _selectionTapCallback?.Invoke(targetButton.ButtonName);
                }
            }
        }

        private void UnSelectAll()
        {
            _allButtons.ForEach(button =>
            {
                button.ToggleInteract(true);
                button.GetComponent<Image>().color = _unSelectColor;
                button.GetComponentInChildren<TextMeshProUGUI>().color = _unelectFontColor;
                button.SetLockColor(_unelectFontColor);
            });
        }

        private void HandleOnButtonTap(ButtonWithName button)
        {
            _selectionTapCallback?.Invoke(button.ButtonName);
        }
    }
}