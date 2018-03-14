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
    Trade[] points;
    int current = 0;
    int tFrameCount;

    public int TFrameCount {get{return tFrameCount; }}

    public DateTime ChartBeginTime
    {
        get
        {
            return points[0].DTime;
        }
    }

    public DateTime ChartEndTime
    {
        get
        {
            return points[points.Length-1].DTime;
        }
    }

    private TimeFrame tFrame;
    public TimeFrame TFrame {
        get { return tFrame; }
        set
        {
            tFrame = value;
            tFrameCount = DateTimeTools.CountFramesInPeriod(TFrame, points[0].DTime, points[points.Length-1].DTime, TimeSpan.Zero);
        }
    }

    public SimpleChartViewer(int size, TimeFrame timeFrame)
    {
        points = new Trade[size];
        for(int id = 0; id!=size; id++)
        {
            points[id] = new Trade(Mathf.Sin(id)+ id/100 , new DateTime(10,1,1).AddMinutes(id), 1);
        }

        TFrame = timeFrame;
    }

    public Trade GetCurrentPoint()
    {
        if (points != null)
            return points[current];
        else
            return null;
    }
    public double GetPrice(double timestamp)
    {
        int id = ArrayTools.BinarySearch(points, timestamp);
        if (id < 0)
        {
            Debug.Log("Элемент не найден");
            return -1;
        }
        return points[id].Price;
    }

    public PriceFluctuation GetFluctuation(DateTime timestamp)
    {
        if (!IsSettingsSet()) return null;

        int id = ArrayTools.BinarySearch(points, timestamp);
        if (id < 0)
        {
            Debug.Log("Элемент не найден");
            return null;
        }
        
        DateTime periodBegin = timestamp.FloorToTimeFrame(tFrame);
        DateTime periodEnd = timestamp.UpToNextFrame(tFrame);

        return GetPriceFluctuation(periodBegin, periodEnd);
    }

    private PriceFluctuation GetPriceFluctuation(DateTime periodBegin, DateTime periodEnd)
    {
        double high, low, open, close;
        double volume = 0;
        //Поиск свечки через Linq
        var pointsQuery = points.Where(point => point.DTime >= periodBegin && point.DTime < periodEnd);

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
        if (TFrame.count == 0 || points == null || tFrameCount <=0)
        {
            string errMsg = TFrame.count == 0 ? "Нет данных о тайм-фрейме" : 
                points == null? "Нет данных о цене и стоимости": 
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
            periodBegin = points[jd].DTime.FloorToTimeFrame(TFrame);
            periodEnd = points[jd].DTime.UpToNextFrame(TFrame);
            high = low = open = close = points[jd].Price;
            volume = points[jd].Volume;
            for (jd = jd + 1; jd < points.Length && points[jd].DTime < periodEnd; jd++)
            {

                if (points[jd].Price > high) high = points[jd].Price;
                if (points[jd].Price < low) low = points[jd].Price;
                close = points[jd].Price;
                volume += points[jd].Volume;

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
        DateTime periodBegin, periodEnd;
        double volume;

        int jd = points.Length - 1;

        for (int id = candles.Length - 1; id != -1; id--)
        {
            periodBegin = points[jd].DTime.FloorToTimeFrame(TFrame);
            periodEnd = points[jd].DTime.UpToNextFrame(TFrame);

            high = low = open = close = points[jd].Price;
            volume = points[jd].Volume;

            for (jd = jd - 1; jd != -1 && points[jd].DTime > periodBegin; jd--)
            {

                if (points[jd].Price > high) high = points[id].Price;
                if (points[jd].Price < low) low = points[id].Price;
                open = points[jd].Price;
                volume += points[jd].Volume;

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
                } break;
        }
        return candles;
    }

    public IEnumerable<PriceFluctuation> GetPriceFluctuationsByTimeFrame(DateTime firstDate, DateTime secondDate)
    {
        if (!IsSettingsSet()) return null;

        //Вычисляем количество свечей в периоде
        /*int count= DateTimeInstruments.CountFramesInPeriod(TFrame, firstDate, secondDate, TimeSpan.Zero);
        if (count < 0)
        {
            Debug.Log("Не верное количество фреймов в периоде");
            return null;
        }
        //Если запрос загрузить количество свечей превышает количество доступных свечей
        if (count > TFrameCount) count = TFrameCount;*/

        double open, close, min, max, volume;
        List <PriceFluctuation> candles = new List<PriceFluctuation>();

        DateTime next_period_begin;
        for (DateTime current_period_begin = (firstDate <= secondDate ? firstDate : secondDate); current_period_begin <= (firstDate > secondDate ? firstDate : secondDate); current_period_begin = next_period_begin)
        {
            next_period_begin = current_period_begin.UpToNextFrame(TFrame);
            var request = points.Where(point => point.DTime >= current_period_begin && point.DTime < next_period_begin).OrderBy(point => point.DTime);
            if (request != null)
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
