using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;

namespace Chart
{
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

        public DateTime FromXAxisToDate(int x)
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
            return y;
        }

        public float FromPriceToYAxis(float price)
        {
            throw new NotImplementedException();
        }
    }
}

