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
        IGrid coordGrid;
        Grid2D grid2D;
        public IChartDataManager ChartDataManager
        {
            get { return chartDataManager; }
            set
            {
                chartDataManager = value;
                
                coordGrid.ZeroPoint = chartDataManager.ChartBeginTime;
                coordGrid.Step = chartDataManager.TFrame;

                /*visibleRange.ticks1 = chartDataManager.ChartEndTime.Ticks;
                visibleRange.ticks0 = (chartDataManager.ChartEndTime - frameCount * chartDataManager.TFrame).Ticks;
                if (!cam)
                    Debug.LogError("Не задана камера");
                else
                {
                    scaleX = visibleRange.ticks1 - visibleRange.ticks0 / cam.pixelWidth;

                }*/
                
            }
        }
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
            if (cam == null || ChartDataManager == null)
                Debug.Log("Повесь скрипт на камеру и задайте все параметры");
            coordGrid = new CoordinateGrid();

        }
        // Use this for initialization
        void Start()
        {
        }

        void Update()
        {
            DrawChart();
        }
        // Update is called once per frame
        void OnPostRender()
        {
            DrawGrid();
            //DrawDateText();
            //DrawCross();
        }

        /*private void OnDrawGizmos()
        {
            if (!cam) cam = GetComponent<Camera>();
            DrawGrid();
        }*/

        public bool IsSettingsSet
        {
            get
            {
                if (ChartDataManager != null && candleDummy != null && candlesParent != null)
                    return true;
                else
                {
                    Debug.Log("ChartDrawer не может выполнить действие, пока не заданы все параметры");
                    return false;
                }
            }
        }

        internal Vector2 GetLastPoint()
        {
            float x = coordGrid.FromDateToXAxis(ChartDataManager.ChartEndTime);
            float y = (float)chartDataManager.GetFluctuation(chartDataManager.ChartEndTime).Close;
            return new Vector2(x, y);
        }

        public List<DateTime> GetDateTimeCluePoints()
        {
            return new List<DateTime>();
        }

        public void DrawGrid()
        {
            leftDownCorner = cam.ViewportToWorldPoint(cachedZero);
            rightUpCorner = cam.ViewportToWorldPoint(cachedOne);



            DrawTools.LineColor = baseColor;
            DrawTools.dashLength = 0.025f;
            DrawTools.gap = 0.03f;


            //Вывод цен на экран
            decimal lowestPrice = (decimal)leftDownCorner.y;
            decimal highestPrice = (decimal)rightUpCorner.y;
            List<decimal> pricesList = Ariphmetic.DividePriceRangeByKeyPoints(lowestPrice, highestPrice, priceTextPool.FieldsAmount);
            priceTextPool.CleanPool();

            float yPoint;
            foreach (var price in pricesList)
            {
                yPoint = coordGrid.FromPriceToYAxis((float)price);
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
            DateTime dt0 = coordGrid.FromXAxisToDate((int)cam.ViewportToWorldPoint(cachedZero).x);
            DateTime dt1 = coordGrid.FromXAxisToDate((int)cam.ViewportToWorldPoint(cachedOne).x);

            if (coordGrid is CoordinateGrid)
                dt0 = (coordGrid as CoordinateGrid).DateCorrection(dt0, dt1);

            var dateList = DateTimeTools.DividePeriodByKeyPoints(dt0, dt1, dateTextPool.FieldsAmount);
            dateTextPool.CleanPool();
            foreach (var date in dateList)
            {
                Vector2 dateLine = new Vector2(coordGrid.FromDateToXAxis(date), 0);

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

       
       /* public void DrawCandles(int open, int high, int low, int close,int date, int width)
        {
            Material lineMaterial = new Material(Shader.Find("Unlit/Color"));
            lineMaterial.SetPass(0);

            GL.PushMatrix();
            GL.LoadOrtho();

            GL.Begin(GL.QUADS);

            Vector2 p1, p2, p3, p4;
            double screenBottom =0;
            foreach (PriceFluctuation fluct in fluctuationList)
            {
                p1 = new Vector2(date, low);
                p2 = new Vector2(date + width, low);
                p3 = new Vector2(date, high);
                p4 = new Vector2(date + width, high);

                GL.Vertex3(p1.x, p1.y, 1);
                GL.Vertex3(p2.x, p2.y, 1);
                GL.Vertex3(p3.x, p3.y, 1);
                GL.Vertex3(p4.x, p4.y, 1);
            }
           
            GL.End();
            GL.PopMatrix();
            
        }

        */
       /* public void DrawFluctuations()
        {

            foreach (PriceFluctuation fluctuation in fluctuationList)
            {
                //DrawCandle(1);
            }
        }*/
        public void DownloadFluctuations()
        {
            leftDownCorner = cam.ViewportToWorldPoint(cachedZero);
            rightUpCorner = cam.ViewportToWorldPoint(cachedOne);
            float screenLength = rightUpCorner.x- leftDownCorner.x;
            screenLength = screenLength < 100 ? screenLength : 100;
            TimeFrame timeFrame = chartDataManager.TFrame;
            DateTime visibleStartDate = coordGrid.FromXAxisToDate(leftDownCorner.x- screenLength).FloorToTimeFrame(timeFrame);
            DateTime visibleEndDate = coordGrid.FromXAxisToDate(rightUpCorner.x+ screenLength).UpToNextFrame(timeFrame);
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
            leftDownCorner = cam.ViewportToWorldPoint(cachedZero);
            rightUpCorner = cam.ViewportToWorldPoint(cachedOne);

            TimeFrame timeFrame = chartDataManager.TFrame;
            DateTime visibleStartDate = coordGrid.FromXAxisToDate(leftDownCorner.x).FloorToTimeFrame(timeFrame);
            DateTime visibleEndDate = coordGrid.FromXAxisToDate(rightUpCorner.x).UpToNextFrame(timeFrame);
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

                    if (point2.UpToNextFrame(timeFrame) < visibleEndDate)
                        fluctuations = fluctuations.Union(ChartDataManager.GetPriceFluctuationsByTimeFrame(point2, visibleEndDate));
                }

                //fluctuations = ChartDataManager.GetPriceFluctuationsByTimeFrame(visibleStartDate, visibleEndDate).Where(fluct => candlesInScreen.All(candle=> candle.PeriodBegin != fluct.PeriodBegin));
                else
                {
                    fluctuations = ChartDataManager.GetPriceFluctuationsByTimeFrame(visibleStartDate, visibleEndDate);
                }

                foreach (var priceFluctuation in fluctuations)
                {
                    Candle newCandle = Instantiate(candleDummy, candlesParent);
                    newCandle.Set(coordGrid.FromDateToXAxis(priceFluctuation.PeriodBegin), priceFluctuation);
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
            if (true)
            {
                //pointerWolrdPosition = new Vector2(Mathf.RoundToInt(pointerWolrdPosition.x), pointerWolrdPosition.y);
                //camToWorldFromXToTop = new Vector2(Mathf.RoundToInt(camToWorldFromXToTop.x), camToWorldFromXToTop.y);
                //camToWorldFromXToBottom = new Vector2(Mathf.RoundToInt(camToWorldFromXToBottom.x), camToWorldFromXToBottom.y);
            }

            DrawTools.DrawLine(pointerViewportPosition, camToViewportFromXToTop, cam.orthographicSize * 1.1f, true, cam.aspect);
            DrawTools.DrawLine(pointerViewportPosition, camToViewportFromXToBottom, cam.orthographicSize * 1.1f, true, cam.aspect);
            DrawTools.DrawLine(pointerViewportPosition, cam.ScreenToViewportPoint(new Vector2(0, pointerScreenPosition.y)), cam.orthographicSize, true, cam.aspect);
            DrawTools.DrawLine(pointerViewportPosition, cam.ScreenToViewportPoint(new Vector2(cam.pixelWidth, pointerScreenPosition.y)), cam.orthographicSize, true, cam.aspect);
        }

        public void DrawDateText()
        {
            dateTextPool.CleanPool();
            foreach (var date in dateList)
            {
                dateTextPool.SetText(
                    coordGrid.FromXAxisToDate((int)date).ToShortDateString() + " " + coordGrid.FromXAxisToDate((int)date).Hour + ":" + coordGrid.FromXAxisToDate((int)date).Minute
                    , cam.WorldToScreenPoint(new Vector2(date, 0)).x,
                    TextPoolManager.ShiftBy.Horizontal
                    );
            }
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

            /*foreach(var date in dateList)
              {
                  //Debug.Log(cam.WorldToScreenPoint(new Vector3(date,0,0)).x);
                  GUILayout.BeginArea(new Rect(cam.WorldToScreenPoint(new Vector3(date, 0, 0)).x - 50,0, 200, 200));
                  GUILayout.Label(FromPositionToDate(date).ToShortDateString() +" "+ FromPositionToDate(date).Hour+":"+ FromPositionToDate(date).Minute);
                  //cam.WorldToScreenPoint(new Vector3(price, 0, 0)).
                  GUILayout.EndArea();
              }*/
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
            float x0 = coordGrid.FromDateToXAxis(ChartDataManager.ChartBeginTime);
            float x1 = coordGrid.FromDateToXAxis(ChartDataManager.ChartEndTime);          
            //Debug.Log("BeginTime:" + x0 + " EndTime:" + x1);
            if (point.x < x0 || point.x > x1) return true;
            return false;
        }

    }
}