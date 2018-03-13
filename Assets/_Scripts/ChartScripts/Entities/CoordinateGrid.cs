using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;

namespace Chart
{
    public static class CoordinateGrid
    {
        //Соответствует 0 на оси абцисс
        static DateTime zeroPoint = DateTime.UtcNow;
        //соответствует 1 единице смещения по оси абцисс
        static TimeFrame step = new TimeFrame(Period.Hour);
        
        static public TimeFrame Step
        {
            get { return step; }
            set
            {
                step = value;
            }
        }

        static public Action Updated;

        //Возвращает 
        static float periodXWidth;
        static public float PeriodXWidth { get { return periodXWidth; } }

        static public DateTime FromXAxisToDate(int x)
        {
            return zeroPoint + x * step;
        }

        static public int FromDateToXAxis(DateTime dateTime)
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
                        throw new System.ArgumentOutOfRangeException("Для периода " + step.period.ToString() + "не описано действие");
                    }
            }
        }

        static public float FromXAxisToScreen(int x)
        {

            return 0;
        }
    }
}

