using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Chart
{
    namespace Entity
    {
        public class Trade : IComparable
        {
            double price;
            public double Price { get { return price; } }
            double volume;
            public double Volume { get { return volume; } }

            DateTime? dateTime;
            public DateTime DTime { get{ return (DateTime)dateTime; } }

            public Trade(double price, DateTime dateTime, double volume)
            {
                this.price = price;
                this.dateTime = dateTime;
                this.volume = volume;
            }

            public int CompareTo(object obj)
            {
                if(obj is Trade)
                {
                        return (((DateTime)dateTime).CompareTo((obj as Trade).dateTime));
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }

    }
}
