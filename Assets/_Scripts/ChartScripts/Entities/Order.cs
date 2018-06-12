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
            decimal amount;
            public decimal Amount
            {
                get { return amount; }
                set { amount = value; }
            }

            decimal price;
            public decimal Price
            {
                get { return price; }
                set { price = value; }
            }

            DateTime lastChange;
            public DateTime LastChange { get { return lastChange; } }

            public Order(Type type, decimal amount, decimal price = 0)
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

            public bool IsOrderSettingsCorrect(decimal amount, decimal price)
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
