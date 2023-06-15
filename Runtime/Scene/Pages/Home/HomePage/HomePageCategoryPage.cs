using UnityEngine;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    // used in search page as vertical layout
    public class HomePageCategoryPage : HomePageCategoryBase
    {
        private const float Interval = 30f;

        protected override void Refresh()
        {
            if (Categories == null || Categories.Count == 0)
            {
                return;
            }
            
            Categories.Sort((c1, c2) => c1.GetComponent<RectTransform>().rect.width.CompareTo(c2.GetComponent<RectTransform>().rect.width));

            RectTransform myRect = GetComponent<RectTransform>();
            float totalWidth = myRect.rect.width;
            float lineInterval = Categories[0].GetComponent<RectTransform>().rect.height + Interval;
            // update layout based on size
            float lineEnd = 50f;
            int lineIndex = 0;
            bool nextLine = false;
            float maxWith = 0;
            for (int i = 0; i < Categories.Count; i++)
            {
                RectTransform cRect = Categories[i].GetComponent<RectTransform>();
                float cWidth = cRect.rect.width;

                if (lineEnd + Interval + cWidth > totalWidth)
                {
                    if (nextLine)
                    {
                        lineIndex++;
                        lineEnd = 50;
                        nextLine = false;
                    }
                    else
                    {
                        nextLine = true;
                    }
                }

                cRect.anchoredPosition = new Vector2(lineEnd, -lineIndex * lineInterval);
                lineEnd += Interval + cWidth;
                
                if (lineEnd > maxWith)
                {
                    maxWith = lineEnd;
                }
            }

            GetComponent<RectTransform>().sizeDelta = new Vector2(maxWith, (lineIndex + 1) * lineInterval);
        }
    }
}