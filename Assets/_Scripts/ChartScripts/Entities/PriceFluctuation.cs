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
            DateTime periodBegin;
            double volume;
            Dictionary<int, object> extraData;

            public DateTime PeriodBegin { get { return periodBegin; } }
            public double Open { get { return open; } }
            public double Close { get { return close; } }
            public double Low { get { return low; } }
            public double High { get { return high; } }
            public double Volume { get { return volume; } }
            public Dictionary<int, object> ExtraData { get { return extraData; } }




            public PriceFluctuation(DateTime periodBegin, double volume, double open, double close=0,  double low = 0, double high = 0)
            {
                this.periodBegin = periodBegin;
                this.volume = volume;
                this.open = open;
                this.close = close;
                this.low = low;
                this.high = high;
                extraData = new Dictionary<int, object>();

            }           

            public override string ToString()
            {
                return "Начало периода " + PeriodBegin.Year
                    + "-" + PeriodBegin.Month + "-" + PeriodBegin.Day + " "
                    + PeriodBegin.Hour +":"+ PeriodBegin.Minute+ ":" + PeriodBegin.Second
                     + "\n Цена открытия: " + Open
                     + "\n Цена закрытия: " + Close
                     + "\n Минимальная цена: " + Low
                     + "\n Максимальная цена: " + High
                     + "\n Объём торгов: " + Volume;
            }

        }
    } }