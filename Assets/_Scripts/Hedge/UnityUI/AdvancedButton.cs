using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace Hedge
{ namespace Tools
    {
        namespace UnityUI
        {
            public class AdvancedButton : Selectable
            {

                public event Func<PointerEventData,bool> onPressHold;
                [SerializeField] float delay = 0.3f;
                public float Delay {
                    get { return delay; }
                    set { delay = value; }
                }


                Coroutine pointerDownCoroutine;
                public override void OnPointerUp(PointerEventData eventData)
                {
                    base.OnPointerUp(eventData);

                    if (pointerDownCoroutine!=null)
                        StopCoroutine(pointerDownCoroutine);
                }

                public override void OnPointerDown(PointerEventData eventData)
                {
                    base.OnPointerDown(eventData);

                    pointerDownCoroutine = StartCoroutine(PressCoroutine(eventData));
                }

                IEnumerator PressCoroutine(PointerEventData eventData)
                {
                    if (onPressHold == null)  yield break;
                    while (onPressHold(eventData))
                    {
                        yield return new WaitForSeconds(Delay);
                    }
                }

                private void OnApplicationPause(bool pause)
                {
                    if(pause)
                        if (pointerDownCoroutine != null)
                            StopCoroutine(pointerDownCoroutine);
                }
            }
        }
    }
}

