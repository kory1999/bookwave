using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.OverlayPage
{
    public class UserGuideFinishPage : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private List<CanvasGroup> _Lines;
        [SerializeField] private List<Transform> _checks;

        private Tweener _tweener;

        public void DoFill(Action callback)
        {
            _tweener?.Kill();
            _tweener = DOTween.To(() => { return _slider.value; }, (newValue) =>
            {
                _progressText.text = $"{((int) (newValue * 100f)).ToString()}%";
                _slider.value = newValue;
            }, 1f, 6f).OnComplete(() =>
            {
                callback?.Invoke();
            });

            StartCoroutine(ShowTexts());
        }

        private IEnumerator ShowTexts()
        {
            for (int i = 0; i < _Lines.Count; i++)
            {
                int index = i;
                RectTransform rectTransform = _Lines[i].GetComponent<RectTransform>();
                Vector2 currentAnchoredPosition = rectTransform.anchoredPosition;
                rectTransform.anchoredPosition =
                    new Vector2(currentAnchoredPosition.x, currentAnchoredPosition.y - 200f);
                rectTransform.DOAnchorPosY(currentAnchoredPosition.y, 1f).SetEase(Ease.InOutQuad);
                DOTween.To(() => { return _Lines[index].alpha; }, (newValue) => { _Lines[index].alpha = newValue;}, 1f,1f);
                yield return new WaitForSeconds(1f);
                _checks[i].DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBounce);
                yield return new WaitForSeconds(0.25f);
            }
        }

        private void OnDestroy()
        {
            _tweener?.Kill();
        }
    }
}