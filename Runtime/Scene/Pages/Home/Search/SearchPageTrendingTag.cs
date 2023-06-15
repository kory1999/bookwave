using System;
using BW.Framework.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class SearchPageTrendingTag : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Button button;
        [SerializeField] private GameObject hotIcon;

        [SerializeField] private string FrameHotColor = "#F0F5FF";
        [SerializeField] private string FrameNormalColor = "#F5F5F5";
        [SerializeField] private string TextHotColor = "#162666";
        [SerializeField] private string TextNormalColor = "#999999";

        public void Initialize(Action<string> tapCallback)
        {
            button.onClick.AddListener(() => tapCallback?.Invoke(text.text));
        }

        public void SetData(string name, bool isHot)
        {
            text.text = name;

            if (hotIcon.activeSelf != isHot)
            {
                hotIcon.gameObject.SetActive(isHot);
            }
            
            GetComponent<Image>().color = Color.white.SetHex(isHot ? FrameHotColor : FrameNormalColor);
            text.color = text.color.SetHex(isHot ? TextHotColor : TextNormalColor);
        }
    }
}