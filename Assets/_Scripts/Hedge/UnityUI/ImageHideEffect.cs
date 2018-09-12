using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hedge.UnityUI
{
    [RequireComponent(typeof(Image))]
    public class ImageHideEffect : HideEffect
    {
        public bool fade = true;      
        protected Image img;
        

        // Use this for initialization
        protected override void Awake()
        {
            base.Awake();
            img = GetComponent<Image>();
            if (fade)PlayEffect += FadeImage;
           
        }

        private void FadeImage()
        {
            img.CrossFadeAlpha(0, life_time, true);
           // img.color = new Color(img.color.r, img.color.g, img.color.b, img.color.a * current_life_time/life_time);
        }

    }

}
