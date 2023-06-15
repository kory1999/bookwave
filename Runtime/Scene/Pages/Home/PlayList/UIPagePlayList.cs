using System;
using System.Collections.Generic;
using System.Linq;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using BW.Framework.Utils;
using DG.Tweening;
using Mosframe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.PlayList
{
    public class UIPagePlayList : MonoBehaviour, IDynamicScrollViewItemProxy
    {
        // Initialize list size to avoid memory allocation
        const int kMaxItem = 128;


        [SerializeField] private Button _btnBackground;
        [SerializeField] private Button _btnSelectAll;
        [SerializeField] private Button _btnClean;
        [SerializeField] private Button _btnClose;
        [SerializeField] private DynamicScrollView _scrollView;
        [SerializeField] private Transform _window;

        [SerializeField] private Transform _showTransform;
        [SerializeField] private Transform _hideTransform;

        public List<int> _downloadQueue = new List<int>();
        private List<PlayItem> _itemList = new List<PlayItem>(kMaxItem);
        private PlayItem _currentItem;
        private List<UIWidgetPlayItem> _allItems = new List<UIWidgetPlayItem>(8);

        private bool _bCleanMode;
        private bool _bIsOpen = false;
        private bool _isDownloading = false;

        private void Awake()
        {
            _scrollView.SetProxy(this);
            _scrollView.AutoScrollBottom = false;
            _scrollView.totalItemCount = 0;
        }

        private void OnDestroy()
        {
            _scrollView.SetProxy(null);
        }

        private void Start()
        {
            _btnSelectAll.onClick.AddListener(OnSelectAllClicked);
            _btnClean.onClick.AddListener(OnCleanClicked);
            _btnClose.onClick.AddListener(OnCloseClicked);
            _btnBackground.onClick.AddListener(OnCloseClicked);

            _bCleanMode = false;
        }

        public void Itemlist2LocalData(PlayListData data)
        {
            data.Items = new List<PlayItemData>(_itemList.Count);
            foreach (var item in _itemList)
            {
                data.Items.Add(new PlayItemData
                {
                    BookID = item.BookID,
                    Amount = item.Amount,
                    Current = item.PlayIndex,
                    Name = item.Name,
                    Cached = item.NativeVideo
                });
            }
        }


        public void PopUp(bool bShow)
        {
            if (_bIsOpen == bShow)
                return;
            _bIsOpen = bShow;
            _btnBackground.gameObject.SetActive(bShow);
            if (bShow)
            {
                _window.DOLocalMoveY(_showTransform.localPosition.y, 0.5f);
            }
            else
            {
                _window.DOLocalMoveY(_hideTransform.localPosition.y, 0.5f);
            }
        }

        void OnSelectAllClicked()
        {
            TrackEvent(BookwavesAnalytics.Event_PlayList_DeleteAll);

            foreach (var item in _itemList)
            {
                item.Selected = !item.Selected;
            }

            _scrollView.ForeachItem((t, i) =>
            {
                var item = t.GetComponent<UIWidgetPlayItem>();
                if (item)
                {
                    item.RefreshSelectState();
                }
            });
        }

        // void TryRefreshList()
        // {
        //     _scrollView.refresh();
        // }

        void OnCleanClicked()
        {
            TrackEvent(BookwavesAnalytics.Event_PlayList_Delete);

            if (!_bCleanMode)
            {
                if (_itemList.Count <= 0)
                {
                    OnCloseClicked();
                    return;
                }

                _btnSelectAll.gameObject.SetActive(true);
                _bCleanMode = true;

                RefreshAllItemsWithState(PlayItemState.kSelect);
            }
            else
            {
                List<PlayItem> items = new List<PlayItem>();
                foreach (var item in _itemList)
                {
                    if (item.Selected)
                    {
                        items.Add(item);
                        if (_currentItem == item)
                            _currentItem = null;
                    }
                }

                if (items.Count > 0)
                {
                    RemoveItems(items);
                    _scrollView.totalItemCount = _itemList.Count;
                }

                _btnSelectAll.gameObject.SetActive(false);

                RefreshAllItemsWithState(PlayItemState.kUnknown);
                RefreshAllItems();
                _bCleanMode = false;
            }
        }

        void RefreshAllItemsWithState(PlayItemState state)
        {
            foreach (var item in _allItems)
            {
                item.SetReadingState(state);
            }
        }

        void OnCloseClicked()
        {
            PopUp(false);
            if (_bCleanMode)
            {
                foreach (var item in _itemList)
                {
                    item.Selected = false;
                }
                _btnSelectAll.gameObject.SetActive(false);
                RefreshAllItemsWithState(PlayItemState.kUnknown);
                RefreshAllItems();
                _bCleanMode = false;
            }
        }

        public PlayItem GetItem(int index)
        {
            if (index < 0 || index >= _itemList.Count)
                return null;
            return _itemList[index];
        }
        
        public void ScrollItemToIndex(int index)
        {
            _scrollView.scrollByItemIndex(index);
        }

        public void Scroll2CurrentItem()
        {
            if (_currentItem != null)
            {
                for (int i = 0; i < _itemList.Count(); i++)
                {
                    if (_itemList[i] == _currentItem)
                    {
                        ScrollItemToIndex(i);
                        return;
                    }
                }
            }
        }

        public void ScrollFirstItem()
        {
            _scrollView.scrollToFirstPos();
        }

        public void HandleItemClick(PlayItemEvent evt, PlayItem item)
        {
            if (_bCleanMode)
                return;
            
            if (evt == PlayItemEvent.kPlay)
            {
                TrackEvent(BookwavesAnalytics.Event_PlayList_Play);

                if (_currentItem != null && _currentItem.BookID == item.BookID)
                    return;
                
                GlobalEvent.GetEvent<GetBookEvent>().Publish(item.BookID, data =>
                {
                    if (data == null)
                    {
                        BaseLogger.Log(nameof(UIPagePlayList),$"Get book data is null!",LogType.Error);
                        return;
                    }
                    EventData eventData = new EventData();
                    eventData.BookID = item.BookID;
                    eventData.State = PlayItemState.kListening;
                    int currentChapter = 0;
                    if (data.isFree || GameManager.IsGameUnlocked)
                    {
                        currentChapter = item.PlayIndex - 1;
                    }

                    MainScene.Event.GetEvent<OpenBookBriefEvent>().Publish(data);
                    MainScene.Event.GetEvent<OpenBookContentEvent>().Publish(data, currentChapter, false, 0);
                    //状态更改
                    //SetItemState(eventData.BookID, eventData.State);
                    //当前在播放的Item
                    
                });
            }
            else if (evt == PlayItemEvent.kDownload)
            {
                TrackEvent(BookwavesAnalytics.Event_PlayList_Download);
                //Get Book Brief
                GlobalEvent.GetEvent<GetBookEvent>().Publish(item.BookID, bookBriefData =>
                {
                    //can't download if you are not a vip
                    if (!bookBriefData.isFree && !GameManager.IsGameUnlocked)
                    {
                        GlobalEvent.GetEvent<OpenStoreEvent>().Publish(BookwavesAnalytics.Prefix_Playlist);
                        return;
                    }

                    // duplicate download is not allowed
                    if(_downloadQueue.IndexOf(item.BookID)>=0)
                        return;
                    
                    _downloadQueue.Add(item.BookID);
                    
                    //download toast show
                    GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("Start download",
                        text => { GlobalEvent.GetEvent<ShowToastEvent>().Publish(text, 0.2f); });

                    //download book
                    GlobalEvent.GetEvent<DownloadBookSoundEvent>().Publish(item.BookID, b =>
                    {
                        _downloadQueue.Remove(item.BookID);
                        //download toast show
                        GlobalEvent.GetEvent<GetLocalizationEvent>().Publish(
                            b ? "Download successful" : "Download failed",
                            text => { GlobalEvent.GetEvent<ShowToastEvent>().Publish(text, 0.2f); });

                        //download state change
                        if (b)
                        {
                            item.NativeVideo = true;
                            this.RefreshItemByBookId(item.BookID);
                        }
                    });
                });
            }
        }

        public void onUpdateItem(GameObject obj, int index)
        {
            var item = obj.GetComponent<UIWidgetPlayItem>();
            PlayItem playItem = GetItem(index);
            item.Setup(playItem);
            item.SetClickCallback(HandleItemClick);
            if (_bCleanMode)
            {
                item.SetReadingState(PlayItemState.kSelect);
            }
        }

        public void onNewItem(GameObject obj, int index)
        {
            var item = obj.GetComponent<UIWidgetPlayItem>();
            if (item != null)
            {
                _allItems.Add(item);
            }
        }

        public void RefreshItemByBookId(int bookId)
        {
            foreach (var item in _allItems)
            {
                if (item.BookID == bookId)
                {
                    
                    item.Refresh();
                    break;
                }
            }
        }

        public void RefreshAllItems()
        {
            foreach (var item in _allItems)
            {
                item.Refresh();
            }
        }

        public void RemoveItems(List<PlayItem> items, bool bRefresh = true)
        {
            foreach (var item in items)
            {
                RemovePlayItemByBookId(item.BookID, false);
                
            }

            if (bRefresh)
            {
                _scrollView.totalItemCount = _itemList.Count;
            }
        }

        public void AddItem(PlayItem item)
        {
            if (item != null)
            {
                _itemList.Insert(0,item);
                _scrollView.totalItemCount = _itemList.Count;
            }
        }

        public int AddItems(List<PlayItem> items)
        {
            if(items == null || items.Count == 0)
                return 0;
            
            int count = _scrollView.totalItemCount;

            for (int i = 0; i < items.Count; i++)
            {
                for (int j = 0; j < _itemList.Count; j++)
                {
                    if (items[i].BookID == _itemList[j].BookID)
                    {
                        _itemList[j].PlayIndex = items[i].PlayIndex;
                        _itemList[j].NativeVideo = items[i].NativeVideo;
                        if (_itemList[j].State != PlayItemState.kListening)
                        {
                            _itemList[j].State = items[i].State;    
                        }
                        items.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }


            _itemList.InsertRange(0, items);

            if (_itemList.Count != count)
            {
                _scrollView.totalItemCount = _itemList.Count;
            }

            return items.Count;
        }

        public bool SetItemIndex(int bookId, int index)
        {
            bool updated = false;
            foreach (var item in _itemList)
            {
                if (item.BookID == bookId)
                {
                    item.PlayIndex = index;
                    updated = true;
                }
            }

            if (updated)
            {
                RefreshItemByBookId(bookId);
            }

            return updated;
        }

        public bool SetItemState(int bookId, PlayItemState state)
        {
            bool updated = false;

            foreach (var item in _itemList)
            {
                if (item.BookID == bookId)
                {
                    if (state == PlayItemState.kListening)
                    {
                        if (_currentItem != null)
                        {
                            if (_currentItem != item)
                            {
                                _currentItem.State = PlayItemState.kUnknown;
                                RefreshItemByBookId(_currentItem.BookID);
                                _currentItem = item;
                                item.State = state;
                                updated = true;
                            }
                        }
                        else
                        {
                            item.State = state;
                            _currentItem = item;
                            RefreshItemByBookId(_currentItem.BookID);
                        }
                    }
                    
                    break;
                }
            }

            if (updated)
            {
                RefreshItemByBookId(bookId);
            }

            return updated;
        }


        public void TriggerPlayNext(int preBookId)
        {
            if (_itemList.Count <= 0)
            {
                return;
            }

            if (_itemList[_itemList.Count - 1].BookID == preBookId)
            {
                return;
            }

            for (int i = 0; i < _itemList.Count; ++i)
            {
                if (_itemList[i].BookID == preBookId)
                {
                    if (i + 1 < _itemList.Count)
                    {
                        var item = _itemList[i + 1];
                        HandleItemClick(PlayItemEvent.kPlay, item);
                    }

                    break;
                }
            }
        }

        public bool RemovePlayItemByBookId(int bookId, bool bRefresh)
        {
            for (int i = 0; i < _itemList.Count; i++)
            {
                if (_itemList[i].BookID == bookId)
                {
                    _itemList.RemoveAt(i);
                    if (bRefresh)
                    {
                        _scrollView.totalItemCount = _itemList.Count;
                    }

                    return true;
                }
            }

            return false;
        }

        private void TrackEvent(string eventName)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(eventName);
        }
    }
}