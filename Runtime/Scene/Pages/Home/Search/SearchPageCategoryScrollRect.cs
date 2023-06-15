using System;
using System.Collections;
using System.Collections.Generic;
using BW.Framework.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class SearchPageCategoryScrollRect : ScrollRect
    {
        public event Action OnPreBeginDragEvent;
        public event Action<float> OnDragEvent;
        public event Action<float> OnEndDragEvent;

        private bool _checkDragOrientation;
        private DataStatistics<Vector2> _data = new DataStatistics<Vector2>(3);

        public override void OnBeginDrag(PointerEventData eventData)
        {
            _data.Clear();
            _checkDragOrientation = true;
            
            base.OnBeginDrag(eventData);
        }
        
        public override void OnDrag(PointerEventData eventData)
        {
            if (_checkDragOrientation)
            {
                _data.Append(eventData.delta);
                
                if (_data.IsFull())
                {
                    Vector2 offset = _data.Average();
                    if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
                    {
                        OnPreBeginDragEvent?.Invoke();
                        vertical = false;
                    }
                    else
                    {
                        vertical = true;
                    }

                    _checkDragOrientation = false;
                }
            }

            OnDragEvent?.Invoke(eventData.delta.x);
            base.OnDrag(eventData);
        }
        
        public override void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragEvent?.Invoke(eventData.delta.x);
            base.OnEndDrag(eventData);
        }
        
     

    }
}