using System;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Analytics;
using BeWild.AIBook.Runtime.Global;
using BeWild.AIBook.Runtime.Manager;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.PlayList
{
    public class PlayListCenter
    {
        const string PlayListDataName = "PlayListData";
        const string LogHeader = nameof(PlayListCenter);
        const string PrefabPath = "PlayList/UIPagePlayList";
        private static PlayListCenter _instance;
        private LocalDataManager _localDataManager;
        private UIPagePlayList _pagePlayList;

        private PlayListCenter()
        {
        }

        public static PlayListCenter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlayListCenter();
                }

                return _instance;
            }
        }

        public void SetLocalDataManager(LocalDataManager manager)
        {
            _localDataManager = manager;
        }

        public void Initialize()
        {
            if (_pagePlayList != null)
                return;

            GameObject go = Resources.Load<GameObject>(PrefabPath);
            GameObject ins = GameObject.Instantiate(go, OverlayPage.OverlayPage.Instance.transform);
            _pagePlayList = ins.GetComponent<UIPagePlayList>();

            LoadLocalData();

            Application.focusChanged += b =>
            {
                if (!b) SaveLocalData();
            };
        }

        public void SaveLocalData()
        {
            if (_localDataManager == null)
                return;

            PlayListData data = new PlayListData();
            _pagePlayList.Itemlist2LocalData(data);
            _localDataManager.Save(data, PlayListDataName);
        }

        public void LoadLocalData()
        {
            if (_localDataManager == null)
                return;

            try
            {
                PlayListData data = _localDataManager.Load<PlayListData>(PlayListDataName);
                if (data != null && data.Items != null)
                {
                    List<PlayItem> lst = new List<PlayItem>(data.Items.Count);
                    foreach (var item in data.Items)
                    {
                        lst.Add(new PlayItem(item.BookID, item.Amount, item.Current, item.Name, item.Cached));
                    }

                    _pagePlayList.AddItems(lst);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void TriggerEvent(PlayCenterEvent e, EventData data)
        {
            switch (e)
            {
                case PlayCenterEvent.kOpenPage:
                    OnOpenPage(data);
                    break;
                case PlayCenterEvent.kClosePage:
                    OnClosePage(data);
                    break;
                case PlayCenterEvent.kAddItem:
                    OnAddItem(data);
                    break;
                case PlayCenterEvent.kAddItemAndPlay:
                    OnAddItemAndPlay(data);
                    break;
                case PlayCenterEvent.kRemoveBookwaves:
                    OnRemoveBookwaves(data);
                    break;
                case PlayCenterEvent.kChangeItemState:
                    OnChangeItemState(data);
                    break;
                case PlayCenterEvent.kIndexChaned:
                    OnIndexChanged(data);
                    break;
                case PlayCenterEvent.kRequestNextBook:
                    OnRequestNextBook(data);
                    break;
                default:
                    break;
            }
        }

        private void OnRequestNextBook(EventData data)
        {
            _pagePlayList.TriggerPlayNext(data.BookID);
        }

        public void OnOpenPage(EventData data)
        {
            TrackEvent(BookwavesAnalytics.Event_PlayList_Show);
            
            _pagePlayList.PopUp(true);
        }

        public void OnClosePage(EventData data)
        {
            TrackEvent(BookwavesAnalytics.Event_PlayList_Close);
            
            _pagePlayList.PopUp(false);
        }

        public void OnAddItem(EventData data)
        {
            if (data.Items.Count > 0)
            {
                _pagePlayList.AddItems(data.Items);
            }
        }

        public void OnAddItemAndPlay(EventData data)
        {
            if (data.Items.Count > 0)
            {
                PlayItem tmp= data.Items[0];
                int count = _pagePlayList.AddItems(data.Items);

                if (count > 0)
                {
                    _pagePlayList.ScrollFirstItem();
                    _pagePlayList.HandleItemClick(PlayItemEvent.kPlay, tmp);
                    _pagePlayList.RefreshAllItems();    
                }
                else
                {
                    _pagePlayList.Scroll2CurrentItem();
                }
                
                // int id = data.Items[0].BookID;
                // _pagePlayList.SetItemState(id, data.State);
            }
        }

        public void OnRemoveBookwaves(EventData data)
        {           
            TrackEvent(BookwavesAnalytics.Event_PlayList_Delete);

            _pagePlayList.RemoveItems(data.Items, true);
        }

        public void OnChangeItemState(EventData data)
        {
            _pagePlayList.SetItemState(data.BookID, data.State);
        }

        public void OnIndexChanged(EventData data)
        {
            _pagePlayList.SetItemIndex(data.BookID, data.ReadingIndex);
        }

        private void TrackEvent(string eventName)
        {
            GlobalEvent.GetEvent<TrackingEvent>().Publish(eventName);
        }
    }
}