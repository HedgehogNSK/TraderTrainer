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
        //Vector3 leftUpCorner = new Vector3(0, 1);
        //Vector3 rightDownCorner = new Vector3(1,0);
        Vector3 rightUpCorner;
        Vector3 cachedZero = Vector3.zero;
        Vector3 cachedOne = Vector2.one;
        int periodDevider = 5;
        

        List<Candle> candles = new List<Candle>();
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
            //float xLenght = rightUpCorner.x - leftDownCorner.x;
            float yLenght = rightUpCorner.y - leftDownCorner.y;

            //float xShift = xLenght / periodDevider;
            float yShift = yLenght / periodDevider;

            DrawTools.LineColor = baseColor;
            DrawTools.dashLength = 0.025f;
            DrawTools.gap = 0.03f;
            //dateList.Clear();
  
            priceTextPool.CleanPool();

            for (int i = 0; i != periodDevider + 1; i++)
            {
               //int xPoint = (int)(leftDownCorner.x + i * xShift);
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

           // var dateList =  GetKeyDataPoints1(dateTextPool.FieldsAmount,chartDataManager.TFrame);
            var dateList =  GetKeyDataPoints2(dateTextPool.FieldsAmount, chartDataManager.TFrame);
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

        public IEnumerable<DateTime> GetKeyDataPoints1(int maxDivison, TimeFrame minFrame)
        {
            DateTime dt0 = coordGrid.FromXAxisToDate((int)cam.ViewportToWorldPoint(cachedZero).x);
            DateTime dt1 = coordGrid.FromXAxisToDate((int)cam.ViewportToWorldPoint(cachedOne).x);

           //// Debug.Log(dt0.ToLongDateString() + " " +dt0.ToLongTimeString());
            ///Debug.Log(dt1.ToLongDateString() + " " +dt1.ToLongTimeString());

            List<DateTime> keyPoints = new List<DateTime>();
            int textFieldsLeft = maxDivison;
            DateTime dt_current;

            int[] possibleSteps;
            //diff разница в годах
            double yearDiff = dt1.Year - dt0.Year;
            double step = 1;
            int intStep=1;
            int startYear = dt0.Year+1;

            if (textFieldsLeft < yearDiff && textFieldsLeft != 0)
            {
                intStep = (int)(yearDiff / textFieldsLeft+1);
                startYear = (dt0.Year / intStep) * intStep + intStep;
            }

            while (startYear <= dt1.Year)
                {
                    keyPoints.Add(new DateTime(startYear, 1, 1));
                    textFieldsLeft--;
                    startYear += intStep;
            }

            //diff разница в месяцах
            double monthDiff = yearDiff * 12 + dt1.Month - dt0.Month;

            step = monthDiff / maxDivison;
            if (step > 0 && step <= 12)
            {
                possibleSteps = new int[] { 1, 2, 3, 4, 6 };

                intStep = possibleSteps.Where(x => x >= step).DefaultIfEmpty(0).Min();

                if (intStep > 0)
                {
                    TimeFrame monthes = new TimeFrame(Period.Month, intStep);
                    int tempMonthAmount = ((dt0.Month - 1) / intStep + 1) * intStep - dt0.Month + 1;
                    dt_current = dt0.AddMonths(tempMonthAmount).FloorToMonths();

                    while (dt_current <= dt1 && textFieldsLeft != 0)
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
            double dayDiff =(dt1 - dt0).TotalDays;
            {
                step = dayDiff / (maxDivison - (monthDiff + 1));
                

                if (step <15 && step>0)//Середина месяца
                {
                    step = step <= 1 ? 1 : step + 1;
                    // int month;
                    TimeFrame days = new TimeFrame(Period.Day, (int)step);
                    dt_current = dt0.FloorToTimeFrame(days).UpToNextFrame(days);

                    while (dt_current <= dt1 && textFieldsLeft != 0)
                    {
                        if (dt_current.Day != 1)
                        {
                            keyPoints.Add(dt_current.FloorToTimeFrame(minFrame));
                            textFieldsLeft--;
                        }
                        //month = dt_current.Month;
                        dt_current += days;
                        //if (month < dt_current.Month) dt_current.FloorToMonths();
                    }
                }


            }

            // diff разница в часах
            double hourDiff = (dt1 - dt0).TotalHours;

            step = hourDiff / maxDivison;
            possibleSteps = new int[] { 1, 2, 3, 4, 6, 8, 12 };
            intStep = possibleSteps.Where(x => x >= step).DefaultIfEmpty(0).Min();
            if(intStep>0)

            {
                TimeFrame hours = new TimeFrame(Period.Hour, intStep);

                dt_current = dt0.FloorToTimeFrame(hours).UpToNextFrame(hours);

                while (dt_current <= dt1 && textFieldsLeft != 0)
                {
                    if (dt_current.Hour != 0)
                    {
                        if (dt_current != dt_current.FloorToTimeFrame(minFrame))
                            keyPoints.Add(dt_current.UpToNextFrame(minFrame));
                        else
                            keyPoints.Add(dt_current);
                        textFieldsLeft--;
                    }
                    dt_current += hours;
                }
            }


            // diff разница в минутах
            double minuteDiff = (dt1 - dt0).TotalMinutes;
            step =  minuteDiff / maxDivison;
            Debug.Log(step);
            possibleSteps = new int[] { 1, 2, 3, 4, 5, 6, 10, 12, 15,20, 30 };
            intStep = possibleSteps.Where(x => x >= step).DefaultIfEmpty(0).Min();
            if (intStep > 0)
            {
                TimeFrame minutes = new TimeFrame(Period.Minute, intStep);

                dt_current = dt0.FloorToTimeFrame(minutes).UpToNextFrame(minutes);

                while (dt_current <= dt1 && textFieldsLeft != 0)
                {
                    if (dt_current.Minute != 0)
                    {
                        if (dt_current != dt_current.FloorToTimeFrame(minFrame))
                            keyPoints.Add(dt_current.UpToNextFrame(minFrame));
                        else
                            keyPoints.Add(dt_current);
                        textFieldsLeft--;
                    }
                    dt_current += minutes;
                }
            }





            return keyPoints;
        }

        double tmp =0;
        public IEnumerable<DateTime> GetKeyDataPoints2(int maxDivison, TimeFrame chartTimeFrame)
        {
            DateTime dt1 = coordGrid.FromXAxisToDate(cam.ViewportToWorldPoint(cachedOne).x);
            DateTime dt0 = coordGrid.FromXAxisToDate(cam.ViewportToWorldPoint(cachedZero).x);

            List<DateTime> keyPoints = new List<DateTime>();           
         
            TimeSpan dateDiff = dt1 - dt0;
            double dateDifference;
            long dateDiffInTicks = dateDiff.Ticks;
            long stepTicks = dateDiffInTicks / maxDivison;
            TimeSpan stepTS = new TimeSpan(stepTicks);
            int step;
            TimeFrame timeFrame;
            Period period;
            Debug.Log("Делителей: "+maxDivison +"; Разница дат: "+ dateDiff.ToString() );
            if (stepTS.TotalDays > 365)
            {
                //1+ необходимо для увеличения размера шага, так как int округлит деление в меньшую сторону и делителей не хватит
                dateDifference = dt1.Year - dt0.Year;
                period = Period.Year;    
            }
            else if(stepTS.TotalDays > 31)
            {
                dateDifference = (dt1.Year - dt0.Year) * 12 + dt1.Month - dt0.Month;
                period = Period.Month;
            }
            else if(stepTS.TotalDays>1)
            {
                dateDifference = (dt1 - dt0).TotalDays;
                period = Period.Day;
            }
            else if(stepTS.TotalHours > 1)
            {
                dateDifference = (dt1 - dt0).TotalHours;
                period = Period.Hour;
            }

            else if (stepTS.TotalMinutes>1)
            {          
                dateDifference = (dt1 - dt0).TotalMinutes;
                period = Period.Minute;
            }
            else
            {               
                Debug.Log("Слишком маленький промежуток");
                return null;
            }

            if (Math.Abs(tmp / dateDifference - 1) >= 0.05)
            {
                tmp = dateDifference;
            }
            //Debug.Log(dateDifference + " " + tmp);
            step = (int)(1 + tmp / (maxDivison-2));
            timeFrame = new TimeFrame(period, step);
            TimeFrame halfFrame = new TimeFrame(period, step);
            DateTime next_date;
            DateTime current_date;
            DateTime floor_date;

            //Debug.Log(dt0.ToShortTimeString() + " " + dt0.FloorToTimeFrame(timeFrame).ToShortTimeString() + " " + dt0.FloorToTimeFrame(timeFrame).ToShortTimeString());

            current_date = dt0.UpToNextFrame(timeFrame);

            next_date = current_date;
            if (current_date.Year > dt0.Year)
            {
                next_date = current_date.FloorToYears();
            }
            else if (current_date.Month != dt0.Month)
            {
                next_date = current_date.FloorToMonths();
            }
            else if (current_date.Day != dt0.Day)
            {
                next_date = current_date.FloorToDays();

            }
            else if (current_date.Hour != dt0.Hour)
            {
                if (chartTimeFrame.period == Period.Hour)
                { next_date = next_date.FloorToTimeFrame(chartTimeFrame); }
                else
                { next_date = next_date.FloorToHours(); }
            }
            
            if(next_date > dt0)
            {
                keyPoints.Add(next_date);
                Debug.Log(1);
            }
            else
            {
                keyPoints.Add(current_date.FloorToTimeFrame(chartTimeFrame));
                Debug.Log(2);

            }

            while (current_date < dt1)
            {              
                next_date = current_date +timeFrame;

                if (next_date.Year > current_date.Year)
                {
                    floor_date = next_date.FloorToYears();
                }
                else if (next_date.Month != current_date.Month)
                {

                    floor_date = next_date.FloorToMonths();
                }
                else if (next_date.Day != current_date.Day)
                {
                    if (chartTimeFrame.period == Period.Day)
                    {
                        floor_date = next_date.FloorToTimeFrame(chartTimeFrame);
                    }
                    else
                    {
                        floor_date = next_date.FloorToDays();
                    }
                   
                }
                else if (next_date.Hour != current_date.Hour)
                {
                    //int count = chartTimeFrame.period == Period.Hour ? chartTimeFrame.count : 1;
                    if (chartTimeFrame.period == Period.Hour)
                    { floor_date = next_date.FloorToTimeFrame(chartTimeFrame); }
                    else
                    { floor_date = next_date.FloorToHours(); }
                }
                else
                {
                    floor_date = next_date;
                    keyPoints.Add(current_date.FloorToTimeFrame(chartTimeFrame));
                }

                if (2 * floor_date.Ticks - current_date.Ticks > next_date.Ticks)
                {

                    keyPoints.Add(floor_date);
                }
                else
                {
                    if(keyPoints.Count!=0)
                    keyPoints[keyPoints.Count - 1] = floor_date;
                }

                current_date = next_date;
            }
            return keyPoints;
        }
        }
    }