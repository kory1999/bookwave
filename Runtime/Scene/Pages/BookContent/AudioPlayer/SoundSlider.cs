using System;
using BW.Framework.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.AudioPlayer
{
    public class SoundSlider : MonoBehaviour
    {
        public event Action<float> OnSliderEndDragEvent; 
        
        [SerializeField] private TextMeshProUGUI _currentTimeText;
        [SerializeField] private TextMeshProUGUI _totalTimeText;
        [SerializeField] private Slider _slider;
        [SerializeField] private EventTrigger _eventTrigger;

        private bool _isPointerDown = false;

        public void Setup()
        {
            UpdateProgress(0,0);
            _eventTrigger.AddListener(EventTriggerType.PointerDown,HandleOnPointerDown);
            _eventTrigger.AddListener(EventTriggerType.PointerUp,HandleOnPointerUp);
        }

        public void UpdateProgress(double currentTime,double totalTime)
        {
            _currentTimeText.text = GetTimeFormat(currentTime);
            _totalTimeText.text = GetTimeFormat(totalTime);
            if (_isPointerDown)
            {
                return;
            }
            if (totalTime == 0)
            {
                _slider.value = 0;
            }
            else
            {
                _slider.value = ((float) currentTime) / ((float)totalTime);
            }
        }

        private string GetTimeFormat(double currentTime)
        {
            int minutes = (int)(currentTime / 60);
            int seconds = (int)(currentTime - minutes * 60);

            return $"{minutes:d2}:{seconds:d2}";
        }
        
        private void HandleOnPointerDown(BaseEventData data)
        {
            _isPointerDown = true;
        }

        private void HandleOnPointerUp(BaseEventData data)
        {
            _isPointerDown = false;
            OnSliderEndDragEvent?.Invoke(_slider.value);
        }
    }
}