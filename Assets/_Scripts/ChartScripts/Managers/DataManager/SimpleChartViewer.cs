using System;
using System.Collections;
using System.Collections.Generic;
using Hedge.Tools;
using Chart;
using Chart.Entity;
using UnityEngine;
using System.Linq;

public class SimpleChartViewer : IChartDataManager
{
    Trade[] trades;
    int current = 0;
    int tFrameCount;

    public int TFrameCount {get{return tFrameCount; }}

    public DateTime DataBeginTime
    {
        get
        {
            return trades[0].DTime;
        }
    }

    public DateTime DataEndTime
    {
        get
        {
            return trades[trades.Length-1].DTime;
        }
    }

    private TimeFrame tFrame;
    public TimeFrame TFrame {
        get { return tFrame; }
        set
        {
            tFrame = value;
            tFrameCount = DateTimeTools.CountFramesInPeriod(TFrame, trades[0].DTime, trades[trades.Length-1].DTime, TimeSpan.Zero);
        }
    }

    public SimpleChartViewer(int size, TimeFrame timeFrame)
    {
        trades = new Trade[size];
        for(int id = 0; id!=size; id++)
        {
            trades[id] = new Trade(Mathf.Sin(id)+ id/100 , new DateTime(2011,1,1).AddHours(id), 1);
        }

        TFrame = timeFrame;
    }

    public Trade GetCurrentPoint()
    {
        if (trades != null)
            return trades[current];
        else
            return null;
    }
    public double GetPrice(double timestamp)
    {
        int id = ArrayTools.BinarySearch(trades, timestamp);
        if (id < 0)
        {
            Debug.Log("Элемент не найден");
            return -1;
        }
        return trades[id].Price;
    }

    public PriceFluctuation GetPriceFluctuation(DateTime timestamp)
    {
        if (!IsSettingsSet()) return null;

       // int id = ArrayTools.BinarySearch(points, timestamp);
       //if (id < 0)
       // {
       //     Debug.Log("Элемент не найден");
       //     return null;
       // }
        
        DateTime periodBegin = timestamp.FloorToTimeFrame(tFrame);
        DateTime periodEnd = timestamp.UpToNextFrame(tFrame);

        return GetPriceFluctuation(periodBegin, periodEnd);
    }

    private PriceFluctuation GetPriceFluctuation(DateTime periodBegin, DateTime periodEnd)
    {
        double high, low, open, close;
        double volume = 0;
        //Поиск свечки через Linq
        var pointsQuery = trades.Where(point => point.DTime >= periodBegin && point.DTime < periodEnd);

        high = pointsQuery.Max(x => x.Price);
        low = pointsQuery.Min(x => x.Price);
        open = pointsQuery.LastOrDefault().Price;
        close = pointsQuery.FirstOrDefault().Price;
        volume = pointsQuery.Sum(x => x.Volume);

        //Поиск свечки через массив
        //Вычисляем разбор цены на фрейме/считаем свечки
        //high = low = open = close = points[id].Price;
        //for (int jd = id + 1; jd < points.Length && points[jd].DTime < periodEnd; jd++)
        //{

        //    if (points[jd].Price > high) high = points[jd].Price;
        //    if (points[jd].Price < low) low = points[jd].Price;
        //    close = points[jd].Price;
        //    volume += points[jd].Volume;

        //}
        return new PriceFluctuation(periodBegin, volume, close, open, low, high);
    }

    public bool IsSettingsSet()
    {
        if (TFrame.count == 0 || trades == null || tFrameCount <=0)
        {
            string errMsg = TFrame.count == 0 ? "Нет данных о тайм-фрейме" : 
                trades == null? "Нет данных о цене и стоимости": 
                "Не верное количество фреймов";
            Debug.LogError(errMsg);
            return false;
        }

        return true;
    }

    IEnumerable<PriceFluctuation> GetPriceFluctationInASC(int framesCount)
    {

        if (!IsSettingsSet()) return null;

        PriceFluctuation[] candles = new PriceFluctuation[framesCount];
        double high, low, open, close;
        DateTime periodBegin, periodEnd;
        double volume;


        int jd = 0;
        for (int id = 0; id != candles.Length; id++)
        {
            periodBegin = trades[jd].DTime.FloorToTimeFrame(TFrame);
            periodEnd = trades[jd].DTime.UpToNextFrame(TFrame);
            high = low = open = close = trades[jd].Price;
            volume = trades[jd].Volume;
            for (jd = jd + 1; jd < trades.Length && trades[jd].DTime < periodEnd; jd++)
            {

                if (trades[jd].Price > high) high = trades[jd].Price;
                if (trades[jd].Price < low) low = trades[jd].Price;
                close = trades[jd].Price;
                volume += trades[jd].Volume;

            }
            candles[id] = new PriceFluctuation(periodBegin, volume, close, open, low, high);
        }
        current = jd;
        return candles;
    }
    IEnumerable<PriceFluctuation> GetPriceFluctationInDESC(int framesCount)
    {

        if (!IsSettingsSet()) return null;

        PriceFluctuation[] candles = new PriceFluctuation[framesCount];
        double high, low, open, close;
        DateTime periodBegin; //periodEnd;
        double volume;

        int jd = trades.Length - 1;

        for (int id = candles.Length - 1; id != -1; id--)
        {
            periodBegin = trades[jd].DTime.FloorToTimeFrame(TFrame);
            //periodEnd = trades[jd].DTime.UpToNextFrame(TFrame);

            high = low = open = close = trades[jd].Price;
            volume = trades[jd].Volume;

            for (jd = jd - 1; jd != -1 && trades[jd].DTime > periodBegin; jd--)
            {

                if (trades[jd].Price > high) high = trades[id].Price;
                if (trades[jd].Price < low) low = trades[id].Price;
                open = trades[jd].Price;
                volume += trades[jd].Volume;

            }
            candles[id] = new PriceFluctuation(periodBegin, volume, close, open, low, high);
        }
        current = jd;
        return candles;
    }
    public IEnumerable<PriceFluctuation> GetPriceFluctuationsByTimeFrame(From position, int framesCount = int.MaxValue)
    {
        if (!IsSettingsSet()) return null;
        
        //Если запрос загрузить количество свечей превышает количество доступных свечей
        if (framesCount > tFrameCount) framesCount = tFrameCount;

        PriceFluctuation[] candles;

        switch (position)
        {
            case From.First:
                {
                    candles = GetPriceFluctationInASC(framesCount).ToArray();
                    
                } break;
            case From.Last:
                {
                    candles = GetPriceFluctationInDESC(framesCount).ToArray();
                }
                break;

            default: {
                    Debug.LogError("Не описано действие для этого положения");
                    return null;
                }
        }
        return candles;
    }

    public IEnumerable<PriceFluctuation> GetPriceFluctuationsByTimeFrame(DateTime firstDate, DateTime secondDate)
    {
        if (!IsSettingsSet()) return null;


        double open, close, min, max, volume;
        List <PriceFluctuation> candles = new List<PriceFluctuation>();

        DateTime next_period_begin;
        for (DateTime current_period_begin = (firstDate <= secondDate ? firstDate : secondDate); current_period_begin <= (firstDate > secondDate ? firstDate : secondDate); current_period_begin = next_period_begin)
        {
            next_period_begin = current_period_begin.UpToNextFrame(TFrame);
            var request = trades.Where(point => point.DTime >= current_period_begin && point.DTime < next_period_begin).OrderBy(point => point.DTime);
            if (request.NotNullOrEmpty())
            {
                open = request.FirstOrDefault().Price;
                close = request.LastOrDefault().Price;
                max = request.Max(trade => trade.Price);
                min = request.Min(trade => trade.Price);
                volume = request.Sum(trade => trade.Volume);
                candles.Add(new PriceFluctuation(current_period_begin, volume, open, close, min, max));
            }
        }

        return candles;

    }

    public ExtraData GetDataByPoint(DateTime dateTime)
    {
        throw new NotImplementedException();
    }
}
