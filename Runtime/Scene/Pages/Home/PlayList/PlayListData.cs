using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.PlayList
{
    public enum PlayCenterEvent
    {
        kOpenPage,          // 打开页面
        kClosePage,         // 关闭页面
        kAddItem,           // 添加播放项
        kAddItemAndPlay,    // 添加播放项并播放
        kRemoveBookwaves,   // 移除播放项
        kChangeItemState,   // 改变播放项状态
        kIndexChaned,       // 播放项索引改变
        kRequestNextBook,   // 请求下一本书
        
    }
    public enum PlayItemState
    {
        kUnknown,
        kListening,
        kReading,
        kSelect
    }

    public enum PlayItemEvent
    {
        kUnknown,
        kDownload,
        kPlay,
    }
    [SerializeField]
    public class PlayItem:object
    {
        public PlayItem(int bookId,int amount,int current,string name,bool nativeVideo)
        {
            _bookId = bookId;
            _amount = amount;
            PlayIndex = current;
            _name = name;
            NativeVideo = nativeVideo;
            State = PlayItemState.kUnknown;
            Selected = false;
        }
        
        public bool Selected { get; set; }
        public int    Amount
        {
            get { return _amount; }
        }
        public int BookID
        {
            get { return _bookId; }
        }

        public int PlayIndex 
        {
            get;
            set;
        }
        
        public PlayItemState State
        {
            get;
            set;
        }
        
        public bool NativeVideo { get; set; }
        public string Name
        {
            get { return _name; }
        }

        private int   _bookId;
        private int   _amount;
        private string  _name;

        public override int GetHashCode()
        {
            return _bookId;
        }
    }
    
    
    public class EventData
    {
        //要添加的
        public List<PlayItem> Items;
        public int BookID;
        //对应ID的状态
        public PlayItemState State;
        //对应ID的当前章节
        public int ReadingIndex;
    }

    [Serializable]
    public class PlayItemData
    {
        public int BookID;
        public int Amount;
        public int Current;
        public bool Cached;
        public string Name;
    }
    
    
    [Serializable]
    public class PlayListData
    {
        public List<PlayItemData> Items;
    }
}