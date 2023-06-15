using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home
{
    public class OverlayUI : MonoBehaviour
    {
        [SerializeField] private Color selectColor, unselectColor;
        [SerializeField] private Button[] buttons;
        [SerializeField] private Image[] icons;
        [SerializeField] private Sprite[] iconAssets;
        [SerializeField] private TMP_Text[] texts;
        [SerializeField] private Image[] _circleImages;
        [SerializeField] private Image redSprite;
        public void Initialize(Action<int> tapCallback)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;
                buttons[i].onClick.AddListener(() => tapCallback?.Invoke(index));
            }

            InitRedSprite();

        }

        public void ToggleTo(int index)
        {
            int count = buttons.Length;
            if (index < count)
            {
                for (int i = 0; i < count; i++)
                {
                    bool active = i == index;
                    icons[i].color = active ? selectColor : unselectColor;
                    icons[i].sprite = active ? iconAssets[i * 2] : iconAssets[i * 2 + 1];
                    texts[i].color = active ? selectColor : unselectColor;
                    _circleImages[i].gameObject.SetActive(active);
                    if (i == 1 && active)
                    {
                        redSprite.gameObject.SetActive(false);
                        PlayerPrefs.SetString("hasStart","hasStart");
                        PlayerPrefs.Save();
                    }
                }
            }
        }

        private void InitRedSprite()
        {
            if (PlayerPrefs.HasKey("hasStart"))
            {
                redSprite.gameObject.SetActive(false);
            }
            else
            {
                redSprite.gameObject.SetActive(true);
            }
        }
    }
}