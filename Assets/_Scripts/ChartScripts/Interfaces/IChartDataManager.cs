using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;

namespace Chart
{
    public enum From
    {
        First,
        Last,
    }

    //Интерфейс предназначен для управления данными изменения цены со временем
    public interface IChartDataManager
    {
        //Задаёт тайм-фрейм на котором интерфейс должен оперировать в текущий момент
        TimeFrame TFrame { get; set; }
        DateTime DataBeginTime { get; }
        DateTime DataEndTime { get; }
        
        //По timestamp возвращает данные о колебании цены в рамках таймфрейма
        Entity.PriceFluctuation GetFluctuation(DateTime dateTime);
        //По возвращает данные околибании цен в рамках таймфрейма
        IEnumerable<Entity.PriceFluctuation> GetPriceFluctuationsByTimeFrame(DateTime fromDate, DateTime toDate);
        //IEnumerable<Entity.PriceFluctuation> GetPriceFluctuationsByTimeFrame(From position, int framesCount = int.MaxValue);
        Entity.ExtraData GetDataByPoint(DateTime dateTime);

        
    }
}

