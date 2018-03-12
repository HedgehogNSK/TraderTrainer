using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;
using System;
using Chart.Entity;
namespace Chart
{
    public class ChartDrawer : MonoBehaviour, IChartDrawer {

        public IChartViewer chartViewer;
        public Color baseColor;
        [SerializeField] Candle candleDummy;
        [SerializeField] Transform candlesParent;

        Camera cam;

        Vector3 leftDownCorner;
        Vector3 leftUpCorner = new Vector3(0, 1);
        Vector3 rightDownCorner = new Vector3(1,0);
        Vector3 rightUpCorner;
        Vector3 cachedZero = Vector3.zero;
        Vector3 cachedOne = Vector2.one;
        int periodDevider = 5;

        DateTime startingPoint = DateTime.MinValue;

        List<Candle> candles = new List<Candle>();
       
        public bool IsSettingsSet {
            get {
                if (chartViewer != null && candleDummy != null && candlesParent != null)
                    return true;
                else
                {
                    Debug.Log("ChartDrawer не может выполнить действие, пока не заданы все параметры");
                    return false;
                }
            }
        }
        public void DrawGizmos()
        {
            //Debug.Log(cam.ViewportToWorldPoint(cachedOne));
            leftDownCorner = cam.ViewportToWorldPoint(cachedZero);
            rightUpCorner = cam.ViewportToWorldPoint(cachedOne);
            float xLenght = rightUpCorner.x - leftDownCorner.x;
            float yLenght = rightUpCorner.y - leftDownCorner.y;

            float xShift = xLenght / periodDevider;
            float yShift = yLenght / periodDevider;
            for (int i = 0; i != periodDevider + 1; i++)
            {
                int xPoint = (int)(leftDownCorner.x + i * xShift);
                int yPoint = (int)(leftDownCorner.y + i * yShift);
               DrawTools.DrawLine(new Vector2(xPoint, leftDownCorner.y), new Vector2(xPoint, rightUpCorner.y), cam.orthographicSize, true);
               DrawTools.DrawLine(new Vector2(leftDownCorner.x, yPoint), new Vector2(rightUpCorner.x, yPoint), cam.orthographicSize, true);
            }
        }

        void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null || chartViewer == null)
                Debug.Log("Повесь скрипт на камеру и задайте все параметры");
          
        }
        // Use this for initialization
        void Start() {
            DrawTools.SetProperties(baseColor);     
        }

        // Update is called once per frame
        void OnPostRender() {
            DrawGizmos();
        }

        private void OnDrawGizmos()
        {
            if(!cam) cam = GetComponent<Camera>();
            DrawGizmos();
        }

        public void DrawChart()
        {
            int id = 0;
            foreach (var priceFluctuation in chartViewer.GetPriceFluctuationsByTimeFrame(chartViewer.ChartBeginTime, chartViewer.ChartEndTime))
            {
                Candle newCandle = Instantiate(candleDummy, candlesParent);
                newCandle.Set(FromDateToPosition(priceFluctuation.PeriodDateBegin), priceFluctuation);
                candles.Add(newCandle);
                id++;
            }
        }
        int FromDateToPosition(DateTime dt)
        {
            TimeFrame tFrame = chartViewer.TFrame;
            if (startingPoint == DateTime.MinValue)
            {
                startingPoint = dt.FloorToTimeFrame(tFrame);
                return 0;
            }

            return DateTimeTools.CountFramesInPeriod(tFrame, startingPoint, dt, TimeSpan.Zero);            
        }


    }
}