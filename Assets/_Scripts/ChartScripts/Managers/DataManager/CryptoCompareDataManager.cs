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
    public class CryptoCompareDataManager : IScalableDataManager
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

        private DateTime workBeginTime;
        public DateTime WorkBeginTime
        {
            get
            {
                return workBeginTime;
            }
        }

        private DateTime workEndTime;
        public DateTime WorkEndTime
        {
            get
            {                
                return workEndTime;
            }
        }

        private string url_base = "https://min-api.cryptocompare.com/data/";
        private string data_frame = "histoday";
        private const int MaxLimit = 2000;
        private string reciprocal_currency_acronym;
        private string base_currency_acronym;
        private string market_acronym ;
        public JsonData dc;

        
        bool initialized = false;

        public event Action WorkFlowChanged;

        public CryptoCompareDataManager(TimeFrame tframe, string base_currency_acronym = "BTC", string reciprocal_currency_acronym = "USD", string market_acronym = "Bitfinex")
        {
            Debug.Log("Пара: " + base_currency_acronym + " " + reciprocal_currency_acronym+ " Биржа: "+ market_acronym +" Тайм-фрейм:"+ tframe);
            tFrame = tframe;
            this.base_currency_acronym = base_currency_acronym;
            this.reciprocal_currency_acronym = reciprocal_currency_acronym;
            this.market_acronym = market_acronym;
            var e1 = getData(tframe);
            while (e1.MoveNext()) ;

        }

        public IEnumerator getData(TimeFrame timeFrame)
        {
            int limit = MaxLimit;
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
                    Debug.Log(www.text);
                    dc = JsonUtility.FromJson<JsonData>(www.text);
                    dataEndTime = DateTimeTools.TimestampToDate(dc.TimeTo);
                    if (dataEndTime != dataEndTime.FloorToTimeFrame(TFrame))
                        dataEndTime = dataEndTime.UpToNextFrame(TFrame);
                    workEndTime = dataEndTime;

                    int i = 0;
                    try
                    { while (dc.Data[i].open == 0 && dc.Data[i].close == 0) i++; }
                    catch
                    {
                        Debug.Log(base_currency_acronym + ":" + reciprocal_currency_acronym);
                        Debug.Log("Количество данных:" + dc.Data.Length);
                        for (int j = 0; j != dc.Data.Length; j++)
                        {
                            Debug.Log("Цена открытия "+j+"й свечи:"+dc.Data[j].open + " Дата:"+ DateTimeTools.TimestampToDate(dc.Data[j].time));
                        }
                        throw new ArgumentOutOfRangeException("dc.Data вышла за границы");
                    }
                    dataBeginTime = DateTimeTools.TimestampToDate(dc.Data[i].time).FloorToTimeFrame(TFrame);
                    workBeginTime = dataBeginTime;

                    candles = new List<PriceFluctuation>();
                    DateTime dt_current = dataBeginTime;
                    DateTime dt_next;
                    double volume, open, close, low, high;

                   
                    open = high = low = close = volume = 0;
                    double next_date_ts;
                    while (dt_current <= DataEndTime)
                    {
                        dt_next = dt_current.UpToNextFrame(TFrame);
                        next_date_ts = DateTimeTools.DateToTimestamp(dt_next);
                        if (i != dc.Data.Length && dc.Data[i].time < next_date_ts)
                        {

                            open = dc.Data[i].open;
                            low = dc.Data[i].low;
                            high = dc.Data[i].high;
                            volume = dc.Data[i].volumefrom;
                            i++;
                        }

                        while (i != dc.Data.Length && dc.Data[i].time < next_date_ts)
                        {
                            if (dc.Data[i].high > high) high = dc.Data[i].high;
                            if (dc.Data[i].low < low) low = dc.Data[i].low;
                            volume += dc.Data[i].volumefrom;
                            i++;
                        }

                        if (i == dc.Data.Length || dc.Data[i].time >= next_date_ts)
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
            if (fromTime > DataBeginTime || toTime < DataEndTime)
            {
                Debug.LogError("Рабочая временная область не должна выходить за временную область данных");
                return;
            }

            workBeginTime = fromTime;
            workEndTime = toTime;
            if (WorkFlowChanged != null) WorkFlowChanged();
        }
        public void ResetWorkDataRange()
        {
            workBeginTime = dataBeginTime;
            workEndTime = dataEndTime;
           
            if(WorkFlowChanged!=null) WorkFlowChanged();
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
            if (tmp < DataBeginTime || tmp2 > DataEndTime)
            {
                Debug.LogError("Рабочая временная область не должна выходить за временную область данных");
                return;
            }

            workBeginTime = tmp;
            workEndTime = tmp2;
            if (WorkFlowChanged != null) WorkFlowChanged();
        }


        public bool AddTimeStep()
        {
            DateTime tmp = workEndTime + tFrame;
            if (tmp > candles.Last().PeriodBegin.UpToNextFrame(TFrame))
            {
                workEndTime = candles.Last().PeriodBegin.UpToNextFrame(TFrame);
                if (WorkFlowChanged != null) WorkFlowChanged();
                return false;
            }
            else
            {
                workEndTime = tmp;
                if (WorkFlowChanged != null) WorkFlowChanged();
                return true;
            }

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

