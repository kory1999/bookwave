using System;
using BeWild.AIBook.Runtime.Data;
using BW.Framework.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class HomePageCategoryColor : HomePageCategory
    {
        [SerializeField] private Image frame;

        private bool _isActiveState = true;
        [SerializeField] private string TextActiveColor = "#162666";
        [SerializeField] private string TextInactiveColor = "#999999";
        [SerializeField] private string FrameInactiveColor = "#F5F5F5";
        [SerializeField] private string FrameActiveColor = "#2F6AF7";

        public override void Initialize(CategoryData data, Action<int> tapCallback)
        {
            base.Initialize(data, tapCallback);

            // 不需要再进行颜色设置，直接在prefab中设置好颜色即可
            // if(!string.IsNullOrEmpty(data.color) && _isActiveState)
            // {
            //     frame.color = frame.color.SetHex(data.color);
            // }
        }

        public void ToggleActiveState(bool on)
        {
            _isActiveState = on;

            TMP_Text text = GetComponentInChildren<TMP_Text>();
            text.color = text.color.SetHex(on ? TextActiveColor : TextInactiveColor);
            frame.color = frame.color.SetHex(on ? FrameActiveColor : FrameInactiveColor);
        }
    }
}