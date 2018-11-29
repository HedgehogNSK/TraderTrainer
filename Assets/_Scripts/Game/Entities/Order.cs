using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Chart.Entity;
namespace ChartGame
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

            decimal exePrice;
            public decimal ExecutionPrice
            {
                get { return exePrice; }
                private set { exePrice = value; }
            }

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

            //Trying to execute. Returning true if success.
            internal bool TryToExecute(PriceFluctuation fluctuation)
            {
                        
                switch (type)
                {
                    case Type.Limit:
                        {
                            if (!IsAmountCorrect(PlayerManager.Instance, Price))
                            {
                                state = State.Canceled;
                                Debug.Log("Недостаточно средств, для выполнения ордера");
                                return false;
                            }
                             
                                if (Amount > 0)
                                {
                                    if (Price >= (decimal)fluctuation.Open)
                                    {
                                        ExecutionPrice = (decimal)fluctuation.Open;

                                    }
                                    else if (Price <= (decimal)fluctuation.Low)
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    if (Price <= (decimal)fluctuation.Open)
                                    {
                                        ExecutionPrice = (decimal)fluctuation.Open;

                                    }
                                    else if (Price >= (decimal)fluctuation.High)
                                    {
                                        return false;
                                    }

                                }
                            
                            

                        }
                        break;
                    case Type.Market:
                        {
                            ExecutionPrice = (decimal)fluctuation.Open;
                            Amount = AmountFilter(PlayerManager.Instance);
                        }
                        break;

                    default: { throw new ArgumentOutOfRangeException("Действие для этого типа ордера не описано"); }
                }
                state = State.Filled;
                return true;

            }

            //Проверка количества
            public bool IsAmountCorrect(PlayerManager player, decimal price )
            {

                return (Amount != decimal.MaxValue && Amount != decimal.MinValue && Math.Abs(Amount + player.PositionSize) <= player.Total(price) / price);
            }

            //Фильтр размера ордера   
            public decimal AmountFilter(PlayerManager player)
            {
                if (!IsAmountCorrect(player,ExecutionPrice))
                {

                    if (Math.Sign(Amount) != Math.Sign(player.PositionSize))
                    {
                        Amount = Math.Sign(Amount) * (player.Total(ExecutionPrice) / ExecutionPrice) - player.PositionSize;
                    }
                    else
                    {
                        Amount = Math.Sign(Amount) * (player.PlayerCurrentBalance / ExecutionPrice);
                    }
                }
                return Amount;
            }

        }
    }
}
