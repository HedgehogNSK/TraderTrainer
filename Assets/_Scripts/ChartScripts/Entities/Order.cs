using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Chart
{
    namespace Entity
    {
        public class Order
        {
            public enum State
            {
                Waiting,
                Filled,
                PartiallyFilled,
                Canceled
           
            }
            public enum Type
            {
                Market,Limit
            }

            public State state { get; set; }
            public Type type { get; set; }
            double amount;
            public double Amount
            {
                get { return amount; }
                set { amount = value; }
            }

            double price;
            public double Price
            {
                get { return price; }
                set { price = value; }
            }

            DateTime lastChange;
            public DateTime LastChange { get { return lastChange; } }

            public Order(Type type, double amount,  double price = 0)
            {
                this.amount = amount;
                this.price = price;
                this.type = type;
                if (IsOrderSettingsCorrect(amount, price))
                {
                    state = State.Waiting;
                }
                else
                {
                    state = State.Canceled;
                }


            }

            public bool IsOrderSettingsCorrect(double amount, double price)
            {

                    if (amount == 0)
                    {
                        Debug.Log("Ордер не может быть создан для 0 количества актива");
                        return false;
                    }
                    return true;
                
            }
        }
    }
}
