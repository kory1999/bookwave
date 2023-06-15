using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Popup
{
    public class GeneralToast : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text, _textSizeFitter;
        [SerializeField] private Image _frame;

        private Tweener _tweener;
        
        public void Show(string text, float duration, Action callback)
        {
            _text.text = text;
            _textSizeFitter.text = text;

            _tweener = DOTween.To(SetAlpha, 0f, 1f, 0.5f).SetEase(Ease.InOutQuad);
            _tweener.onComplete += () =>
            {
                _tweener = DOTween.To(SetAlpha, 1f, 0f, 0.5f).SetEase(Ease.InOutQuad).SetDelay(duration);
                _tweener.onComplete += () =>
                {
                    _tweener = null;
                    callback?.Invoke();
                };
            };
        }

        public void SetAlpha(float alpha)
        {
            Color c = _text.color;
            c.a = alpha;
            _text.color = c;
            _textSizeFitter.color = c;
            
            c = _frame.color;
            c.a = alpha * 0.7f;
            _frame.color = c;
        }

        public bool IsShowing()
        {
            return _tweener != null;
        }

        private void OnDestroy()
        {
            if (_tweener != null)
            {
                _tweener.Kill();
            }
        }
    }
}