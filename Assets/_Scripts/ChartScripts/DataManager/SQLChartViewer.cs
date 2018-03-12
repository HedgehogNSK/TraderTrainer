using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;
using Chart;
using Chart.Entity;
using UnityNpgsql;
using UnityNpgsqlTypes;
using Microsoft;
public class SQLChartViewer : IChartViewer
{

    NpgsqlConnection dbcon;

    ~SQLChartViewer()
    {
        dbcon.Close();
    }

    public SQLChartViewer()
    {
        string connectionString =
          "Server=localhost;" +
          "Database=game;" +
          "User ID=user;" +
          "Password=F1ndS0me)neElse;";
        // IDbConnection dbcon; ## CHANGE THIS TO


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

    public SQLChartViewer(TimeFrame tframe) :this()
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

    public bool TyrToSetPairByAcronym(string base_currency_acronym, string reciprocal_currency_acronym)
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
    /* void Connection()
     {

         string connectionString =
           "Server=localhost;" +
           "Database=test;" +
           "User ID=postgres;" +
           "Password=fun2db;";
         // IDbConnection dbcon; ## CHANGE THIS TO
         NpgsqlConnection dbcon;

         dbcon = new NpgsqlConnection(connectionString);
         dbcon.Open();
         //IDbCommand dbcmd = dbcon.CreateCommand();## CHANGE THIS TO
         NpgsqlCommand dbcmd = dbcon.CreateCommand();
         // requires a table to be created named employee
         // with columns firstname and lastname
         // such as,
         //        CREATE TABLE employee (
         //           firstname varchar(32),
         //           lastname varchar(32));
         string sql =
             "SELECT firstname, lastname " +
             "FROM employee";
         dbcmd.CommandText = sql;
         //IDataReader reader = dbcmd.ExecuteReader(); ## CHANGE THIS TO
         NpgsqlDataReader reader = dbcmd.ExecuteReader();
         while (reader.Read())
         {
             string FirstName = (string)reader["firstname"];
             string LastName = (string)reader["lastname"];
             Console.WriteLine("Name: " +
                  FirstName + " " + LastName);
         }
         // clean up
         reader.Close();
         reader = null;
         dbcmd.Dispose();
         dbcmd = null;
         dbcon.Close();
         dbcon = null;
     }*/

    TimeFrame tFrame;
    public TimeFrame TFrame
    {
        get { return tFrame; }
        set
        {
            tFrame = value;
        }
    }

    public DateTime? chartBeginTime;
    public DateTime ChartBeginTime { get
        {
            if (chartBeginTime == null)
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
                        chartBeginTime = reader.GetDateTime(0);
                    } }
                    catch
                    {
                        Debug.LogError("Минимальная дата для данной пары не найдена");                    
                    }
                }

            }
            return (DateTime)chartBeginTime;
        } }

    public DateTime? chartEndTime;
    public DateTime ChartEndTime { get
        {
            if (chartEndTime==null)
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
                            chartEndTime = reader.GetDateTime(0);
                         
                        }
                    }
                    catch
                    {
                        Debug.LogError("Максимальная дата для данной пары не найдена");
                    }
                }             
            
            }
            return (DateTime)chartEndTime;
        } }

    public ExtraData GetDataByPoint(double timestamp)
    {
        throw new System.NotImplementedException();
    }

    public DateTime GetPrice(double timestamp)
    {
        UnityNpgsqlTypes.NpgsqlTimeStamp stamp;
        DateTime date;
        DateTimeOffset dateOffset;
        using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT date " +
             "FROM trades " +
             "LIMIT 1;", dbcon))
        {
            using (var reader = cmd.ExecuteReader())
            {
                reader.Read();
                date = reader.GetTimeStamp(0);
            }
        }
        return date;
    }

    public PriceFluctuation GetFluctuation(DateTime timestamp)
    { 
        DateTime periodEnd;
        List<double> prices = new List<double>();
        double volume =0;
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


    public IEnumerable<PriceFluctuation> GetPriceFluctuationsByTimeFrame(DateTime fromDate, DateTime toDate)
    {
        if (fromDate == toDate)
        {
            //Debug.LogError("Дата не может быть NULL");
            //return null;
        }

        List<PriceFluctuation> pricesFluct = new List<PriceFluctuation>();
        List<Trade> trades = new List<Trade>();
        DateTime endDate = toDate.UpToNextFrame(TFrame);
        DateTime beginDate = fromDate.FloorToTimeFrame(TFrame);
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

    public ExtraData GetDataByPoint(DateTime dateTime)
    {
        throw new NotImplementedException();
    }
}

