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
            public class AdvancedButton : MonoBehaviour, IPointerDownHandler,IPointerUpHandler
            {

                public event Func<PointerEventData,bool> onPressHold;
                [SerializeField] float delay = 0.3f;
                public float Delay {
                    get { return delay; }
                    set { delay = value; }
                }


                Coroutine pointerDownCoroutine;
                public void OnPointerUp(PointerEventData eventData)
                {
                  if (pointerDownCoroutine!=null)
                        StopCoroutine(pointerDownCoroutine);
                }

                public void OnPointerDown(PointerEventData eventData)
                {
                  pointerDownCoroutine = StartCoroutine(PressCoroutine(eventData));
                }

                IEnumerator PressCoroutine(PointerEventData eventData)
                {
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

