using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedge
{
    namespace Tools
    {
        static public class Ariphmetic
        {
            static public List<decimal> DividePriceRangeByKeyPoints(decimal first, decimal second, int divisorsMaxAmount, int significantDigits = 3)
            {
                List<decimal> keyPoints = new List<decimal>();
                decimal range = second - first;
                decimal step = range / (divisorsMaxAmount-1);

                step = getSignificationDigit(step, significantDigits,5);

                decimal point = (int)(first / step) * step;
                if (point != first)
                    point += step;
                
                while(point <= second)
                {
                    keyPoints.Add(point);
                    point += step;
                }            

                return keyPoints;
            }

            static public decimal getSignificationDigit(decimal number, int digits, int roundUpTo = 0)
            {

                decimal r;
                //Порядок числа
                int magnitudeOfNumber = (int)Math.Floor(Math.Log10((double)number));

                decimal dp = (decimal)Math.Pow(10, magnitudeOfNumber);
                decimal dd = (decimal)Math.Pow(10, digits - 1);
                r = number / dp*dd;

                if (roundUpTo != 0)
                {
                    number = (int)(r / roundUpTo) * roundUpTo;
                    if (number != r)
                        number += roundUpTo;
                }
                else
                {
                    number = Math.Round(r);
                }
                return number / dd * dp;
            }
        }
    }
}