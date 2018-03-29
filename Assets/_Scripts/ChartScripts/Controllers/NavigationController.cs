using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Linq;

namespace Chart
{
    namespace Controllers
    {
      
        [RequireComponent(typeof(UnityEngine.UI.Image))]
        public class NavigationController : MonoBehaviour, IDragHandler, IScrollHandler
        {
            static NavigationController _instance;
            public static NavigationController Instance
            {
                get { if(!_instance)
                    {
                        _instance = FindObjectOfType<NavigationController>();
                    }
                    return _instance;
                }
            }
            Camera cam;
            [SerializeField] Camera objCamera;
            [SerializeField] float scrollMaxLimit = 25;
            [SerializeField] float scrollMinLimit = 2;
            [SerializeField] float defaultOrthoSize = 10;

            Transform cameraTransform;
            //float xBounds = 0.0f;
            //float yBounds =0.8f;
            float speed = 5f;
            
            //Vector2 screen;
            private void Start()
            {
                cam = Camera.main;
                cameraTransform = cam.transform;
                //screen = new Vector2(Screen.width, Screen.height);
                Initialize();
            }
            public void OnDrag(PointerEventData eventData)
            {
                //Это сработает только для камеры параллельной оси Х
                
                Vector3 shift = new Vector2(eventData.delta.x / Screen.width, eventData.delta.y / Screen.height) * speed * cam.orthographicSize;
                if (ChartDrawer.Instance.IsPointToFar(cameraTransform.position - shift))
                {  
                   // return;
                }
                    cameraTransform.position -= shift;
                
            }

            public void OnScroll(PointerEventData eventData)
            {
                cam.orthographicSize -= eventData.scrollDelta.y;
                if (cam.orthographicSize > scrollMaxLimit) cam.orthographicSize = scrollMaxLimit;
                else if (cam.orthographicSize < scrollMinLimit) cam.orthographicSize = scrollMinLimit;
                objCamera.orthographicSize = cam.orthographicSize;
            }

            public void GoToLastPoint()
            {
                cameraTransform.position = (Vector3)ChartDrawer.Instance.GetLastPoint()  + Vector3.forward*cameraTransform.position.z;

                //Смещение на удобный обзор
                cameraTransform.position -= Vector3.right *cam.orthographicSize * cam.aspect * 0.4f;
            }

            public void Initialize()
            {             
                cam.orthographicSize = defaultOrthoSize;
                objCamera.orthographicSize = defaultOrthoSize;
                GoToLastPoint();
            }

        }
    }

}
