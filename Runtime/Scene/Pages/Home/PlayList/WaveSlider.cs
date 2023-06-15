using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.PlayList
{
    public class WaveSlider : MonoBehaviour
    {
       [SerializeField] private Animation[] _animations;


       private void OnEnable()
       {
           for(int i=0;i<_animations.Length;i++)
           {
               AnimationState state = _animations[i]["WaveSlider"];
               state.speed = Random.Range(0.5f, 1.5f);
               
               _animations[i].Play();
           }
       }

      private void OnDisable()
        {
            for (int i = 0; i < _animations.Length; i++)
            {
                _animations[i].Stop();
            }
        }
    }

}
