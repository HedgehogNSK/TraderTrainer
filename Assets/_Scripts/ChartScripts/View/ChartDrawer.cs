using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hedge.Tools;
using Chart.Entity;

namespace Chart
{
    using Managers;
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

        //Отставание графика от вертикальных границ
        public float chartOffsetFromVerticalBorders = 1.05f;
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
        [SerializeField] Toggle autoscaleToggle;
        IChartDataManager chartDataManager;
        public IChartDataManager ChartDataManager
        {
            get { return chartDataManager; }
            set { chartDataManager = value; }
        }

        public IGrid CoordGrid { get; set; }
        public Color baseColor;
        public Color crossColor;
        public Color volumeUpColor;
        public Color volumeDownColor;

        [SerializeField] Candle candleDummy;
        [SerializeField] Transform candlesParent;

        Camera cam;

        Vector3 worldPointInLeftDownCorner;
        Vector3 worldPointInRightUpCorner;
        Vector3 camPreviousPosition;
        float orthographicSizePrevious;
        float previousScale =0;
        Vector3 cachedZero = Vector3.zero;
        Vector3 cachedOne = Vector3.one;
        bool autoscale = false;
        bool autoscaleSwitched = true;
        public bool Autoscale
        {
            get { return autoscale; }
            set {
                if(autoscaleToggle!=null && autoscaleToggle.isOn!=value)
                    autoscaleToggle.isOn = value;
                autoscale = value;             
                autoscaleSwitched = true;
            }
        }

        List<Candle> candles = new List<Candle>();

        void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null )
                Debug.Log("ChartDrawer должен знать на какую камеру рисовать");
        }
        void Start()
        {
            GameManager.Instance.GoToNextFluctuation += UpdateInNextFrame;
            Autoscale = true;
        }

        private void UpdateInNextFrame()
        {
            needToBeUpdated = true;
        }

        void Update()
        {          
            if (NeedToBeUpdated)//Оптимизация
            {

                worldPointInLeftDownCorner = cam.ViewportToWorldPoint(cachedZero);
                worldPointInRightUpCorner = cam.ViewportToWorldPoint(cachedOne);
                if (Autoscale) ScaleChart();
                DrawChart();

            }
        }

        void LateUpdate()
        {
            camPreviousPosition = cam.transform.position;
            orthographicSizePrevious = cam.orthographicSize;
            
        }

        bool needToBeUpdated =false;
        bool NeedToBeUpdated {
            get
            {
               /* if (orthographicSizePrevious != cam.orthographicSize)
                {
                    Autoscale = true;
                    return true;
                }*/
                if (autoscaleSwitched)
                {
                    autoscaleSwitched = false;
                    return true;
                }
                if(Mathf.Abs(CoordGrid.Scale/previousScale-1) >0.0001f)
                {
                    previousScale = CoordGrid.Scale;
                    if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
                    {
                       
                        autoscaleToggle.isOn = false;
                    }
                    return true;
                }

                if (needToBeUpdated) {
                    needToBeUpdated = false;
                    return true;
                }

                return camPreviousPosition != cam.transform.position;
            }
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


            DrawTools.LineColor = baseColor;
            DrawTools.dashLength = 0.05f;
            DrawTools.gap = 0.07f;


            //Вывод цен на экран
            decimal lowestPrice = (decimal)CoordGrid.FromYAxisToPrice(worldPointInLeftDownCorner.y);
            decimal highestPrice = (decimal)CoordGrid.FromYAxisToPrice(worldPointInRightUpCorner.y);
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
                //Vector2 pricePoint1 = new Vector2(0, cam.WorldToViewportPoint(new Vector2(0, yPoint)).y);
                //Vector2 pricePoint2 = new Vector2(1, cam.WorldToViewportPoint(new Vector2(0, yPoint)).y);

                //DrawTools.DrawLine(pricePoint1, pricePoint2, cam.orthographicSize, true, cam.aspect);

                Vector2 pricePoint1 = new Vector2(worldPointInLeftDownCorner.x,  yPoint);
                Vector2 pricePoint2 = new Vector2(worldPointInRightUpCorner.x, yPoint);
               
                DrawTools.DrawLine(pricePoint1, pricePoint2, cam.orthographicSize, true);
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
                //Vector2 dateLine = new Vector2(CoordGrid.FromDateToXAxis(date), 0);
                float dateLine = CoordGrid.FromDateToXAxis(date);
                dateTextPool.SetText(
                    date.ChartStringFormat(),
                    //cam.WorldToScreenPoint(dateLine).x,
                    cam.WorldToScreenPoint(new Vector2(dateLine,0)).x,
                    TextPoolManager.ShiftBy.Horizontal
                    );

                //Vector2 datePoint1 = new Vector2(cam.WorldToViewportPoint(dateLine).x, 0);
                //Vector2 datePoint2 = new Vector2(cam.WorldToViewportPoint(dateLine).x, 1);

                //DrawTools.DrawLine(datePoint1, datePoint2, cam.orthographicSize, true, cam.aspect);

                Vector2 datePoint1 = new Vector2(dateLine, worldPointInLeftDownCorner.y);
                Vector2 datePoint2 = new Vector2(dateLine, worldPointInRightUpCorner.y);

                DrawTools.DrawLine(datePoint1, datePoint2, cam.orthographicSize, true);

            }
        }

        Transform camTransform;
        IEnumerable<PriceFluctuation> fluctuations;

        //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        private void ScaleChart()
        {
            
           
            if (!IsSettingsSet) return;
            
            TimeFrame timeFrame = chartDataManager.TFrame;
            DateTime visibleStartDate = CoordGrid.FromXAxisToDate(worldPointInLeftDownCorner.x).FloorToTimeFrame(timeFrame);
            DateTime visibleEndDate = CoordGrid.FromXAxisToDate(worldPointInRightUpCorner.x).UpToNextFrame(timeFrame);
            fluctuations = ChartDataManager.GetPriceFluctuationsByTimeFrame(visibleStartDate, visibleEndDate);    

            float highestPriceOnScreen = CoordGrid.FromYAxisToPrice(worldPointInRightUpCorner.y);
            float lowestPriceOnScreen = CoordGrid.FromYAxisToPrice(worldPointInLeftDownCorner.y);

            float highestPrice = (float) fluctuations.Max(f => f.High);
            float lowestPrice = (float) fluctuations.Min(f => f.Low);
            
            float priceRange = highestPrice - lowestPrice;
            float new_y = CoordGrid.FromPriceToYAxis(lowestPrice + priceRange / 2);


            if (Mathf.Abs(new_y/cam.transform.position.y-1) >0.001f)
            cam.transform.position = new Vector3(cam.transform.position.x, new_y, cam.transform.position.z);

            CoordGrid.Scale *= (highestPriceOnScreen - lowestPriceOnScreen) / (priceRange * chartOffsetFromVerticalBorders);


        }

        public void DrawChart()
        {
            if (!IsSettingsSet) return;

            TimeFrame timeFrame = chartDataManager.TFrame;
            DateTime visibleStartDate = CoordGrid.FromXAxisToDate(worldPointInLeftDownCorner.x).FloorToTimeFrame(timeFrame);
            DateTime visibleEndDate = CoordGrid.FromXAxisToDate(worldPointInRightUpCorner.x).UpToNextFrame(timeFrame);
            if (chartDataManager.DataEndTime < visibleEndDate)
                visibleEndDate = chartDataManager.DataEndTime;
            if (chartDataManager.DataBeginTime > visibleStartDate)
                visibleStartDate = chartDataManager.DataBeginTime;

            var candlesInScreen = candles.Where(candle => candle.PeriodBegin >= visibleStartDate && candle.PeriodBegin <= visibleEndDate);
            var existDatePoints = candlesInScreen.Select(c => c.PeriodBegin);

            fluctuations = new List<PriceFluctuation>();
            if (visibleStartDate <= visibleEndDate)
            {
                if (existDatePoints.NotNullOrEmpty())
                {
                    DateTime point1 = existDatePoints.Min();
                    DateTime point2 = existDatePoints.Max();

                    if (point1.FloorToTimeFrame(timeFrame) != visibleStartDate)
                        fluctuations = ChartDataManager.GetPriceFluctuationsByTimeFrame(visibleStartDate, point1);

                    if (point2.UpToNextFrame(timeFrame) <= visibleEndDate)
                        fluctuations = fluctuations.Union(ChartDataManager.GetPriceFluctuationsByTimeFrame(point2.UpToNextFrame(timeFrame), visibleEndDate));
                }

               else
                {
                    fluctuations = ChartDataManager.GetPriceFluctuationsByTimeFrame(visibleStartDate, visibleEndDate);
                }

                foreach (PriceFluctuation priceFluctuation in fluctuations)
                {
                    Candle newCandle = Instantiate(candleDummy, candlesParent);
                    newCandle.Grid = CoordGrid;
                    newCandle.Set(priceFluctuation);
                    candles.Add(newCandle);

                }
            }

        }

        public void DrawCross()
        {

            Vector2 pointerScreenPosition;
            Vector3 pointerWolrdPosition;
            GetWorldPointerPosition(out pointerScreenPosition, out pointerWolrdPosition);
            //Vector2 pointerViewportPosition = cam.ScreenToViewportPoint(pointerScreenPosition);
            DrawTools.LineColor = crossColor;
            DrawTools.dashLength = 0.05f;
            DrawTools.gap = 0.07f;

            Vector2 modifiedPointerPosition = new Vector2(Mathf.Round(pointerWolrdPosition.x), pointerWolrdPosition.y);
            Vector2 camToViewportFromXToTop =new Vector2(modifiedPointerPosition.x, worldPointInRightUpCorner.y);
            Vector2 camToViewportFromXToBottom = new Vector2(modifiedPointerPosition.x, worldPointInLeftDownCorner.y);

            //Магнитизм к X. Необходимо будет переписать

            DrawTools.DrawLine(modifiedPointerPosition, camToViewportFromXToTop, cam.orthographicSize *1.1f, true);
            DrawTools.DrawLine(modifiedPointerPosition, camToViewportFromXToBottom, cam.orthographicSize *1.1f, true);
            DrawTools.DrawLine(modifiedPointerPosition, new Vector2(worldPointInLeftDownCorner.x, pointerWolrdPosition.y), cam.orthographicSize, true);
            DrawTools.DrawLine(modifiedPointerPosition, new Vector2(worldPointInRightUpCorner.x, pointerWolrdPosition.y), cam.orthographicSize, true);
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

        public void DrawVolume(Rect camRect)
        {
            if (!IsSettingsSet) return;

            TimeFrame timeFrame = chartDataManager.TFrame;
            DateTime visibleStartDate = CoordGrid.FromXAxisToDate(worldPointInLeftDownCorner.x);
            DateTime visibleEndDate = CoordGrid.FromXAxisToDate(worldPointInRightUpCorner.x).UpToNextFrame(timeFrame);
           
            visibleEndDate = visibleEndDate.UpToNextFrame(timeFrame);
            float tmp = (CoordGrid.FromDateToXAxis(visibleEndDate) - CoordGrid.FromDateToXAxis(visibleStartDate)) / (worldPointInRightUpCorner.x - worldPointInLeftDownCorner.x);
            float tmp2 = (CoordGrid.FromDateToXAxis(visibleEndDate) - CoordGrid.FromDateToXAxis(visibleStartDate)) / (CoordGrid.FromDateToXAxis(visibleEndDate) - worldPointInLeftDownCorner.x) - 1;


            if (visibleStartDate <= visibleEndDate)
            {
                fluctuations = chartDataManager.GetPriceFluctuationsByTimeFrame(visibleStartDate, visibleEndDate);

                int count = DateTimeTools.CountFramesInPeriod(timeFrame, visibleStartDate, visibleEndDate, TimeSpan.Zero);
                float pixelLenghtFrame = camRect.width * tmp / count;
                float maxVolume = (float)fluctuations.Max(x => x.Volume);
               
                Vector2 barLeftDownCorner = camRect.min - new Vector2 (tmp2 *camRect.width + pixelLenghtFrame / 2, 0) ;
                Vector2 barRightUpCorner;
                Vector2 bordersOffset = new Vector2((100/count<1? 1: 100/count), 0);

                if (chartDataManager.DataBeginTime > visibleStartDate)
                {
                    int shift = DateTimeTools.CountFramesInPeriod(timeFrame, visibleStartDate, chartDataManager.DataBeginTime, TimeSpan.Zero);
                    barLeftDownCorner += new Vector2(shift* pixelLenghtFrame, 0);
                }

                foreach (var fluctuation in fluctuations)
                {
                    
                    float pixelHeightFrame = (float)fluctuation.Volume / maxVolume * camRect.height;
                    barRightUpCorner = barLeftDownCorner + new Vector2(pixelLenghtFrame, pixelHeightFrame);

                    DrawTools.DrawRectangle(barLeftDownCorner+ bordersOffset, barRightUpCorner- bordersOffset, fluctuation.Close - fluctuation.Open>0? volumeUpColor: volumeDownColor);

                    barLeftDownCorner = new Vector2(barRightUpCorner.x,camRect.min.y);

                }
            }
         
        }

        private void OnDestroy()
        {
            if(GameManager.Instance)
                GameManager.Instance.GoToNextFluctuation -= UpdateInNextFrame;
        }

    }
}