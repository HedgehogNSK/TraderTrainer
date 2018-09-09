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
        [Range(0,0.7f)]
        public float chartOffsetFromVerticalBorders = 0.05f;
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
        IScalableDataManager chartDataManager;
        public IScalableDataManager ChartDataManager
        {
            get { return chartDataManager; }
            set { chartDataManager = value; }
        }

        public IGrid CoordGrid { get; set; }
        public Color gridColor;
        public Color crossColor;
        public Color volumeUpColor;
        public Color volumeDownColor;

        [SerializeField] Candle candleDummy;
        [SerializeField] Transform candlesParent;

        Camera cam;

        Vector3 worldPointInLeftDownCorner;
        Vector3 worldPointInRightUpCorner;
        Vector3 camPrevPosition;
        float camPrevOrthoSize;
        Vector3 cachedZero = Vector3.zero;
        Vector3 cachedOne = Vector3.one;
        bool autoscale = false;
        public bool Autoscale
        {
            get { return autoscale; }
            set {
                if(autoscaleToggle!=null && autoscaleToggle.isOn!=value)
                    autoscaleToggle.isOn = value;
                autoscale = value;

                if(value) UpdateInNextFrame();
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
            Autoscale = true;          
        }

        public void UpdateInNextFrame()
        {
            needToBeUpdated = true;
        }
        public void UpdateInNextFrame<T>(T x)
        {
            UpdateInNextFrame();
        }
        DateTime visibleStartDate, cachedStart;
        DateTime visibleEndDate, cachedEnd;
        IEnumerable<DateTime> datesList;
        IEnumerable<decimal> pricesList;
        void Update()
        {
            worldPointInLeftDownCorner = cam.ViewportToWorldPoint(cachedZero);
            worldPointInRightUpCorner = cam.ViewportToWorldPoint(cachedOne);

            visibleStartDate = CoordGrid.FromXAxisToDate(worldPointInLeftDownCorner.x).FloorToTimeFrame(chartDataManager.TFrame);
            if (visibleStartDate < chartDataManager.WorkBeginTime)
                visibleStartDate = chartDataManager.WorkBeginTime;

            visibleEndDate = CoordGrid.FromXAxisToDate(worldPointInRightUpCorner.x).UpToNextFrame(chartDataManager.TFrame);
            if (visibleEndDate > chartDataManager.WorkEndTime)
                visibleEndDate = chartDataManager.WorkEndTime;

            if (visibleStartDate!= cachedStart || visibleEndDate !=cachedEnd)
            visibleFluctuations = chartDataManager.GetPriceFluctuationsByTimeFrame(visibleStartDate, visibleEndDate);

            cachedStart = visibleStartDate;
            cachedEnd = visibleEndDate;

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
            {
                autoscaleToggle.isOn = false;
            }

            if (NeedToBeUpdated)//Оптимизация
            {
                if (Autoscale) ScaleChart();

                datesList = GetVisibleDatesList();
                pricesList = GetVisiblePricesList();
                DrawChart();
            }
        }

        bool needToBeUpdated =false;
        bool NeedToBeUpdated
        {
            get
            {
                if (needToBeUpdated)
                {
                    needToBeUpdated = false;
                    return true;
                }


                bool camViewChanged = cam.orthographicSize != camPrevOrthoSize || camPrevPosition != cam.transform.position;

                camPrevOrthoSize = cam.orthographicSize;
                camPrevPosition = cam.transform.position;

                return camViewChanged;
            }
        }

        // Update is called once per frame
        void OnPostRender()
        {
            DrawGrid(datesList, pricesList);

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

        private IEnumerable<DateTime> GetVisibleDatesList()
        {
            //Вычисляем точки на временнОй сетке и отрисовываем их
            DateTime dt0 = CoordGrid.FromXAxisToDate(worldPointInLeftDownCorner.x);
            DateTime dt1 = CoordGrid.FromXAxisToDate(worldPointInRightUpCorner.x);

            if (CoordGrid is CoordinateGrid)
                dt0 = (CoordGrid as CoordinateGrid).DateCorrection(dt0, dt1);

            IEnumerable<DateTime> dateList = DateTimeTools.DividePeriodByKeyPoints(dt0, dt1, dateTextPool.FieldsAmount);
            return dateList;
        }

        private IEnumerable<decimal> GetVisiblePricesList()
        {
            //Вывод цен на экран
            decimal lowestPrice = (decimal)CoordGrid.FromYAxisToPrice(worldPointInLeftDownCorner.y);
            decimal highestPrice = (decimal)CoordGrid.FromYAxisToPrice(worldPointInRightUpCorner.y);
            List<decimal> pricesList = Ariphmetic.DividePriceRangeByKeyPoints(lowestPrice, highestPrice, priceTextPool.FieldsAmount);
            return pricesList;
        }
        public void DrawGrid(IEnumerable<DateTime>dateList,IEnumerable<decimal>pricesList)
        {
            if (!IsSettingsSet) return;


            DrawTools.LineColor = gridColor;
            DrawTools.dashLength = 0.05f;
            DrawTools.gap = 0.07f;

           priceTextPool.CleanPool();

            float yPoint;
            foreach (var price in pricesList)
            {
                yPoint = CoordGrid.FromPriceToYAxis((float)price);
                yPoint = cam.WorldToScreenPoint(new Vector2(0, yPoint)).y;
                priceTextPool.SetText(
                    price.ToString("F8"),
                    yPoint,
                    TextPoolManager.ShiftBy.Vertical
                    );

                Vector2 pricePoint1 = new Vector2(cam.WorldToScreenPoint(worldPointInLeftDownCorner).x, yPoint);
                Vector2 pricePoint2 = new Vector2(cam.WorldToScreenPoint(worldPointInRightUpCorner).x, yPoint);
               
                DrawTools.DrawOnePixelLine(pricePoint1, pricePoint2, true);
            }
           
            dateTextPool.CleanPool();
            foreach (var date in dateList)
            {
                float dateLine = CoordGrid.FromDateToXAxis(date);
                dateLine = cam.WorldToScreenPoint(new Vector2(dateLine, 0)).x;
                dateTextPool.SetText(
                    date.ChartStringFormat(),
                    dateLine,
                    TextPoolManager.ShiftBy.Horizontal
                    );


                Vector2 datePoint1 = new Vector2(dateLine, cam.pixelRect.min.y);
                Vector2 datePoint2 = new Vector2(dateLine, cam.pixelRect.max.y);

                DrawTools.DrawOnePixelLine(datePoint1, datePoint2, true);

            }
        }

        Transform camTransform;
        IEnumerable<PriceFluctuation> visibleFluctuations;

        //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        private void ScaleChart()
        {
            
           
            if (!IsSettingsSet) return;
              

            double highestPriceOnScreen = CoordGrid.FromYAxisToPrice(worldPointInRightUpCorner.y);
            double lowestPriceOnScreen = CoordGrid.FromYAxisToPrice(worldPointInLeftDownCorner.y);

            double highestPrice =  visibleFluctuations.Max(f => f.High);
            double lowestPrice =  visibleFluctuations.Min(f => f.Low);

            double priceRange = highestPrice - lowestPrice;
            float new_y = CoordGrid.FromPriceToYAxis((float)(lowestPrice + priceRange / 2));


            if (Math.Abs(new_y/cam.transform.position.y-1) >1e-4)
            cam.transform.position = new Vector3(cam.transform.position.x, new_y, cam.transform.position.z);

            if(highestPrice!=lowestPrice)
            CoordGrid.Scale *= (1-chartOffsetFromVerticalBorders)* (float)((highestPriceOnScreen - lowestPriceOnScreen) / priceRange);


        }

        public void DrawChart()
        {
            if (!IsSettingsSet) return;

            TimeFrame timeFrame = chartDataManager.TFrame;
            DateTime startDate = visibleStartDate;
            DateTime endDate = visibleEndDate;

            if (chartDataManager.WorkEndTime < endDate)
                endDate = chartDataManager.WorkEndTime;
            if (chartDataManager.WorkBeginTime > startDate)
                startDate = chartDataManager.WorkBeginTime;

            var candlesInScreen = candles.Where(candle => candle.PeriodBegin >= startDate && candle.PeriodBegin <= endDate);
            var existDatePoints = candlesInScreen.Select(c => c.PeriodBegin);

            IEnumerable<PriceFluctuation>  flucToLoad = new List<PriceFluctuation>();
            if (startDate <= endDate)
            {
                if (existDatePoints.NotNullOrEmpty())
                {
                    DateTime point1 = existDatePoints.Min();
                    DateTime point2 = existDatePoints.Max();

                    if (point1.FloorToTimeFrame(timeFrame) != startDate)
                        flucToLoad = ChartDataManager.GetPriceFluctuationsByTimeFrame(startDate, point1);

                    if (point2.UpToNextFrame(timeFrame) <= endDate)
                        flucToLoad = flucToLoad.Union(ChartDataManager.GetPriceFluctuationsByTimeFrame(point2.UpToNextFrame(timeFrame), endDate));
                }

               else
                {
                    flucToLoad = visibleFluctuations;
                }

                foreach (PriceFluctuation priceFluctuation in flucToLoad)
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
            
            Vector2 modifiedPointerPosition = cam.WorldToScreenPoint(new Vector2(Mathf.Round(pointerWolrdPosition.x), pointerWolrdPosition.y));
            Vector2 camToViewportFromXToTop = new Vector2(modifiedPointerPosition.x, cam.pixelRect.yMin);
            Vector2 camToViewportFromXToBottom = new Vector2(modifiedPointerPosition.x, cam.pixelRect.yMax);

            //Магнитизм к X. Необходимо будет переписать
            DrawTools.DrawOnePixelLine(modifiedPointerPosition, camToViewportFromXToTop,true);
            DrawTools.DrawOnePixelLine(modifiedPointerPosition, camToViewportFromXToBottom,  true);
            DrawTools.DrawOnePixelLine(modifiedPointerPosition, new Vector2(cam.pixelRect.xMin, pointerScreenPosition.y), true);
            DrawTools.DrawOnePixelLine(modifiedPointerPosition, new Vector2(cam.pixelRect.xMax, pointerScreenPosition.y), true);
        }

        /*void OnGUI()
        {
            Vector2 pointerScreenPosition;
            Vector3 pointerWolrdPosition;
            GetWorldPointerPosition(out pointerScreenPosition, out pointerWolrdPosition);

            GUILayout.BeginArea(new Rect(20, 50, 250, 120));
            GUILayout.Label("Screen pixels: " + cam.pixelWidth + ":" + cam.pixelHeight);
            GUILayout.Label("Mouse position: " + pointerScreenPosition);
            GUILayout.Label("World position: " + pointerWolrdPosition.ToString("F3"));
            GUILayout.EndArea();

        }*/

        public void GetWorldPointerPosition(out Vector2 pointerScreenPosition, out Vector3 pointerWolrdPosition)
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

            float worldRealToScreen = (CoordGrid.FromDateToXAxis(visibleEndDate) - CoordGrid.FromDateToXAxis(visibleStartDate)) / (worldPointInRightUpCorner.x - worldPointInLeftDownCorner.x);


            int count = DateTimeTools.CountFramesInPeriod(chartDataManager.TFrame, visibleStartDate, visibleEndDate, TimeSpan.Zero);
            float pixelLenghtFrame = camRect.width * worldRealToScreen / count;
            float maxVolume = (float)visibleFluctuations.Max(x => x.Volume);

            //Если свечка видна только частично, то необходимо смещать отрисовку объёма на равную долю видимости
            float diff = (worldPointInLeftDownCorner.x - CoordGrid.FromDateToXAxis(visibleStartDate)) * camRect.width / (worldPointInRightUpCorner.x - worldPointInLeftDownCorner.x);
            Vector2 barLeftDownCorner = camRect.min - new Vector2(diff + pixelLenghtFrame / 2, 0);
            Vector2 barRightUpCorner;
            Vector2 bordersOffset = new Vector2((20 / count < 1 ? 1 : 20 / count), 0);

            if (chartDataManager.WorkBeginTime > visibleStartDate)
            {
                int shift = DateTimeTools.CountFramesInPeriod(chartDataManager.TFrame, visibleStartDate, chartDataManager.WorkBeginTime, TimeSpan.Zero);
                barLeftDownCorner += new Vector2(shift * pixelLenghtFrame, 0);
            }

            foreach (var fluctuation in visibleFluctuations)
            {
                float pixelHeightFrame = (float)fluctuation.Volume / maxVolume * camRect.height;
                barRightUpCorner = barLeftDownCorner + new Vector2(pixelLenghtFrame, pixelHeightFrame);

                DrawTools.DrawRectangle(barLeftDownCorner + bordersOffset, barRightUpCorner - bordersOffset, fluctuation.Close - fluctuation.Open > 0 ? volumeUpColor : volumeDownColor);

                barLeftDownCorner = new Vector2(barRightUpCorner.x, camRect.min.y);

            }


        }

        public void UpdateMovingAverage(int id, int length)
        {
            if (length <= 0)
            {
                Debug.LogError("length должен быть >0");
                return;
            }

            IEnumerable<PriceFluctuation> fluctuations = chartDataManager.GetPriceFluctuationsByTimeFrame(chartDataManager.DataBeginTime,chartDataManager.WorkEndTime);
            IEnumerable<PriceFluctuation> fluctation2calc = fluctuations.Where(fluct => !fluct.ExtraData.ContainsKey(id));

            foreach (var fluct in fluctation2calc.Where(f => f.PeriodBegin >= chartDataManager.WorkBeginTime))
            {
                double ma = 0;
                int i = 0;

                foreach (var prev_fluct in fluctuations.Where(f => f.PeriodBegin <= fluct.PeriodBegin && f.PeriodBegin + (length - 1) * chartDataManager.TFrame >= fluct.PeriodBegin))
                {

                    ma += prev_fluct.Close;
                    i++;
                }


                if (i == length)
                {
                    ma /= length;
                    fluct.ExtraData[id] = (float)ma;
                }
            }
            

        }
        public void CalculateMovingAverage(int id, int length)
        {
            if (length <= 0)
            {
                Debug.LogError("length должен быть >0");
                return;
            }

            IEnumerable<PriceFluctuation> fluctuations = chartDataManager.GetPriceFluctuationsByTimeFrame(chartDataManager.DataBeginTime, chartDataManager.WorkEndTime);
            PriceFluctuation startFluct = fluctuations.OrderBy(f => f.PeriodBegin).ElementAtOrDefault(length - 1);
            if (startFluct != null)
            {
                foreach (var fluct in fluctuations.Where(f => f.PeriodBegin >= chartDataManager.WorkBeginTime && f.PeriodBegin >= startFluct.PeriodBegin))
                {
                    double ma = 0;
                    int i = 0;

                    foreach (var prev_fluct in fluctuations.Where(f => f.PeriodBegin <= fluct.PeriodBegin && f.PeriodBegin + (length - 1) * chartDataManager.TFrame >= fluct.PeriodBegin))
                    {

                        ma += prev_fluct.Close;
                        i++;
                    }


                    if (i == length)
                    {
                        ma /= length;
                        fluct.ExtraData[id] = (float)ma;
                    }
                }
            }

        }
        public void DrawPointArray(int id, Color color)
        {
            Vector2 point, point2;
            DrawTools.LineColor = color;
            IEnumerator<PriceFluctuation> it = visibleFluctuations.GetEnumerator();
         
            while (it.MoveNext() &&  (!it.Current.ExtraData.ContainsKey(id) || !(it.Current.ExtraData[id] is float) )) ;

            if (!it.Current.ExtraData.ContainsKey(id)) return;
            
                point = cam.WorldToScreenPoint(new Vector2(CoordGrid.FromDateToXAxis(it.Current.PeriodBegin), CoordGrid.FromPriceToYAxis((float)it.Current.ExtraData[id])));

                while (it.MoveNext())
                {
                    if (it.Current.ExtraData[id] is float)
                    {
                        point2 = cam.WorldToScreenPoint(new Vector2(CoordGrid.FromDateToXAxis(it.Current.PeriodBegin), CoordGrid.FromPriceToYAxis((float)it.Current.ExtraData[id])));
                        DrawTools.DrawLine(point, point2, false);
                        point = point2;
                    }


                }
            

        }
        public void ReloadData()
        {
            foreach (var candle in candles)
            {
               
                Destroy(candle.gameObject);
                
            }
            candles = new List<Candle>();
        }
    }
}