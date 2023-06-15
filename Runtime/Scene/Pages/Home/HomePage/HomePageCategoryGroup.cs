using System.Collections.Generic;
using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    // used in home page as horizontal layout
    public class HomePageCategoryGroup : HomePageCategoryBase
    {
        [SerializeField] private float HorizontalInterval = 20f;
        [SerializeField] private float VerticalInterval = 28f;
        [SerializeField] private float LeftPadding = 50f;

        [SerializeField] private RectTransform scrollerRect;

        public void SetCategories(List<HomePageCategory> categorys)
        {
            Categories = categorys;
            
            Refresh();
        }
        
        protected override void Refresh()
        {
            if (Categories.Count == 0)
            {
                return;
            }
            
            float totalHeight = scrollerRect.rect.height;
            float categoryHeight = Categories[0].GetComponent<RectTransform>().rect.height;
            int lineCount = 1;
            if (totalHeight > categoryHeight)
            {
                lineCount += Mathf.FloorToInt((totalHeight - categoryHeight) / (categoryHeight + HorizontalInterval));
            }

            float[] lineLengthArray = new float[lineCount];
            for (int i = 0; i < lineLengthArray.Length; i++)
            {
                lineLengthArray[i] += LeftPadding;
            }
            
            foreach (HomePageCategory category in Categories)
            {
                int lineToAdd = GetShortestLine();
                RectTransform categoryRect = category.GetComponent<RectTransform>();
                categoryRect.anchoredPosition = new Vector2(lineLengthArray[lineToAdd], - lineToAdd * (categoryHeight + VerticalInterval));
                lineLengthArray[lineToAdd] += categoryRect.rect.width + HorizontalInterval;
            }

            scrollerRect.sizeDelta = new Vector2(GetLongestLineWidth(), totalHeight);

            int GetShortestLine()
            {
                int index = 0;
                float length = float.MaxValue;
                for (int i = 0; i < lineLengthArray.Length; i++)
                {
                    if (lineLengthArray[i] < length)
                    {
                        length = lineLengthArray[i];
                        index = i;
                    }
                }

                return index;
            }

            float GetLongestLineWidth()
            {
                float length = float.MinValue;
                for (int i = 0; i < lineLengthArray.Length; i++)
                {
                    if (lineLengthArray[i] > length)
                    {
                        length = lineLengthArray[i];
                    }
                }

                return length;
            }
        }
    }
}