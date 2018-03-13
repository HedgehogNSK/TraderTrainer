using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
            Camera camera;
            public Camera objCamera;
            Transform cameraTransform;
            float xBounds = 0.0f;
            float yBounds =0.8f;
            float speed = 5f;
            float scrollLimit = 100;
            Vector2 screen;
            private void Start()
            {
                camera = Camera.main;
                cameraTransform = camera.transform;
                screen = new Vector2(Screen.width, Screen.height);
            }
            public void OnDrag(PointerEventData eventData)
            {
                Vector3 shift = new Vector2(eventData.delta.x / Screen.width, eventData.delta.y / Screen.height);
                cameraTransform.position -= shift* speed * camera.orthographicSize;
            }

            public void OnScroll(PointerEventData eventData)
            {
                camera.orthographicSize -= eventData.scrollDelta.y;
                if (camera.orthographicSize > 100) camera.orthographicSize = 100;
                objCamera.orthographicSize = camera.orthographicSize;
            }

        }
    }

}
