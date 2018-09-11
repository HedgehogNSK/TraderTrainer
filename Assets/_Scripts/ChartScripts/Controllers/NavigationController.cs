using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using Hedge.Tools;
using Chart.Entity;
using ChartGame;
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
            [SerializeField] float camShift = 0.1f;

            Transform cameraTransform;
            //float xBounds = 0.0f;
            //float yBounds =0.8f;
            float speed = 5f;
            float cachedXPos;
            public event Action<PriceFluctuation> OnFluctuationSelect;

            private IGrid coordGrid;
            public IGrid CoordGrid {
                get { return coordGrid; }
                set
                {
                    if(CoordGrid!=null)
                        CoordGrid.OnScaleChange -= ShiftCamera;
                    coordGrid = value;
                    if(coordGrid!=null)
                        coordGrid.OnScaleChange += ShiftCamera;

                }
            }
            IScalableDataManager chartDataManager;
            public IScalableDataManager ChartDataManager
            {
                get { return chartDataManager; }
                set
                {
                    chartDataManager = value;
                }
            }
            public bool IsSettingsSet
            {
                get
                {
                    if (ChartDataManager != null && CoordGrid != null)
                        return true;
                    else
                    {
                        Debug.Log("Navigation Controller не может выполнить действие, пока не заданы все параметры");
                        return false;
                    }
                }
            }

            private void Awake()
            {
                cam = Camera.main;
                cameraTransform = cam.transform;
            }

            public void OnDrag(PointerEventData eventData)
            {
                //Это сработает только для камеры параллельной оси Х
                
                Vector3 shift = new Vector2(eventData.delta.x / Screen.width, eventData.delta.y / Screen.height) * speed * cam.orthographicSize;

                //Проверка выхода за границы
                if (!CanMoveFurther(cameraTransform.position,-shift))
                {  
                    return;
                }

                if (ChartDrawer.Instance.Autoscale)
                {
                    cameraTransform.position -= Vector3.right*shift.x;
                }
                else
                {
                    cameraTransform.position -= shift;
                }
                
            }

            public void OnScroll(PointerEventData eventData)
            {
                Vector2 shift = cam.ScreenToWorldPoint(eventData.pointerCurrentRaycast.screenPosition);
                cam.orthographicSize -= eventData.scrollDelta.y* cam.orthographicSize/10;
                if (cam.orthographicSize > scrollMaxLimit) cam.orthographicSize = scrollMaxLimit;
                else if (cam.orthographicSize < scrollMinLimit) cam.orthographicSize = scrollMinLimit;
                objCamera.orthographicSize = cam.orthographicSize;

                shift -= (Vector2)cam.ScreenToWorldPoint(eventData.pointerCurrentRaycast.screenPosition);
                
                cameraTransform.position += (Vector3)shift;
            }

            public void Update()
            {
                
                Vector3 pointerWorldPos;
                Vector2 pointerScreenPos;
                ChartDrawer.Instance.GetWorldPointerPosition(out pointerScreenPos, out pointerWorldPos);
                if(cachedXPos != Mathf.Round(pointerWorldPos.x))
                {
                    cachedXPos = Mathf.Round(pointerWorldPos.x);
                    DateTime fluctuationDate =CoordGrid.FromXAxisToDate(cachedXPos);
                    if(OnFluctuationSelect!=null)
                        OnFluctuationSelect(ChartDataManager.GetPriceFluctuation(fluctuationDate));
                    
                }
            }

            internal Vector3 GetLastPoint()
            {
                if (!IsSettingsSet) return Vector3.zero;

              
                float x = CoordGrid.FromDateToXAxis(chartDataManager.WorkEndTime);
                float y;
                if (ChartDrawer.Instance.Autoscale)
                y = cameraTransform.position.y;
                else
                y = CoordGrid.FromPriceToYAxis((float)chartDataManager.GetPriceFluctuation(chartDataManager.WorkEndTime).Close);
                    
                return new Vector3(x, y);
            }

            public void GoToLastPoint()
            {
                cameraTransform.position = GetLastPoint()  + Vector3.forward*cameraTransform.position.z;
                //Смещение фокуса, искомая свеча была не по центру
                cameraTransform.position -= Vector3.right *cam.orthographicSize * cam.aspect * camShift;
            }
            public void ShiftCamera(float mult)
            {

                cameraTransform.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y* mult, cameraTransform.position.z);
            }

            
            public bool CanMoveFurther(Vector3 fromPoint, Vector3 byStep)
            {
                float x0 = CoordGrid.FromDateToXAxis(ChartDataManager.WorkBeginTime);
                float x1 = CoordGrid.FromDateToXAxis(ChartDataManager.WorkEndTime);

                Vector3 nextpoint = fromPoint + byStep;
                if ((nextpoint.x>x0 && nextpoint.x < x1) 
                    ||(nextpoint.x <= x0&& byStep.x >=0 )
                    ||(nextpoint.x >= x1 && byStep.x <= 0)) return true;
                return false;
            }

            public void Initialize()
            {
                
                cam.orthographicSize = defaultOrthoSize;
                objCamera.orthographicSize = defaultOrthoSize;              
                GoToLastPoint();
            }

            private void OnDestroy()
            {
               if(GameManager.Instance) GameManager.Instance.GoToNextFluctuation -= GoToLastPoint;
            }


        }
    }

}
