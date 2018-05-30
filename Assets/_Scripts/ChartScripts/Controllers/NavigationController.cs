﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using Hedge.Tools;
using Chart.Entity;
namespace Chart
{
    using Managers;
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
            Vector2 cachedZero = Vector2.zero;
            Vector2 cachedOne = Vector2.one;

            private IGrid coordGrid;
            public IGrid CoordGrid {
                get { return coordGrid; }
                set
                {
                    if(CoordGrid!=null)
                        CoordGrid.OnScaleChange -= ShiftCamera;
                    coordGrid = value;

                }
            }
            IChartDataManager chartDataManager;
            public IChartDataManager ChartDataManager
            {
                get { return chartDataManager; }
                set
                {
                    chartDataManager = value;
                }
            }
            public bool autoscale = false;
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
            private void Start()
            {               
                Initialize();
            }
            public void OnDrag(PointerEventData eventData)
            {
                //Это сработает только для камеры параллельной оси Х
                
                Vector3 shift = new Vector2(eventData.delta.x / Screen.width, eventData.delta.y / Screen.height) * speed * cam.orthographicSize;

                //Проверка выхода за границы
                if (ChartDrawer.Instance.IsDateToFar(cameraTransform.position - shift))
                {  
                    return;
                }

                if (autoscale)
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

            internal Vector3 GetLastPoint()
            {
                if (!IsSettingsSet) return Vector3.zero;

                DateTime endTime = ChartDataManager.ChartBeginTime  + (GameManager.Instance.firstFluctuationID + GameManager.Instance.fluctuationsCountToLoad) * ChartDataManager.TFrame;                                        //Грузить информацию до определённой свечи
                endTime = endTime < chartDataManager.ChartEndTime ? endTime : chartDataManager.ChartEndTime;            //Проверка, что определённой свеча должна быть не позже имеющейся


                float x = CoordGrid.FromDateToXAxis(endTime);
                float y = CoordGrid.FromPriceToYAxis((float)chartDataManager.GetFluctuation(endTime).Close);
                return new Vector3(x, y);
            }

            public void GoToLastPoint()
            {
                cameraTransform.position = GetLastPoint()  + Vector3.forward*cameraTransform.position.z;
                //Смещение фокуса, искомая свеча была не по центру
                cameraTransform.position -= Vector3.right *cam.orthographicSize * cam.aspect * 0.4f;
            }
            public void ShiftCamera(float mult)
            {
                cameraTransform.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y* mult, cameraTransform.position.z);
            }
            public void Initialize()
            {
                CoordGrid.OnScaleChange += ShiftCamera;
                cam.orthographicSize = defaultOrthoSize;
                objCamera.orthographicSize = defaultOrthoSize;
                GameManager.Instance.GoToNextFluctuation += GoToLastPoint;
                GoToLastPoint();
            }

            private void OnDestroy()
            {
                GameManager.Instance.GoToNextFluctuation -= GoToLastPoint;
            }

        }
    }

}
