using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;
using Chart.Entity;
namespace Chart
{
    public class ChartDrawer : MonoBehaviour, IChartDrawer
    {
        #region SINGLETON
        static ChartDrawer _instance;
        public static ChartDrawer Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<ChartDrawer>();
                }
                return _instance;
            }
        }
        #endregion

        float scaleY;
        long scaleX;
        float frameCount;
        struct ScreenViewField
        {
            public float price0;
            public float price1;
            public long ticks0;
            public long ticks1;
        }
        ScreenViewField visibleRange;

        [SerializeField] TextPoolManager dateTextPool;
        [SerializeField] TextPoolManager priceTextPool;


        IChartDataManager chartDataManager;
        public IChartDataManager ChartDataManager
        {
            get { return chartDataManager; }
            set { chartDataManager = value; }
        }

        public IGrid CoordGrid { get; set; }
        public Color baseColor;
        public Color crossColor;
        [SerializeField] Candle candleDummy;
        [SerializeField] Transform candlesParent;

        Camera cam;

        Vector3 leftDownCorner;
        //Vector3 leftUpCorner = new Vector3(0, 1);
        //Vector3 rightDownCorner = new Vector3(1,0);
        Vector3 rightUpCorner;
        Vector3 cachedZero = Vector3.zero;
        Vector3 cachedOne = Vector2.one;
        int periodDevider = 5;


        List<Candle> candles = new List<Candle>();
        List<PriceFluctuation> fluctuationList = new List<PriceFluctuation>();
        List<float> dateList = new List<float>();
        //List<float> priceList = new List<float>();

        void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null )
                Debug.Log("ChartDrawer должен знать на какую камеру рисовать");

        }
        void Update()
        {
            DrawChart();
        }
        // Update is called once per frame
        void OnPostRender()
        {
            DrawGrid();
        }

        public bool IsSettingsSet
        {
            get
            {
                if (ChartDataManager != null && CoordGrid!=null && candleDummy != null && candlesParent != null)
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
            if (!IsSettingsSet) return;

            leftDownCorner = cam.ViewportToWorldPoint(cachedZero);
            rightUpCorner = cam.ViewportToWorldPoint(cachedOne);

            DrawTools.LineColor = baseColor;
            DrawTools.dashLength = 0.025f;
            DrawTools.gap = 0.03f;


            //Вывод цен на экран
            decimal lowestPrice = (decimal)CoordGrid.FromYAxisToPrice(leftDownCorner.y);
            decimal highestPrice = (decimal)CoordGrid.FromYAxisToPrice(rightUpCorner.y);
            List<decimal> pricesList = Ariphmetic.DividePriceRangeByKeyPoints(lowestPrice, highestPrice, priceTextPool.FieldsAmount);
            priceTextPool.CleanPool();

            float yPoint;
            foreach (var price in pricesList)
            {
                yPoint = CoordGrid.FromPriceToYAxis((float)price);
                priceTextPool.SetText(
                    price.ToString("F8"),
                    cam.WorldToScreenPoint(new Vector2(0, yPoint)).y,
                    TextPoolManager.ShiftBy.Vertical
                    );
                Vector2 pricePoint1 = new Vector2(0, cam.WorldToViewportPoint(new Vector2(0, yPoint)).y);
                Vector2 pricePoint2 = new Vector2(1, cam.WorldToViewportPoint(new Vector2(0, yPoint)).y);

                DrawTools.DrawLine(pricePoint1, pricePoint2, cam.orthographicSize, true, cam.aspect);
            }



            //Вычисляем точки на временной сетке и отрисовываем их
            DateTime dt0 = CoordGrid.FromXAxisToDate((int)cam.ViewportToWorldPoint(cachedZero).x);
            DateTime dt1 = CoordGrid.FromXAxisToDate((int)cam.ViewportToWorldPoint(cachedOne).x);

            if (CoordGrid is CoordinateGrid)
                dt0 = (CoordGrid as CoordinateGrid).DateCorrection(dt0, dt1);

            var dateList = DateTimeTools.DividePeriodByKeyPoints(dt0, dt1, dateTextPool.FieldsAmount);
            dateTextPool.CleanPool();
            foreach (var date in dateList)
            {
                Vector2 dateLine = new Vector2(CoordGrid.FromDateToXAxis(date), 0);

                dateTextPool.SetText(
                    date.ChartStringFormat(),
                    cam.WorldToScreenPoint(dateLine).x,
                    TextPoolManager.ShiftBy.Horizontal
                    );

                Vector2 datePoint1 = new Vector2(cam.WorldToViewportPoint(dateLine).x, 0);
                Vector2 datePoint2 = new Vector2(cam.WorldToViewportPoint(dateLine).x, 1);
                DrawTools.DrawLine(datePoint1, datePoint2, cam.orthographicSize, true, cam.aspect);
            }
        }

       
        public void DownloadFluctuations()
        {
            if (!IsSettingsSet) return;

            leftDownCorner = cam.ViewportToWorldPoint(cachedZero);
            rightUpCorner = cam.ViewportToWorldPoint(cachedOne);
            float screenLength = rightUpCorner.x- leftDownCorner.x;
            screenLength = screenLength < 100 ? screenLength : 100;
            TimeFrame timeFrame = chartDataManager.TFrame;
            DateTime visibleStartDate = CoordGrid.FromXAxisToDate(leftDownCorner.x- screenLength).FloorToTimeFrame(timeFrame);
            DateTime visibleEndDate = CoordGrid.FromXAxisToDate(rightUpCorner.x+ screenLength).UpToNextFrame(timeFrame);
            if (chartDataManager.ChartEndTime < visibleEndDate)
                visibleEndDate = chartDataManager.ChartEndTime;
            if (chartDataManager.ChartBeginTime > visibleStartDate)
                visibleStartDate = chartDataManager.ChartBeginTime;

            var onScreenFluctuations = fluctuationList.Where(fluctuation => fluctuation.PeriodBegin >= visibleStartDate && fluctuation.PeriodBegin <= visibleEndDate);
            var existDatePoints = onScreenFluctuations.Select(c => c.PeriodBegin);

            IEnumerable<PriceFluctuation> fluctuations = new List<PriceFluctuation>();
            if (visibleStartDate < visibleEndDate)
            {
                if (existDatePoints.NotNullOrEmpty())
                {
                    var point1 = existDatePoints.Min();
                    var point2 = existDatePoints.Max();

                    if (point1.FloorToTimeFrame(timeFrame) != visibleStartDate)
                        fluctuations = ChartDataManager.GetPriceFluctuationsByTimeFrame(visibleStartDate, point1);

                    if (point2.UpToNextFrame(timeFrame) < visibleEndDate)
                        fluctuations = fluctuations.Union(ChartDataManager.GetPriceFluctuationsByTimeFrame(point2, visibleEndDate));
                }

               else
                {
                    fluctuations = ChartDataManager.GetPriceFluctuationsByTimeFrame(visibleStartDate, visibleEndDate);
                }

                if(fluctuations.NotNullOrEmpty())
                    fluctuationList.Concat(fluctuations);

            }
        }

        public void DrawChart()
        {
            if (!IsSettingsSet) return;
            leftDownCorner = cam.ViewportToWorldPoint(cachedZero);
            rightUpCorner = cam.ViewportToWorldPoint(cachedOne);

            TimeFrame timeFrame = chartDataManager.TFrame;
            DateTime visibleStartDate = CoordGrid.FromXAxisToDate(leftDownCorner.x).FloorToTimeFrame(timeFrame);
            DateTime visibleEndDate = CoordGrid.FromXAxisToDate(rightUpCorner.x).UpToNextFrame(timeFrame);
            if (chartDataManager.ChartEndTime < visibleEndDate)
                visibleEndDate = chartDataManager.ChartEndTime;
            if (chartDataManager.ChartBeginTime > visibleStartDate)
                visibleStartDate = chartDataManager.ChartBeginTime;

            var candlesInScreen = candles.Where(candle => candle.PeriodBegin >= visibleStartDate && candle.PeriodBegin <= visibleEndDate);
            var existDatePoints = candlesInScreen.Select(c => c.PeriodBegin);

            IEnumerable<PriceFluctuation> fluctuations = new List<PriceFluctuation>();
            if (visibleStartDate < visibleEndDate)
            {
                if (existDatePoints.NotNullOrEmpty())
                {
                    var point1 = existDatePoints.Min();
                    var point2 = existDatePoints.Max();

                    if (point1.FloorToTimeFrame(timeFrame) != visibleStartDate)
                        fluctuations = ChartDataManager.GetPriceFluctuationsByTimeFrame(visibleStartDate, point1);

                    if (point2.UpToNextFrame(timeFrame) <= visibleEndDate)
                        fluctuations = fluctuations.Union(ChartDataManager.GetPriceFluctuationsByTimeFrame(point2, visibleEndDate));
                }

               else
                {
                    fluctuations = ChartDataManager.GetPriceFluctuationsByTimeFrame(visibleStartDate, visibleEndDate);
                }

                foreach (var priceFluctuation in fluctuations)
                {
                    Candle newCandle = Instantiate(candleDummy, candlesParent);
                    newCandle.Grid = CoordGrid;
                    newCandle.Set(CoordGrid.FromDateToXAxis(priceFluctuation.PeriodBegin), priceFluctuation);
                    candles.Add(newCandle);

                }
            }

        }

        public void DrawCross()
        {

            Vector2 pointerScreenPosition;
            Vector3 pointerWolrdPosition;
            GetWorldPointerPosition(out pointerScreenPosition, out pointerWolrdPosition);
            Vector2 pointerViewportPosition = cam.ScreenToViewportPoint(pointerScreenPosition);
            DrawTools.LineColor = crossColor;
            DrawTools.dashLength = 0.03f;
            DrawTools.gap = 0.04f;

            Vector2 camToViewportFromXToTop = cam.ScreenToViewportPoint(new Vector2(pointerScreenPosition.x, cam.pixelHeight));
            Vector2 camToViewportFromXToBottom = cam.ScreenToViewportPoint(new Vector2(pointerScreenPosition.x, 0));

            //Магнитизм к X. Необходимо будет переписать

            DrawTools.DrawLine(pointerViewportPosition, camToViewportFromXToTop, cam.orthographicSize * 1.1f, true, cam.aspect);
            DrawTools.DrawLine(pointerViewportPosition, camToViewportFromXToBottom, cam.orthographicSize * 1.1f, true, cam.aspect);
            DrawTools.DrawLine(pointerViewportPosition, cam.ScreenToViewportPoint(new Vector2(0, pointerScreenPosition.y)), cam.orthographicSize, true, cam.aspect);
            DrawTools.DrawLine(pointerViewportPosition, cam.ScreenToViewportPoint(new Vector2(cam.pixelWidth, pointerScreenPosition.y)), cam.orthographicSize, true, cam.aspect);
        }

        void OnGUI()
        {
            Vector2 pointerScreenPosition;
            Vector3 pointerWolrdPosition;
            GetWorldPointerPosition(out pointerScreenPosition, out pointerWolrdPosition);

            GUILayout.BeginArea(new Rect(20, 50, 250, 120));
            GUILayout.Label("Screen pixels: " + cam.pixelWidth + ":" + cam.pixelHeight);
            GUILayout.Label("Mouse position: " + pointerScreenPosition);
            GUILayout.Label("World position: " + pointerWolrdPosition.ToString("F3"));
            GUILayout.EndArea();

        }

        private void GetWorldPointerPosition(out Vector2 pointerScreenPosition, out Vector3 pointerWolrdPosition)
        {

#if UNITY_EDITOR || UNITY_WEBGL
            pointerScreenPosition = Input.mousePosition;
#endif
#if UNITY_EDITOR || UNITY_IPHONE || UNITY_ANDROID
#endif

            pointerWolrdPosition = cam.ScreenToWorldPoint(new Vector3(pointerScreenPosition.x, pointerScreenPosition.y, cam.nearClipPlane));
        }


        public bool IsDateToFar(Vector3 point)
        {
            float x0 = CoordGrid.FromDateToXAxis(ChartDataManager.ChartBeginTime);
            float x1 = CoordGrid.FromDateToXAxis(ChartDataManager.ChartEndTime);          
            //Debug.Log("BeginTime:" + x0 + " EndTime:" + x1);
            if (point.x < x0 || point.x > x1) return true;
            return false;
        }

    }
}