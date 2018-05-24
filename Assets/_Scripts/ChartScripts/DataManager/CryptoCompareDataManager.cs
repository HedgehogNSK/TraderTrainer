using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;
using Chart;
using Chart.Entity;

public class CryptoCompareDataManager : IChartDataManager
{
    TimeFrame tFrame;
    public TimeFrame TFrame
    {

        get { return tFrame; }
        set
        {
            tFrame = value;
        }
    }

    private DateTime chartBeginTime;
    public DateTime ChartBeginTime
    {
        get
        {
            return chartBeginTime;
        }
    }

    private DateTime chartEndTime;
    public DateTime ChartEndTime
    {
        get
        {
            
            return chartEndTime;
        }
    }

    private string url_base = "https://min-api.cryptocompare.com/data/";
    private string data_frame = "histoday";
    private string reciprocal_currency_acronym = "USD";
    private string base_currency_acronym = "BTC";
    private string market_acronym = "Bitfinex";
    public JsonData dc;

    bool initialized = false;

    public CryptoCompareDataManager(TimeFrame tframe)
    {
        tFrame = tframe;
        var e1 = getData(tframe);
        while (e1.MoveNext());
    }
    public CryptoCompareDataManager(TimeFrame tframe,string base_currency_acronym, string reciprocal_currency_acronym ="USD", string market_acronym = "CCCAGG")
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
                } break;
            case Period.Hour:
                {
                    data_frame = "histohour";
                } break;
            case Period.Day:
                {
                    data_frame = "histoday";
                } break;
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
                chartEndTime = DateTimeTools.TimestampToDate(dc.TimeTo);
                chartBeginTime = DateTimeTools.TimestampToDate(dc.TimeFrom);


                candles = new List<PriceFluctuation>();
                DateTime dt_current = chartBeginTime;
                DateTime dt_next;
                IEnumerable<TradeData> tradeData = dc.Data;
                double volume, open, close, low, high;

                int i = 0;
                open = high = low = close = volume = 0;
                while (dt_current<=chartEndTime)
                {
                    dt_next = dt_current.UpToNextFrame(TFrame);

                    if (i != dc.Data.Length && dc.Data[i].time < DateTimeTools.DateToTimestamp(dt_next))
                    { open = dc.Data[i].open;
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
                    close = dc.Data[i-1].close;

                    if (open!=0)
                        candles.Add(new PriceFluctuation(dt_current, volume, open, close, low, high));

                    dt_current = dt_next;
                    open = high = low = close = volume = 0;
                }
                /*for(int j= dc.Data.Length-1;j > dc.Data.Length - 11; j-- )
                {
                    Debug.Log(DateTimeTools.TimestampToDate(dc.Data[j].time) +" Open:" +dc.Data[j].open + 
                        " Close:" + dc.Data[j].close + 
                        " Low:" + dc.Data[j].low + 
                        " High:" + dc.Data[j].high);
                    Debug.Log(candles[j].PeriodBegin + 
                        " Open:" + candles[j].Open +
                        " Close:" + candles[j].Close +
                        " Low:" + candles[j].Low +
                        " High:" + candles[j].High);
                }*/
                initialized = true;
            }

        }
    }
    List<PriceFluctuation> candles;

    public ExtraData GetDataByPoint(DateTime dateTime)
    {
        
        throw new NotImplementedException();
    }

    public PriceFluctuation GetFluctuation(DateTime dateTime)
    {
        if (!initialized) return null;
        PriceFluctuation result;
        int i = candles.Count;
        for (; i >0 && candles[i-1].PeriodBegin > dateTime; i--) ;
        if (i > 0)
        { result = candles[i - 1];
            return result;
        }
        return null;    
    }

    public IEnumerable<PriceFluctuation> GetPriceFluctuationsByTimeFrame(DateTime fromDate, DateTime toDate)
    {
        if (!initialized) return null;

        return candles.Where(candle => candle.PeriodBegin >= fromDate && candle.PeriodBegin<= toDate);
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

 

