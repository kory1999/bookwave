using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.Profile
{
    public class LanguageSelectPageToggle : MonoBehaviour
    {
        [SerializeField] private Color onColor, offColor, textOnColor,textOffColor;
        [SerializeField] private Image baseImage;
        [SerializeField] private TextMeshProUGUI _text;
        
        private void Start()
        {
            Toggle toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(Toggle);
            
            Toggle(toggle.isOn);
        }

        private void Toggle(bool on)
        {
            baseImage.color = on ? onColor : offColor;
            _text.color = on ? textOnColor : textOffColor;
        }
    }
}