using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hedge.UnityUI
{
    [RequireComponent(typeof(Text))]
    public class TextHideEffect : GraphicHideEffect
    {
        protected Text txt;


        // Use this for initialization
        protected override void Awake()
        {
            base.Awake();
            txt = graphic as Text;

        }

        public void SetText(string str)
        {
            txt.text = str;
        }
    
    }

}
