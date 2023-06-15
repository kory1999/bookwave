using System;
using BeWild.AIBook.Runtime.Manager;
using BW.Framework.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookBrief
{
    public class BookBriefSelection : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _chapterText;
        [SerializeField] private TextMeshProUGUI _chapterNumText;
        [SerializeField] private Button _button;
        [SerializeField] private Image _line;
        [SerializeField] private Image _normalImage;
        [SerializeField] private Image _vipLock;

        private int _chapterIndex;
        private Action<int> _tapCallback;

        public void Setup(int chapterIndex, string chapterName, bool showLine, Action<int> tapCallback, bool isLocked)
        {
            _tapCallback = tapCallback;
            _chapterIndex = chapterIndex;
            _chapterNumText.text = (chapterIndex + 1).ToString();
            _chapterText.text = chapterName;
            _button.onClick.AddListener(HandleOnTap);
            _line.color = new Color(_line.color.r, _line.color.g, _line.color.b, showLine ? 1f : 0);
            
            ToggleVIPLock(isLocked);
        }
        
        public void ToggleVIPLock(bool on)
        {
            //不再显示VIP锁，所以这里直接设置为false。modify by:wenyong
            bool showVipLock = false && on && !GameManager.IsFreeChapter(_chapterIndex);
            if (_vipLock != null)
            {
                _normalImage.ChangeAlpha(showVipLock ? 0f : 1f);
                _vipLock.ChangeAlpha(showVipLock ? 1f : 0f);
            }
        }

        private void HandleOnTap()
        {
            _tapCallback?.Invoke(_chapterIndex);
        }
    }
}