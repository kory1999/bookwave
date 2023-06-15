using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage
{
    public class DailyStudyTimeSelection : MonoBehaviour
    {
        [SerializeField] Color UnselectedColor = new Color(1f, 1f, 1f);
        [SerializeField] Color SelectedColor = new Color(0.2f, 0.2f, 0.2f);
        [SerializeField] Color IconSelectedColor = new Color(0.2f, 0.2f, 0.2f);
        [SerializeField] Color IconUnselectedColor = new Color(0, 0, 0);
        [SerializeField] private Graphic _DisplayGraphic;
        [SerializeField] Button _button;
        [SerializeField] Image[] _icons;
        bool _bSelected = false;

        public bool Selected
        {
            get { return _bSelected; }
        }
        public void Initialize(Action<DailyStudyTimeSelection> tapCallback)
        {
            _button.onClick.AddListener(() =>
            {
                tapCallback?.Invoke(this);
            });
            
            SetSelected(false);
        }

        public void SetSelected(bool bSelect)
        {
            _DisplayGraphic.color = bSelect ? SelectedColor : UnselectedColor;
            foreach (var icon in _icons)
            {
                icon.color = bSelect ? IconSelectedColor : IconUnselectedColor;
            }
            _bSelected = bSelect;
        }
    }
}