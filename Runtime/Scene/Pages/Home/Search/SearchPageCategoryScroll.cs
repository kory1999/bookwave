using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class SearchPageCategoryScroll : MonoBehaviour
    {
        [SerializeField] private SearchPageCategoryScrollRect scrollRect;
        [SerializeField] private Transform _transform;
        [SerializeField] private Transform _center;
        private bool _isHorizontal;
        private float _distance;
        private Action<bool> _turnPage;
        private Func<bool, bool> _isEdgePage;
        private bool _isScroll;

        public void Initialize(Action<bool> turnPage, Func<bool, bool> isEdgePage)
        {
            _turnPage = turnPage;
            _isEdgePage = isEdgePage;
            scrollRect.OnPreBeginDragEvent += OnPreBeginDrag;
            scrollRect.OnDragEvent += OnDrag;
            scrollRect.OnEndDragEvent += OnEndDrag;
            _distance = 0;
            _isScroll = false;
        }

        public void ToggleScroll(bool on)
        {
            if (on)
            {
                _isScroll = true;
            }
            else
            {
                _isScroll = false;
            }
        }

        private void OnPreBeginDrag()
        {
            _isHorizontal = true;
        }

        private void OnDrag(float value)
        {
            if (_isScroll)
            {
                Vector3 tmp = new Vector3(value, 0, 0);
                if (_isHorizontal)
                {
                    if (value > 0)
                    {
                        if (_isEdgePage.Invoke(false))
                            return;
                    }

                    if (value < 0)
                    {
                        if (_isEdgePage.Invoke(true))
                            return;
                    }

                    gameObject.GetComponent<RectTransform>().localPosition += tmp;
                    _distance += value;
                }
            }
        
        }

        private void OnEndDrag(float value)
        {
            if (_isScroll)
            {
                _isHorizontal = false;
                //上一页
                if (_distance > _transform.position.x / 2)
                {
                    gameObject.transform.DOMoveX(_transform.position.x, 0.5f).OnComplete((() =>
                    {
                        gameObject.transform.position = _center.position;
                        _turnPage.Invoke(false);
                    }));
                }
                //下一页
                else if (_distance < -transform.position.x / 2)
                {
                    gameObject.transform.DOMoveX(-_transform.position.x, 0.5f).OnComplete((() =>
                    {
                        gameObject.transform.position = _center.position;
                        _turnPage.Invoke(true);
                    }));
                }

                _distance = 0;
            }
        }
    }
}