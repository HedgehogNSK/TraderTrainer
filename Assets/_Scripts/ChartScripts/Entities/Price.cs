using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chart
{
    namespace Entity
    {
        public class PriceFluctuation
        {
            double open;
            double close;
            double low;
            double high;
            double periodBeginTimestamp;
            DateTime periodDateBegin;
            double volume;

            public double PeriodBegin { get { return periodBeginTimestamp; } }
            public DateTime PeriodDateBegin { get { return periodDateBegin; } }
            public double Open { get { return open; } }
            public double Close { get { return close; } }
            public double Low { get { return low; } }
            public double High { get { return high; } }
            public double Volume { get { return volume; } }
            public PriceFluctuation(double periodBeginTimestamp, double volume, double close, double open = 0, double low = 0, double high = 0)
            {
                this.periodBeginTimestamp = periodBeginTimestamp;
                this.volume = volume;
                this.open = open;
                this.close = close;
                this.low = low;
                this.high = high;

            }
            public PriceFluctuation(DateTime periodBegin, double volume, double open, double close=0,  double low = 0, double high = 0)
            {
                this.periodDateBegin = periodBegin;
                this.volume = volume;
                this.open = open;
                this.close = close;
                this.low = low;
                this.high = high;

            }

            public override string ToString()
            {
                return "Начало периода " + PeriodDateBegin.Year
                    + "-" + PeriodDateBegin.Month + "-" + PeriodDateBegin.Day + " "
                    + PeriodDateBegin.Hour +":"+ PeriodDateBegin.Minute+ ":" + PeriodDateBegin.Second
                     + "\n Цена открытия: " + Open
                     + "\n Цена закрытия: " + Close
                     + "\n Минимальная цена: " + Low
                     + "\n Максимальная цена: " + High
                     + "\n Объём торгов: " + Volume;
            }

        }
    } }