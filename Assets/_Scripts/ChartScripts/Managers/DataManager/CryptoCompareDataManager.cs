using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;
using Chart.Entity;

namespace Chart
{
    public class CryptoCompareDataManager : IChartDataManager, IDateWorkFlow
    {
        List<PriceFluctuation> candles;

        TimeFrame tFrame;
        public TimeFrame TFrame
        {

            get { return tFrame; }
            set
            {
                tFrame = value;
            }
        }

        private DateTime dataBeginTime;
        public DateTime DataBeginTime
        {
            get
            {
                return dataBeginTime;
            }
        }

        private DateTime dataEndTime;
        public DateTime DataEndTime
        {
            get
            {
                //EndTime при инициализации возможно будет отличаться после резета
                return dataEndTime;
            }

        }


        private string url_base = "https://min-api.cryptocompare.com/data/";
        private string data_frame = "histoday";
        private string reciprocal_currency_acronym;
        private string base_currency_acronym;
        private string market_acronym ;
        public JsonData dc;

        
        bool initialized = false;

        public CryptoCompareDataManager(TimeFrame tframe, string base_currency_acronym = "BTC", string reciprocal_currency_acronym = "USD", string market_acronym = "Bitfinex")
        {
            tFrame = tframe;
            this.base_currency_acronym = base_currency_acronym;
            this.reciprocal_currency_acronym = reciprocal_currency_acronym;
            this.market_acronym = market_acronym;
            var e1 = getData(tframe);
            while (e1.MoveNext()) ;

        }

        public IEnumerator getData(TimeFrame timeFrame)
        {
            int limit = int.MaxValue;
            switch (timeFrame.period)
            {

                case Period.Minute:
                    {
                        data_frame = "histominute";
                    }
                    break;
                case Period.Hour:
                    {
                        data_frame = "histohour";
                    }
                    break;
                case Period.Day:
                    {
                        data_frame = "histoday";
                    }
                    break;
                default:
                    {
                        Debug.LogError("Для тайм фрейма с периодом " + timeFrame.period + " данные не доступны");
                        yield break;
                    }
            }

            string url = url_base + data_frame + "?fsym=" + base_currency_acronym +
                "&tsym=" + reciprocal_currency_acronym +
                "&limit=" + limit.ToString() +
                //"&aggregate=" + aggregate.ToString() + 
                "&e=" + market_acronym;
            using (WWW www = new WWW(url))
            {
                yield return www;

                while (!www.isDone) ;

                if (www.error != null)
                {
                    throw new Exception(www.error);
                }
                else
                {
                    dc = JsonUtility.FromJson<JsonData>(www.text);
                    dataEndTime = DateTimeTools.TimestampToDate(dc.TimeTo);
                    dataBeginTime = DateTimeTools.TimestampToDate(dc.TimeFrom);


                    candles = new List<PriceFluctuation>();
                    DateTime dt_current = dataBeginTime;
                    DateTime dt_next;
                    double volume, open, close, low, high;

                    int i = 0;
                    open = high = low = close = volume = 0;
                    while (dt_current <= DataEndTime)
                    {
                        dt_next = dt_current.UpToNextFrame(TFrame);

                        if (i != dc.Data.Length && dc.Data[i].time < DateTimeTools.DateToTimestamp(dt_next))
                        {
                            open = dc.Data[i].open;
                            low = dc.Data[i].low;
                            high = dc.Data[i].high;
                            volume = dc.Data[i].volumefrom;
                            i++;
                        }

                        while (i != dc.Data.Length && dc.Data[i].time < DateTimeTools.DateToTimestamp(dt_next))
                        {
                            if (dc.Data[i].high > high) high = dc.Data[i].high;
                            if (dc.Data[i].low < low) low = dc.Data[i].low;
                            volume += dc.Data[i].volumefrom;
                            i++;
                        }

                        if (i == dc.Data.Length || dc.Data[i].time >= DateTimeTools.DateToTimestamp(dt_next))
                            close = dc.Data[i - 1].close;

                        if (open != 0)
                            candles.Add(new PriceFluctuation(dt_current, volume, open, close, low, high));

                        dt_current = dt_next;
                        open = high = low = close = volume = 0;
                    }
                    www.Dispose();
                    initialized = true;
                }

            }
        }


        public PriceFluctuation GetPriceFluctuation(DateTime dateTime)
        {
            if (!initialized) return null;
            PriceFluctuation result;
            int i = candles.Count;
            for (; i > 0 && candles[i - 1].PeriodBegin > dateTime; i--) ;
            if (i > 0)
            {
                result = candles[i - 1];
                return result;
            }
            return null;
        }

        public IEnumerable<PriceFluctuation> GetPriceFluctuationsByTimeFrame(DateTime fromDate, DateTime toDate)
        {
            if (!initialized) return null;

            if (DataBeginTime > fromDate) fromDate = DataBeginTime;
            if (DataEndTime < toDate) toDate = DataEndTime;
            return candles.Where(candle => candle.PeriodBegin >= fromDate && candle.PeriodBegin <= toDate);
        }

        public void SetWorkDataRange(DateTime fromTime, DateTime toTime)
        {
            if (toTime < fromTime)
            {
                DateTime tmp = fromTime;
                fromTime = toTime;
                toTime = tmp;
            }
            if (fromTime > candles[0].PeriodBegin || toTime < candles.Last().PeriodBegin)
            {
                Debug.LogError("Рабочая временная область не должна выходить за временную область данных");
                return;
            }

            dataBeginTime = fromTime;
            dataEndTime = toTime;
        }
        public void ResetWorkDataRange()
        {
            dataBeginTime = candles[0].PeriodBegin;
            dataEndTime = candles.Last().PeriodBegin.UpToNextFrame(TFrame);
        }
        public void SetWorkDataRange(int startFluctuationNumber, int loadFluctuationCount)
        {
            if (startFluctuationNumber < 0 || loadFluctuationCount < 1)
            {
                Debug.LogError("Стартовый номер ценового колебания должен быть >=0, а количество >1");
                return;
            }
            DateTime tmp = candles[0].PeriodBegin + startFluctuationNumber * TFrame;
            DateTime tmp2 = tmp + (loadFluctuationCount-1) * TFrame;
            if (tmp < dataBeginTime || tmp2 > dataEndTime)
            {
                Debug.LogError("Рабочая временная область не должна выходить за временную область данных");
                return;
            }

            dataBeginTime = tmp;
            dataEndTime = tmp2;
        }

        //TODO: Прикутить ограничение
        /*
        DateTime dateLimit; 
        public DateTime DateLimit {
            get { return dateLimit; }
            set
            {
                DateTime tmp = candles.Last().PeriodBegin.UpToNextFrame(TFrame);
                dateLimit = value > tmp ? tmp: value ;
                if (value > tmp) Debug.Log("Значение DateLimit превышает дату закрытия последнего колебания, поэтому ему была присвоена дата закрытия");
            }
        }
        public int MaxCountFluctuationToShow { set { DateLimit = DataBeginTime + value * TFrame; } }
        //*/

        public bool AddTimeStep()
        {
            DateTime tmp = dataEndTime + tFrame;
            if (tmp > candles.Last().PeriodBegin.UpToNextFrame(TFrame))
            {
                dataEndTime = candles.Last().PeriodBegin.UpToNextFrame(TFrame);
                return false;
            }
            else
            {
                dataEndTime = tmp;
                return true;
            }
        }

        public ExtraData GetDataByPoint(DateTime dateTime)
        {
            throw new NotImplementedException();
        }


        [System.Serializable]
        public class TradeData
        {
            public long time;
            public double close;
            public double high;
            public double low;
            public double open;
            public double volumefrom;
            public double volumeto;
        }
        [System.Serializable]
        public class JsonData
        {
            public TradeData[] Data;
            public long TimeFrom;
            public long TimeTo;

        }
    }

}

