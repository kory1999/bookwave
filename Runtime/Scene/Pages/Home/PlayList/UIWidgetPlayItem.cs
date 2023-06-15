using System;
using Mosframe;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.PlayList
{
    public class UIWidgetPlayItem : MonoBehaviour
    {
        #region Widgets

        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _index;
        [SerializeField] private GameObject _nodeStateListening;
        [SerializeField] private GameObject _nodeStateReading;
        [SerializeField] private GameObject _nodeStateCached;
        [SerializeField] private GameObject _nodeStateRemote;
        [SerializeField] private GameObject _nodeStateSelect;
        [SerializeField] private Button _btnDownload;
        [SerializeField] private Button _btnPlay;
        [SerializeField] private Toggle _toggleSelect;

        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _normalColor;

        #endregion

        private PlayItem _itemData;

        public PlayItem GetBookData()
        {
            return _itemData;
        }
        
        public int BookID
        {
            get
            {
                if (_itemData == null)
                {
                    return -1;
                }
                return _itemData.BookID;
            }
        }

        public bool IsPlaying
        {
            get { return _nodeStateReading.activeSelf; }
        }

        public bool Selected
        {
            get { return _toggleSelect.isOn; }
        }

        public PlayItem ItemData
        {
            get { return _itemData; }
        }

        Action<PlayItemEvent, PlayItem> _onClicked;

        private void Start()
        {
            _btnDownload.onClick.AddListener(OnDownloadClicked);
            _btnPlay.onClick.AddListener(OnPlayClicked);
            _toggleSelect.onValueChanged.AddListener(OnSelectChanged);
        }

        private void OnDestroy()
        {
            _btnDownload.onClick.RemoveListener(OnDownloadClicked);
            _btnPlay.onClick.RemoveListener(OnPlayClicked);
            _toggleSelect.onValueChanged.RemoveListener(OnSelectChanged);
        }

        void OnSelectChanged(bool value)
        {
            if (_itemData != null)
            {
                _itemData.Selected = value;
            }
        }

        void OnDownloadClicked()
        {
            TriggerEvent(PlayItemEvent.kDownload);
        }

        void OnPlayClicked()
        {
            TriggerEvent(PlayItemEvent.kPlay);
        }

        public void SetClickCallback(Action<PlayItemEvent, PlayItem> callback)
        {
            _onClicked = callback;
        }

        void TriggerEvent(PlayItemEvent e)
        {
            if (_onClicked != null)
            {
                _onClicked?.Invoke(e, _itemData);
            }
        }

        public bool IsDownload
        {
            get
            {
                if (_itemData == null)
                {
                    return false;
                }

                return _itemData.NativeVideo;
            }
            set
            {
                if (_itemData != null)
                {
                    _itemData.NativeVideo = value;
                    SetDownloadState(value);
                }
            }
        }

        private void SetDownloadState(bool isDownload)
        {
            _nodeStateCached.SetActive(isDownload);
            _nodeStateRemote.SetActive(!isDownload);
        }

        public void SetReadingState(PlayItemState state)
        {
            _name.color = (state == PlayItemState.kListening || state == PlayItemState.kReading)
                ? _selectedColor
                : _normalColor;
            _nodeStateListening.SetActive(state == PlayItemState.kListening);
            _nodeStateReading.SetActive(state == PlayItemState.kReading);
            _nodeStateSelect.SetActive(state == PlayItemState.kSelect);
        }

        public void Setup(PlayItem item)
        {
            _itemData = item;

            Refresh();
        }

        public void RefreshSelectState()
        {
            if (_itemData != null)
            {
                _toggleSelect.isOn = _itemData.Selected;
            }
        }
        public void Refresh()
        {
            if (_itemData != null)
            {
                IsDownload = _itemData.NativeVideo;

                SetReadingState(_itemData.State);

                string idx = String.Format("{0}/{1}", _itemData.PlayIndex, _itemData.Amount);
                _index.SetText(idx);
                _name.SetText(_itemData.Name);

                _toggleSelect.isOn = _itemData.Selected;
            }
            else
            {
                _index.SetText("");
                _name.SetText("");
                SetReadingState(PlayItemState.kUnknown);
                _toggleSelect.isOn = false;
                IsDownload = false;
            }
        }
    }
}