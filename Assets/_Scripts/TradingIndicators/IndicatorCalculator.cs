using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;

namespace Chart
{
    using Entity;
    public class IndicatorCalculator
    {


        public IEnumerable<double?> SimpleMovingAverage(IEnumerable<PriceFluctuation> fluctuations,TimeFrame TFrame, int length)
        {
            if (length <= 0)
            {
                Debug.LogError("length должен быть >0");
                return null;
            }
            

            double?[] ma_values =new double?[fluctuations.Count()];

            PriceFluctuation pf =fluctuations.OrderBy(t=> t.PeriodBegin).ElementAt(length - 1);

            int j = 0;
            int i;
            foreach (var fluct in fluctuations.Where(f => f.PeriodBegin >= pf.PeriodBegin))
            {
                double ma = 0;
                i = 0;

                foreach (var prev_fluct in fluctuations.Where(f => f.PeriodBegin <= fluct.PeriodBegin && f.PeriodBegin + (length - 1) * TFrame >= fluct.PeriodBegin))
                {

                    ma += prev_fluct.Close;
                    i++;
                }


                if (i == length)
                {
                    ma /= length;
                    ma_values[j] = ma;
                }
                else
                {
                    ma_values[j] = null;
                }
                j++;
            }
            return ma_values;

        }
    }
}