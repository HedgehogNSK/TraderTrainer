using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;
using Chart.Entity;
namespace Chart
{
    public class ChartDrawer : MonoBehaviour, IChartDrawer {

        static ChartDrawer _instance;
        public static ChartDrawer Instance
        {
            get
            {
                if(!_instance)
                {
                    _instance = FindObjectOfType<ChartDrawer>();
                }
                return _instance;
            }
        }
        IChartDataManager chartDataManager;
        public IChartDataManager ChartDataManager
        {
            get { return chartDataManager; }
            set
            {
                chartDataManager = value;
                CoordinateGrid.ZeroPoint = chartDataManager.ChartBeginTime;
                CoordinateGrid.Step = chartDataManager.TFrame;
            }
        }
        public Color baseColor;
        public Color crossColor;
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
        List<float> dateList = new List<float>();
        List<float> priceList = new List<float>();

        void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null || ChartDataManager == null)
                Debug.Log("Повесь скрипт на камеру и задайте все параметры");

        }
        // Use this for initialization
        void Start()
        {           
        }

        // Update is called once per frame
        void OnPostRender()
        {
            DrawGrid();
            //DrawCross();
        }

        private void OnDrawGizmos()
        {
            if (!cam) cam = GetComponent<Camera>();
            DrawGrid();
        }

        public bool IsSettingsSet {
            get {
                if (ChartDataManager != null && candleDummy != null && candlesParent != null)
                    return true;
                else
                {
                    Debug.Log("ChartDrawer не может выполнить действие, пока не заданы все параметры");
                    return false;
                }
            }
        }
        public void DrawGrid()
        {
            //Debug.Log(cam.ViewportToWorldPoint(cachedOne));
            leftDownCorner = cam.ViewportToWorldPoint(cachedZero);
            rightUpCorner = cam.ViewportToWorldPoint(cachedOne);
            float xLenght = rightUpCorner.x - leftDownCorner.x;
            float yLenght = rightUpCorner.y - leftDownCorner.y;

            float xShift = xLenght / periodDevider;
            float yShift = yLenght / periodDevider;

            DrawTools.LineColor = baseColor;
            DrawTools.dashLength = 0.02f;
            DrawTools.gap = 0.03f;
            dateList.Clear();
            for (int i = 0; i != periodDevider + 1; i++)
            {
               int xPoint = (int)(leftDownCorner.x + i * xShift);
               int yPoint = (int)(leftDownCorner.y + i * yShift);

                dateList.Add(xPoint);
               DrawTools.DrawLine(new Vector2(xPoint, leftDownCorner.y), new Vector2(xPoint, rightUpCorner.y), cam.orthographicSize, true);
               DrawTools.DrawLine(new Vector2(leftDownCorner.x, yPoint), new Vector2(rightUpCorner.x, yPoint), cam.orthographicSize, true);
            }
        }



        public void DrawChart()
        {
            int id = 0;
            foreach (var priceFluctuation in ChartDataManager.GetPriceFluctuationsByTimeFrame(ChartDataManager.ChartBeginTime, ChartDataManager.ChartEndTime))
            {
                Candle newCandle = Instantiate(candleDummy, candlesParent);
                newCandle.Set(FromDateToPosition(priceFluctuation.PeriodBegin), priceFluctuation);
                candles.Add(newCandle);
                id++;
            }
        }

        public void DrawCross()
        {

            Vector2 pointerScreenPosition;
            Vector3 pointerWolrdPosition;
            GetWorldPointerPosition(out pointerScreenPosition, out pointerWolrdPosition);
            
            DrawTools.LineColor = crossColor;
            DrawTools.dashLength = 0.03f;
            DrawTools.gap = 0.05f;

            Vector2 camToWorldFromXToTop = cam.ScreenToWorldPoint(new Vector2(pointerScreenPosition.x, cam.pixelHeight));
            Vector2 camToWorldFromXToBottom = cam.ScreenToWorldPoint(new Vector2(pointerScreenPosition.x, 0));

            //Магнитизм к X. Необходимо будет переписать
            if (true)
            {
                pointerWolrdPosition = new Vector2(Mathf.RoundToInt(pointerWolrdPosition.x), pointerWolrdPosition.y);
                camToWorldFromXToTop = new Vector2(Mathf.RoundToInt(camToWorldFromXToTop.x), camToWorldFromXToTop.y);
                camToWorldFromXToBottom = new Vector2(Mathf.RoundToInt(camToWorldFromXToBottom.x), camToWorldFromXToBottom.y);
            }

            DrawTools.DrawLine(pointerWolrdPosition, camToWorldFromXToTop, cam.orthographicSize*1.1f, true);
            DrawTools.DrawLine(pointerWolrdPosition, camToWorldFromXToBottom, cam.orthographicSize * 1.1f, true);
            DrawTools.DrawLine(pointerWolrdPosition,cam.ScreenToWorldPoint(new Vector2(0, pointerScreenPosition.y)), cam.orthographicSize, true);
            DrawTools.DrawLine(pointerWolrdPosition,cam.ScreenToWorldPoint(new Vector2(cam.pixelWidth, pointerScreenPosition.y)), cam.orthographicSize, true);
        } 

        void OnGUI()
        {
            Vector2 pointerScreenPosition;
            Vector3 pointerWolrdPosition;
            GetWorldPointerPosition(out pointerScreenPosition, out pointerWolrdPosition);

            GUILayout.BeginArea(new Rect(20, 20, 250, 120));
            GUILayout.Label("Screen pixels: " + cam.pixelWidth + ":" + cam.pixelHeight);
            GUILayout.Label("Mouse position: " + pointerScreenPosition);
            GUILayout.Label("World position: " + pointerWolrdPosition.ToString("F3"));
            GUILayout.EndArea();

            foreach(var date in dateList)
            {
                //Debug.Log(cam.WorldToScreenPoint(new Vector3(date,0,0)).x);
                GUILayout.BeginArea(new Rect(cam.WorldToScreenPoint(new Vector3(date, 0, 0)).x - 50,0, 200, 200));
                GUILayout.Label(FromPositionToDate(date).ToShortDateString() +" "+ FromPositionToDate(date).Hour+":"+ FromPositionToDate(date).Minute);
                //cam.WorldToScreenPoint(new Vector3(price, 0, 0)).
                GUILayout.EndArea();
            }
        }

        private void GetWorldPointerPosition(out Vector2 pointerScreenPosition, out Vector3 pointerWolrdPosition)
        {

#if UNITY_EDITOR || UNITY_WEBGL
            pointerScreenPosition = Input.mousePosition;
#endif
#if UNITY_EDITOR || UNITY_IPHONE || UNITY_ANDROID
#endif

            pointerWolrdPosition =  cam.ScreenToWorldPoint(new Vector3(pointerScreenPosition.x, pointerScreenPosition.y, cam.nearClipPlane));
        }

        int FromDateToPosition(DateTime dt)
        {
            TimeFrame tFrame = ChartDataManager.TFrame;
            if (startingPoint == DateTime.MinValue)
            {
                startingPoint = dt.FloorToTimeFrame(tFrame);
                return 0;
            }

            return DateTimeTools.CountFramesInPeriod(tFrame, startingPoint, dt, TimeSpan.Zero);
        }

        DateTime FromPositionToDate(float dt)
        {
            TimeFrame tFrame = ChartDataManager.TFrame;
            if(candles[0])
            {
                Candle candle = candles[0];
                int shift = (int)(dt -candle.transform.position.x );
                return candle.PeriodBegin + tFrame * shift;
            }

            throw new ArgumentNullException("На графике нет свечей");
        }

        public bool IsPointToFar(Vector3 point)
        {
            float x0 = CoordinateGrid.FromDateToXAxis(ChartDataManager.ChartBeginTime);
            float x1 = CoordinateGrid.FromDateToXAxis(ChartDataManager.ChartEndTime);
            //Debug.Log("BeginTime:" + x0 + " EndTime:" + x1);
            if (point.x < x0 || point.x > x1) return true;
            return false;
        }
    }
}