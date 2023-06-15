using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ScrollImage : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float speedX = 0;
    [SerializeField] private float speedY = 0;
    [SerializeField] private float startX = 0;
    [SerializeField] private float startY = 0;
    [SerializeField] private RawImage target;
    [SerializeField] private bool bUpdateEnable = true;

    private Rect _uvRect=new Rect();
    void Start()
    {
        if (target)
        {
            _uvRect.x = startX;
            _uvRect.y = startY;
            _uvRect.width = target.uvRect.width;
            _uvRect.height = target.uvRect.height;
            UpdateRawImage(_uvRect);   
        }
        else
        {
            Debug.LogError("ScrollImage missing update target");
        }
    }

    public void SetUpdateEnable(bool bEnable)
    {
        bUpdateEnable = bEnable;
    }

    private void UpdateRawImage(Rect rect)
    {
        if (target)
        {
            target.uvRect = rect;
        }
    }

    private void FixedUpdate()
    {
        if (bUpdateEnable && target)
        {
            _uvRect.x += (speedX * Time.deltaTime);
            _uvRect.y += (speedY * Time.deltaTime);

            if (_uvRect.x > 1.0f)
                _uvRect.x -= 1.0f;
            else if (_uvRect.x < -1.0f)
                _uvRect.x += 1.0f;
            
            if (_uvRect.y > 1.0f) 
                _uvRect.y -= 1.0f;
            else if (_uvRect.y < -1.0f)
                _uvRect.y += 1.0f;

            UpdateRawImage(_uvRect);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
