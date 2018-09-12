using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hedge.UnityUI
{
    [RequireComponent(typeof(Graphic))]
    public class GraphicHideEffect : HideEffect
    {

        public bool fade = true;
        protected Graphic graphic;


        // Use this for initialization
        protected override void Awake()
        {
            base.Awake();
            graphic = GetComponent<Graphic>();
            if (fade) PlayEffect += FadeImage;

        }

        private void FadeImage()
        {
            graphic.CrossFadeAlpha(0, life_time, true);
            // img.color = new Color(img.color.r, img.color.g, img.color.b, img.color.a * current_life_time/life_time);
        }
    }

}
