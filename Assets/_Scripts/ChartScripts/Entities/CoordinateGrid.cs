using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;

namespace Chart
{
    using Entity;
    public class CoordinateGrid : IGrid
    {
        //Соответствует 0 на оси абцисс
        DateTime zeroPoint = DateTime.UtcNow;
        //соответствует 1 единице смещения по оси абцисс
        TimeFrame step = new TimeFrame(Period.Hour);

        public DateTime ZeroPoint
            {
            get
            {
                return zeroPoint;
            }
            set {zeroPoint = value;} }

        public TimeFrame Step
        {
            get { return step; }
            set
            {
                step = value;
            }
        }

        public Action Updated;

        public DateTime FromXAxisToDate(float x)
        {
            return zeroPoint + x * step;
        }

        public int FromDateToXAxis(DateTime dateTime)
        {
            switch (step.period)
            {
                case Period.Minute: { return (int)((dateTime - zeroPoint).TotalMinutes / step.count); } 
                case Period.Hour: { return (int)((dateTime - zeroPoint).TotalHours / step.count); } 
                case Period.Day: { return (int)((dateTime - zeroPoint).TotalDays / step.count); } 
                case Period.Week: { return (int)((dateTime - zeroPoint).TotalDays / (7 * step.count)); } 
                case Period.Month: { return (dateTime.Month - zeroPoint.Month) / step.count; } 
                case Period.Year: { return (dateTime.Year - zeroPoint.Year) / step.count; } 
                default: {
                        throw new ArgumentOutOfRangeException("Для периода " + step.period.ToString() + "не описано действие");
                    }
            }
        }

        public float FromYAxisToPrice(float y)
        {
            return y/ Candle.scale;
        }

        public float FromPriceToYAxis(float price)
        {
            return Candle.scale*price;
        }

        //Костыльная функция для правки дат, которые пришли из GridCoords.
        DateTime correct_dt0, correct_dt1;
        //Относительное отклонение текущей разницы дат, от дат на предыдущей итерации, которое считается в пределах нормы
        const double deviation = 0.0625;
        public DateTime DateCorrection(DateTime dt0, DateTime dt1)
        {
            if (correct_dt0 != correct_dt1)
            {
                long ticksDiff = (dt1 - dt0).Ticks - (correct_dt1 - correct_dt0).Ticks;
                //Debug.Log("Ticks DIfference:" + ticksDiff + " Div:" + Mathf.Abs((float)ticksDiff / (dt1 - dt0).Ticks));
                //Debug.Log(Mathf.Abs((float)ticksDiff / (dt1 - dt0).Ticks));
                if (ticksDiff != 0 && Math.Abs((double)ticksDiff / (dt1 - dt0).Ticks) <= deviation)
                {
                    dt0 = dt0.AddTicks(ticksDiff);

                }
            }
            correct_dt0 = dt0;
            correct_dt1 = dt1;
            return dt0;

        }


    }
}

