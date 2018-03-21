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
        #region SINGLETON
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
        #endregion

        [SerializeField]TextPoolManager dateTextPool;
        [SerializeField]TextPoolManager priceTextPool;


        IChartDataManager chartDataManager;
        IGrid coordGrid;
        public IChartDataManager ChartDataManager
        {
            get { return chartDataManager; }
            set
            {
                chartDataManager = value;
                coordGrid.ZeroPoint = chartDataManager.ChartBeginTime;
                coordGrid.Step = chartDataManager.TFrame;
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
            coordGrid = new CoordinateGrid();

        }
        // Use this for initialization
        void Start()
        {           
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

        internal Vector2 GetLastPoint()
        {
            float x = coordGrid.FromDateToXAxis(ChartDataManager.ChartEndTime);
            float y = (float) chartDataManager.GetFluctuation(chartDataManager.ChartEndTime).Close;
            return new Vector2(x, y);
        }

        public List<DateTime> GetDateTimeCluePoints()
        {
            return new List<DateTime>();
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
            DrawTools.dashLength = 0.025f;
            DrawTools.gap = 0.03f;
            //dateList.Clear();
  
            priceTextPool.CleanPool();

            for (int i = 0; i != periodDevider + 1; i++)
            {
               int xPoint = (int)(leftDownCorner.x + i * xShift);
               int yPoint = (int)(leftDownCorner.y + i * yShift);

               // dateList.Add(xPoint);
               // dateTextPool.SetText(
               //     coordGrid.FromXAxisToDate(xPoint).ToChartString(),
               //     cam.WorldToScreenPoint(new Vector2(xPoint, 0)).x,
               //     TextPoolManager.ShiftBy.Horizontal
               //     );

                priceTextPool.SetText(
                    coordGrid.FromYAxisToPrice(yPoint).ToString(),
                    cam.WorldToScreenPoint(new Vector2(0, yPoint)).y,
                    TextPoolManager.ShiftBy.Vertical
                    );

               // Vector2 datePoint1 = new Vector2(cam.WorldToViewportPoint(new Vector2(xPoint, 0)).x, 0);
               //Vector2 datePoint2 = new Vector2(cam.WorldToViewportPoint(new Vector2(xPoint, 0)).x, 1);
                Vector2 pricePoint1 = new Vector2(0,cam.WorldToViewportPoint(new Vector2(0, yPoint)).y);
               Vector2 pricePoint2 = new Vector2(1,cam.WorldToViewportPoint(new Vector2(0, yPoint)).y);
         
               //DrawTools.DrawLine(datePoint1, datePoint2, cam.orthographicSize, true, cam.aspect);
               DrawTools.DrawLine(pricePoint1, pricePoint2, cam.orthographicSize, true, cam.aspect);
            }

            var dateList =  GetKeyDataPoints2(5, chartDataManager.TFrame);
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



        public void DrawChart()
        {
            int id = 0;
            foreach (var priceFluctuation in ChartDataManager.GetPriceFluctuationsByTimeFrame(ChartDataManager.ChartBeginTime, ChartDataManager.ChartEndTime))
            {
                Candle newCandle = Instantiate(candleDummy, candlesParent);
                newCandle.Set(coordGrid.FromDateToXAxis(priceFluctuation.PeriodBegin), priceFluctuation);
                candles.Add(newCandle);
                id++;
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

            DrawTools.DrawLine(pointerViewportPosition, camToViewportFromXToTop, cam.orthographicSize*1.1f, true, cam.aspect);
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

            pointerWolrdPosition =  cam.ScreenToWorldPoint(new Vector3(pointerScreenPosition.x, pointerScreenPosition.y, cam.nearClipPlane));
        }

        //int FromDateToPosition(DateTime dt)
        //{
        //    TimeFrame tFrame = ChartDataManager.TFrame;
        //    if (startingPoint == DateTime.MinValue)
        //    {
        //        startingPoint = dt.FloorToTimeFrame(tFrame);
        //        return 0;
        //    }

        //    return DateTimeTools.CountFramesInPeriod(tFrame, startingPoint, dt, TimeSpan.Zero);
        //}

        //DateTime FromPositionToDate(float dt)
        //{
        //    TimeFrame tFrame = ChartDataManager.TFrame;
        //    if(candles[0])
        //    {
        //        Candle candle = candles[0];
        //        int shift = (int)(dt -candle.transform.position.x );
        //        return candle.PeriodBegin + tFrame * shift;
        //    }

        //    throw new ArgumentNullException("На графике нет свечей");
        //}

        public bool IsPointToFar(Vector3 point)
        {
            float x0 = coordGrid.FromDateToXAxis(ChartDataManager.ChartBeginTime);
            float x1 = coordGrid.FromDateToXAxis(ChartDataManager.ChartEndTime);
            //Debug.Log("BeginTime:" + x0 + " EndTime:" + x1);
            if (point.x < x0 || point.x > x1) return true;
            return false;
        }

        public IEnumerable<DateTime> GetKeyDataPoints()
        {
            DateTime dt0 = coordGrid.FromXAxisToDate((int)cam.ViewportToWorldPoint(cachedZero).x);
            DateTime dt1 = coordGrid.FromXAxisToDate((int)cam.ViewportToWorldPoint(cachedOne).x);

            TimeSpan dif = dt1 - dt0;
            if (dif.Days > 365)
            {
                dt0 = dt0.FloorToMonths();
            }
            else
            {
                if(dif.Days > 28)
                {
                    dt0 = dt0.FloorToDays();
                }
                else
                {
                    if(dif.Hours>24)
                    {
                        dt0 = dt0.FloorToHours();
                    }
                    else
                    {
                        if(dif.Minutes >60)
                        {
                            dt0 = dt0.FloorToMinutes();
                        }
                    }
                }
            }
            dif = dt1 - dt0;

            List<DateTime> keyPoints = new List<DateTime>();

            int jMax = dateTextPool.FieldsAmount;
            long shift = dif.Ticks / jMax;

            DateTime current = dt0.AddTicks(shift);
            DateTime dateToAdd;
            int last;
            int dateSignificanceLVL =0;
            int prevDateSignificanceLVL =0;
            double div;

            //Округляем дату до более значимой, считаем уровень это значимой даты
            while (current <=dt1)
            {
                if (current.Year > dt0.Year)
                {
                    dateToAdd = new DateTime(current.Year, 1, 1);
                    dateSignificanceLVL = 0;
                }
                else
                {
                    if (current.Month > dt0.Month)
                    {
                        dateToAdd = new DateTime(current.Year, current.Month, 1);
                        dateSignificanceLVL = 1;

                    }
                    else
                    {
                        if (current.Day > dt0.Day)
                        {
                            dateToAdd = new DateTime(current.Year, current.Month, current.Day);
                            dateSignificanceLVL = 2;
                        }
                        else
                        {
                            if (current.Hour > dt0.Hour)
                            {
                                dateToAdd = new DateTime(current.Year, current.Month, current.Day, current.Hour, 0, 0);
                                dateSignificanceLVL = 3;
                            }
                            else
                            {
                                if (current.Minute > dt0.Minute)
                                {
                                    dateToAdd = new DateTime(current.Year, current.Month, current.Day, current.Hour, current.Minute, 0);
                                    dateSignificanceLVL = 4;
                                }
                                else
                                {
                                    dateToAdd = current;
                                    dateSignificanceLVL = 5;
                                }
                            }
                        }
                    }
                }


                if (dateSignificanceLVL ==5)
                {
                    keyPoints.Add(current);
                }
                else
                {
                    div = (double)(dateToAdd - dt0).Ticks / (current - dt0).Ticks;
                    //Выбор даты, которых добавляем в список. Добавляем в список только если она 
                    //на достаточном расстоянии от предыдущей, либо если она более значимая, чем предыдущая
                    //
                    if (div > 0.7)
                    {
                        keyPoints.Add(dateToAdd);
                    }
                    else
                    {
                        last = keyPoints.Count - 1;
                        if (prevDateSignificanceLVL > dateSignificanceLVL)
                            keyPoints[last] = dateToAdd;
                    }
                }
                dt0 = current;
                current = current.AddTicks(shift);
                prevDateSignificanceLVL = dateSignificanceLVL;
            }
             

            return keyPoints;
        }

        public IEnumerable<DateTime> GetKeyDataPoints1()
        {
            DateTime dt0 = coordGrid.FromXAxisToDate((int)cam.ViewportToWorldPoint(cachedZero).x);
            DateTime dt1 = coordGrid.FromXAxisToDate((int)cam.ViewportToWorldPoint(cachedOne).x);

            List<DateTime> keyPoints = new List<DateTime>();
            int textFieldsAvailable = dateTextPool.FieldsAmount;
            int textFieldsLeft = textFieldsAvailable;
            DateTime dt_current;

            //diff разница в годах
            double diff = dt1.Year - dt0.Year;
            double step = 1;
            double startYear = dt0.Year+1;

            if (textFieldsLeft < diff && textFieldsLeft != 0)
            {
                step = diff/ textFieldsLeft ;
                startYear = (int)(dt0.Year / step) * step + step;
            }

            while (startYear <= dt1.Year)
                {
                    keyPoints.Add(new DateTime((int)startYear, 1, 1));
                    textFieldsLeft--;
                    startYear += step;
                }

            //diff разница в месяцах
            diff = diff * 12 + dt1.Month - dt0.Month;
            if (diff > 0 && textFieldsLeft != 0)
            {
                step = diff / textFieldsAvailable;
                int[] possibleSteps = new int[] { 2, 3, 4, 6 };

                int monthesPerStep = possibleSteps.Where(x => x <= step).DefaultIfEmpty(0).Max();
                monthesPerStep = monthesPerStep > 0 ? monthesPerStep : 1;

                TimeFrame monthes = new TimeFrame(Period.Month, monthesPerStep);
                dt_current = dt0.UpToMonths(monthesPerStep);

                if (DateTimeTools.CountFramesInPeriod(monthes, dt_current, dt1, TimeSpan.Zero) - (dt1.Year-dt0.Year)  < textFieldsLeft)
                {
                   while (dt_current <= dt1)
                    {

                        if (dt_current.Month != 1)
                        {
                            keyPoints.Add(dt_current);
                            textFieldsLeft--;
                        }
                        dt_current += monthes;
                    }
                }
            }

            //diff разница в днях
            diff = Math.Round((dt1 - dt0).TotalDays);
            if (diff > 0 && textFieldsLeft!= 0 && textFieldsLeft > textFieldsAvailable / 2 - 1)
            {
                step =diff / textFieldsLeft;
                int daysPerStep = step <=1 ? 1: (int) step+1;
                TimeFrame days = new TimeFrame(Period.Day, daysPerStep);

                int month;
                dt_current = dt0.FloorToTimeFrame(days).UpToNextFrame(days);

                    while (dt_current <= dt1 && textFieldsLeft!=0)
                    {
                        if (dt_current.Day != 1)
                        {
                            keyPoints.Add(dt_current);
                            textFieldsLeft--;
                        }
                        month = dt_current.Month;
                        dt_current += days;
                        if (month < dt_current.Month) dt_current.FloorToMonths();
                    }
                
                
            }

            // diff разница в часах
             diff = Math.Round((dt1 - dt0).TotalHours);

             if (step ==1 && diff > 0 && textFieldsLeft != 0 && textFieldsLeft > textFieldsAvailable/2-1)
            {
                step = diff / textFieldsLeft;
                int[] possibleSteps = new int[] { 1, 2, 3, 4, 6, 12 };
                
                int hoursPerStep = possibleSteps.Where(x => x <= step).DefaultIfEmpty(0).Max();
                hoursPerStep = hoursPerStep > 0 ? hoursPerStep : 1;
                TimeFrame hours = new TimeFrame(Period.Hour, hoursPerStep);

                int hour;
                dt_current = dt0.FloorToTimeFrame(hours).UpToNextFrame(hours).UpToNextFrame(hours);

                while (dt_current <= dt1 && textFieldsLeft != 0)
                {
                    if (dt_current.Hour != 0)
                    {
                        keyPoints.Add(dt_current);
                        textFieldsLeft--;
                    }
                    hour = dt_current.Day;
                    dt_current += hours;
                    if (hour < dt_current.Day) dt_current.FloorToMonths();
                }
                if(textFieldsLeft != 0)
                {
                    keyPoints.Add(dt0.FloorToTimeFrame(hours).UpToNextFrame(hours));
                    textFieldsLeft--;
                }
            }



             return keyPoints;
        }

        public IEnumerable<DateTime> GetKeyDataPoints2(int size, TimeFrame timeFrame)
        {
            DateTime dt0 = coordGrid.FromXAxisToDate((int)cam.ViewportToWorldPoint(cachedZero).x);
            DateTime dt1 = coordGrid.FromXAxisToDate((int)cam.ViewportToWorldPoint(cachedOne).x);
            List<DateTime> keyPoints = new List<DateTime>();
            int textFieldsLeft = size;

            TimeSpan dateDiff = dt1 - dt0;

            long dateDiffInTicks = dateDiff.Ticks;
            long step = dateDiffInTicks / size;

            DateTime current_date = dt0.FloorToTimeFrame(timeFrame).UpToNextFrame(timeFrame);
            textFieldsLeft--;
            keyPoints.Add(current_date);
            while (dt0<= dt1 && textFieldsLeft != 0)
            {
                current_date = current_date.AddTicks(step).FloorToTimeFrame(timeFrame);
                keyPoints.Add(current_date);
                textFieldsLeft--;
            }
            return keyPoints;
        }
        }
    }