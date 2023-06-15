using System;
using System.Collections;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class HomeTopBar : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text title;
        [SerializeField] private Button clearButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI _titleText;

        private Action<string> _searchCallback;

        public void Initialize(Action<string> searchCallback,
            Action backButtonCallback)
        {
            _searchCallback = searchCallback;
            inputField.shouldHideMobileInput = true;
            inputField.onValueChanged.AddListener(HandleOnValueChange);
            
            clearButton.onClick.AddListener(ClearInputField);
            backButton.onClick.AddListener(() => backButtonCallback?.Invoke());
        }

        public void ShowSearch()
        {
            inputField.gameObject.SetActive(true);
            title.gameObject.SetActive(false);
            _titleText.gameObject.SetActive(false);
        }

        public void ToggleBackButton(bool on)
        {
            backButton.gameObject.SetActive(on);

            // move search bar
            RectTransform rect = inputField.GetComponent<RectTransform>();
            Vector2 offset = rect.offsetMin;
            offset.x = on ? 100 : 0;
            rect.offsetMin = offset;
        }

        public void ClearSearchContent()
        {
            inputField.text = String.Empty;
        }

        public void ShowTitleText(string titleText)
        {
            _titleText.text = titleText;
            
            inputField.gameObject.SetActive(false);
            _titleText.gameObject.SetActive(true);
        }

        private void HandleOnValueChange(string value)
        {
            if (value != String.Empty)
            {
                StopAllCoroutines();
                StartCoroutine(DelaySearch(value));
            }
        }

        private IEnumerator DelaySearch(string value)
        {
            yield return new WaitForSeconds(1f);

            _searchCallback?.Invoke(value);
        }
        
        private void ClearInputField()
        {
            inputField.text = String.Empty;
        }
    }
}