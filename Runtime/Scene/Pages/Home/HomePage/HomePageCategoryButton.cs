using System;
using BeWild.AIBook.Runtime.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class HomePageCategoryButton : HomePageCategory
    {
        [SerializeField] private Sprite _selectedImage;
        [SerializeField] private Sprite _disableImage;
        private Image _image;
        [FormerlySerializedAs("_textColor")] [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _disableColor;
        private bool _isGray = true;
        private TextMeshProUGUI _text;

        public override void Initialize(CategoryData data, Action<int> tapCallback)
        {
            base.Initialize(data, tapCallback);
            ChangeToGray(true);
        }

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(HandleOnTap);
            _text = GetComponentInChildren<TextMeshProUGUI>();
            ChangeToGray(true);
        }

        private void HandleOnTap()
        {
            ChangeToGray(!_isGray);
            _isGray = !_isGray;
        }

        private void ChangeToGray(bool enable)
        {
            if (_image == null)
            {
                _image = GetComponent<Image>();
            }

            _image.sprite = enable ? _disableImage : _selectedImage;

            if (RawImage == null)
            {
                RawImage = GetComponentInChildren<RawImage>(true);
            }

            RawImage.color = enable ? _disableColor : _selectedColor;

            _text.color = enable ? _disableColor : _selectedColor;

        }
    }
}