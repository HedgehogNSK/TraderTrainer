using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;
using Chart;
using Chart.Entity;
using ChartGame.Entity;
using UnityNpgsql;
using UnityNpgsqlTypes;
using Microsoft;
public class SQLChartDataManager : IChartDataManager
{

    NpgsqlConnection dbcon;

    ~SQLChartDataManager()
    {
        dbcon.Close();
    }

    public SQLChartDataManager()
    {
        string connectionString =
          "Server=localhost;" +
          "Database=game;" +
          "User ID=user;" +
          "Password=F1ndS0me)neElse;";


        dbcon = new NpgsqlConnection(connectionString);
        try
        {
            dbcon.Open();
        }
        catch
        {
            Debug.LogError("Не удалось подключиться к базе");

        }
    }

    public SQLChartDataManager(TimeFrame tframe) :this()
    {
        this.tFrame = tframe;
    }

    public int PairID{ get; set; }

    public bool IsSettingsSet()
    {
        if (TFrame.count == 0 || PairID <= 0)
        {
            string errMsg = TFrame.count == 0 ? "Нет данных о тайм-фрейме" :
                "Не задана или не найдена пара";
            Debug.LogError(errMsg);
            return false;
        }

        return true;
    }

    public bool TryToSetPairByAcronym(string base_currency_acronym, string reciprocal_currency_acronym)
    {
        using (NpgsqlCommand cmd = new NpgsqlCommand(@"SELECT pair_id 
           FROM currency_pairs  
           WHERE base_currency_short = @base_currency_acronym AND 
           reciprocal_currency_short = @reciprocal_currency_acronym;", dbcon))
        {
            cmd.Parameters.AddWithValue("@base_currency_acronym", NpgsqlDbType.Varchar,10,base_currency_acronym);
            cmd.Parameters.AddWithValue("@reciprocal_currency_acronym", NpgsqlDbType.Varchar,10, reciprocal_currency_acronym);

            try
            {
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    PairID = reader.GetInt32(0);
                    return true;
                }
            }
            catch
            {
                return false;
            }
           
        }
    }
    
    TimeFrame tFrame;
    public TimeFrame TFrame
    {
        get { return tFrame; }
        set
        {
            tFrame = value;
        }
    }

    private DateTime? dataBeginTime;
    public DateTime DataBeginTime { get
        {
            if (dataBeginTime == null)
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(@"SELECT MIN(date) 
             FROM trades 
             WHERE pair_id = @pair_id;", dbcon))
                {
                    cmd.Parameters.AddWithValue("@pair_id", NpgsqlDbType.Integer, PairID);
                    try
                    { using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        dataBeginTime = reader.GetDateTime(0);
                    } }
                    catch
                    {
                        Debug.LogError("Минимальная дата для данной пары не найдена");                    
                    }
                }

            }
            return (DateTime)dataBeginTime;
        } }

    private DateTime? dataEndTime;
    public DateTime DataEndTime { get
        {
            if (dataEndTime==null)
            {
             using (NpgsqlCommand cmd = new NpgsqlCommand(@"SELECT MAX(date) 
             FROM trades 
             WHERE pair_id = @pair_id;", dbcon))
                {
                    cmd.Parameters.AddWithValue("@pair_id",NpgsqlDbType.Integer, PairID);
                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            dataEndTime = reader.GetDateTime(0);
                         
                        }
                    }
                    catch
                    {
                        Debug.LogError("Максимальная дата для данной пары не найдена");
                    }
                }             
            
            }
            return (DateTime)dataEndTime;
        } }

    public PriceFluctuation GetPriceFluctuation(DateTime timestamp)
    { 
        DateTime periodEnd;
        periodEnd = timestamp.UpToNextFrame(TFrame);

        using (NpgsqlCommand cmd = new NpgsqlCommand(@"SELECT price, date, volume
FROM trades
WHERE pair_id = @pair_id AND date >= @earliest_date AND date < @latest_date
ORDER BY date ASC;"
             , dbcon))
        {
            cmd.Parameters.AddWithValue("@periodBegin", NpgsqlDbType.Date, 0, timestamp);
            cmd.Parameters.AddWithValue("@periodEnd", NpgsqlDbType.Date, 0, periodEnd);
            cmd.Parameters.AddWithValue("@pair_id", NpgsqlDbType.Integer, PairID);

            try
            {
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    return new PriceFluctuation(timestamp, reader.GetDouble(0), reader.GetDouble(3), reader.GetDouble(4), reader.GetDouble(1), reader.GetDouble(2));
                }
            }
            catch
            {
                Debug.Log("Не удалось получить данные о цене с периода " + timestamp.ToString());
                return null;

            }
        }
       
    }

    //TODO: сделать кэширование
    public IEnumerable<PriceFluctuation> GetPriceFluctuationsByTimeFrame(DateTime fromDate, DateTime toDate)
    {
        if (fromDate >= toDate) Debug.LogWarning("FromDate больше или совпадает с toDate");

        List<PriceFluctuation> pricesFluct = new List<PriceFluctuation>();
        List<Trade> trades = new List<Trade>();
        DateTime endDate = fromDate <= toDate ? toDate.UpToNextFrame(TFrame) : fromDate.UpToNextFrame(TFrame);
        DateTime beginDate = fromDate <= toDate ? fromDate.FloorToTimeFrame(TFrame) : toDate.FloorToTimeFrame(TFrame);
        using (NpgsqlCommand cmd = new NpgsqlCommand(@"SELECT price, date, volume
FROM trades
WHERE pair_id = @pair_id AND date >= @earliest_date AND date < @latest_date
ORDER BY date ASC;"
, dbcon))
        {
            cmd.Parameters.AddWithValue("@earliest_date", NpgsqlDbType.Date, 0, fromDate < toDate ? fromDate : endDate);
            cmd.Parameters.AddWithValue("@latest_date", NpgsqlDbType.Date, 0, fromDate < toDate ? toDate: beginDate);
            cmd.Parameters.AddWithValue("@pair_id", NpgsqlDbType.Integer, PairID);

            try
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        trades.Add(new Trade(reader.GetDouble(0), reader.GetDateTime(1), reader.GetDouble(2)));
                        //Debug.Log(reader.GetDouble(0) + " " + reader.GetDouble(2) + " " + reader.GetDateTime(1));
                    }

                    if (trades.Count!=0)
                    {
                        DateTime current = beginDate;
                        while (current < endDate)
                        {
                            DateTime next = current.UpToNextFrame(TFrame);

                            var currentTrades = trades.Where(trade => trade.DTime >= current && trade.DTime < next);

                            if (currentTrades.FirstOrDefault() != null)
                            {
                                double open = currentTrades.FirstOrDefault().Price;
                                double close = currentTrades.LastOrDefault().Price;
                                double min = currentTrades.Min(x => x.Price);
                                double max = currentTrades.Max(x => x.Price);
                                double volume = currentTrades.Sum(x => x.Volume);
                                pricesFluct.Add(new PriceFluctuation(current, volume, open, close, min, max));
                            }
                            else
                            {
                                pricesFluct.Add(new PriceFluctuation(current, 0, 0, 0, 0, 0));
                            }
                            current = next;
                         }

                    }
                    // return new PriceFluctuation(timestamp, reader.GetDouble(0), reader.GetDouble(3), reader.GetDouble(4), reader.GetDouble(1), reader.GetDouble(2));
                }
            }
            catch
            {
                Debug.Log("Не удалось получить данные о цене с периода " + fromDate.ToString());
                return null;

            }
        }


        return pricesFluct;
    }

}

