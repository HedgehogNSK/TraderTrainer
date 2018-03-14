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
            [SerializeField] Camera objCamera;
            [SerializeField] float scrollMaxLimit = 25;
            [SerializeField] float scrollMinLimit = 2;

            Transform cameraTransform;
            float xBounds = 0.0f;
            float yBounds =0.8f;
            float speed = 5f;
            
            Vector2 screen;
            private void Start()
            {
                camera = Camera.main;
                cameraTransform = camera.transform;
                screen = new Vector2(Screen.width, Screen.height);
            }
            public void OnDrag(PointerEventData eventData)
            {
                //Это сработает только для камеры параллельной оси Х
                
                Vector3 shift = new Vector2(eventData.delta.x / Screen.width, eventData.delta.y / Screen.height) * speed * camera.orthographicSize;
                if (ChartDrawer.Instance.IsPointToFar(cameraTransform.position - shift))
                {
                    //if (ChartDrawer.Instance.IsPointToFar(cameraTransform.position))
                    
                    return;
                }
                    cameraTransform.position -= shift;
                
            }

            public void OnScroll(PointerEventData eventData)
            {
                camera.orthographicSize -= eventData.scrollDelta.y;
                if (camera.orthographicSize > scrollMaxLimit) camera.orthographicSize = scrollMaxLimit;
                else if (camera.orthographicSize < scrollMinLimit) camera.orthographicSize = scrollMinLimit;
                objCamera.orthographicSize = camera.orthographicSize;
            }

        }
    }

}
