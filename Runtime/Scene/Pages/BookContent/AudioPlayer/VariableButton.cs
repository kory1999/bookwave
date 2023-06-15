using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.AudioPlayer
{
    public class VariableButton : MonoBehaviour
    {
        
        
        [SerializeField] private GameObject _nodePlay;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _nodePause;
        
        private Action<string> _onButtonTapCallback;
        private string _currentStatus;

        public void Setup(string initState,Action<string> onButtonTapCallback)
        {
            _onButtonTapCallback = onButtonTapCallback;
            
            _button.onClick.AddListener(HandleOnButtonTap);
            ChangeButtonStatus(initState);
        }

        public void ChangeButtonStatus(string status)
        {
            bool isPlay = status.Equals("Play");
        
            _nodePlay.SetActive(isPlay);
            _nodePause.SetActive(!isPlay);
        }

        private void HandleOnButtonTap()
        {
            _onButtonTapCallback?.Invoke(_currentStatus);
        }
    }
}