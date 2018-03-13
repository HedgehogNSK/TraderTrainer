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
            //double periodBeginTimestamp;
            DateTime periodBegin;
            double volume;

            //public double PeriodBegin { get { return periodBeginTimestamp; } }
            public DateTime PeriodBegin { get { return periodBegin; } }
            public double Open { get { return open; } }
            public double Close { get { return close; } }
            public double Low { get { return low; } }
            public double High { get { return high; } }
            public double Volume { get { return volume; } }

            /*public PriceFluctuation(double periodBeginTimestamp, double volume, double close, double open = 0, double low = 0, double high = 0)
            {
                this.periodBeginTimestamp = periodBeginTimestamp;
                this.volume = volume;
                this.open = open;
                this.close = close;
                this.low = low;
                this.high = high;

            }*/

            public PriceFluctuation(DateTime periodBegin, double volume, double open, double close=0,  double low = 0, double high = 0)
            {
                this.periodBegin = periodBegin;
                this.volume = volume;
                this.open = open;
                this.close = close;
                this.low = low;
                this.high = high;

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