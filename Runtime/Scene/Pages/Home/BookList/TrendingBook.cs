using System;
using System.Collections;
using System.Collections.Generic;
using BeWild.AIBook.Runtime.Global;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.BookList
{
    public class TrendingBook : BookListBook
    {
        [SerializeField] private TMP_Text sortText;

        [SerializeField] private TMP_Text descriptionText;

        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text buttonText;

        [SerializeField] private Color[] sortTextColors;
        [SerializeField] private Sprite[] backgroundSprites;
        [SerializeField] private Color[] buttonTextColors;


        public void SetDetailData(int sort, string authorName, string description)
        {
            GlobalEvent.GetEvent<GetLocalizationEvent>().Publish("Top",s=>sortText.text=$"{s}.{sort+1}");
            
            if (sort + 1 > 4)
            {
                sortText.color = sortTextColors[3];
                backgroundImage.sprite = backgroundSprites[3];
                buttonText.color = buttonTextColors[3];
            }
            else
            {
                sortText.color = sortTextColors[sort % 4];
                backgroundImage.sprite = backgroundSprites[sort % 4];
                buttonText.color = buttonTextColors[sort % 4];
            }

            descriptionText.text = description;
        }
    }
}