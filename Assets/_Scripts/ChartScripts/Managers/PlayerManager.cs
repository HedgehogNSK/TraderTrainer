using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
namespace Chart
{
    using Entity;
    //using System.Runtime.Serialization;
    namespace Managers
    {
        [Serializable]
        public class PlayerManager : MonoBehaviour
        {
 

            static PlayerManager _instance;
            public static PlayerManager Instance
            {
                get
                {
                    if (!_instance)
                    {
                        _instance = FindObjectOfType<PlayerManager>();
                    }
                    return _instance;

                }
            }

            
            [Serializable]
            public enum Position
            {
               // [EnumMember]
                None,
                //[EnumMember]
                Long,
                //[EnumMember]
                Short
            }
            Position tradeDirection;
            List<Order> playerOrders;

            private float cash;
            public float Cash
            {
                get
                {
                    return cash;
                }
                private set { cash = value; }
            }

            private void Initialize()
            {
                tmpPlayerCap =  initialCap = playerCurrentBalance = PlayerPrefs.GetFloat("Deposit", 10000);
                tradeDirection = Position.None;
                playerOrders = new List<Order>();


            }

            public double Total
            {
                get
                {
                    if (chartData != null)
                    {
                        PriceFluctuation fluct = chartData.GetPriceFluctuation(chartData.DataEndTime);
                        if (positionSize >= 0)
                            return
                                (playerCurrentBalance + positionSize * fluct.Close);
                        else
                            return playerCurrentBalance - positionSize * (2 * cachedPrice - fluct.Close);
                    }
                    else return 0;
                }
            }

            public double TotalProfit
            {
                get
                {
                    if (chartData!=null)
                    {
                            return Total - initialCap;
                    }
                    else return 0;
                }
            }

            List<double> lastTradesProfit = new List<double>();
           

            double playerCurrentBalance;
            double initialCap;
            double tmpPlayerCap = 0;
            double positionSize = 0;
            public double PlayerPosition {
                get { return positionSize; }      
            }

            double positionOpenCost = 0;

            
            public void CreateOrder(Order.Type type,double amount, double price=0)
            {
                Order newOrder = new Order(type, amount,  price);
                playerOrders.Add(newOrder);
            }

            //Проверка количества
            public bool IsAmountCorrect(double amount, double price)
            {
                                             
                return (Math.Abs(amount + positionSize) > Total / price);
            }

            //Фильтр размера ордера   
            public double AmountFilter(double amount, double price)
            {                              
                if (IsAmountCorrect(amount, price))
                {
                    amount = Math.Sign(amount) * (Total / price - positionSize);
                }
                return amount;
            }
            double cachedPrice;
            //Сейчас функция не учитывает объём 
            public void CheckPosition()
            {
                PriceFluctuation fluct = chartData.GetPriceFluctuation(chartData.DataEndTime);
                var orders = playerOrders.Where(order => order.state == Order.State.Waiting);
                double price;

                foreach (Order order in orders)
                {
                    switch (order.type)
                    {
                        case Order.Type.Limit:
                            {
                                price = 0;
                                if (order.Amount > 0)
                                {
                                    if (order.Price >= fluct.Open)
                                    {
                                        price = fluct.Open;

                                    }
                                    else if (order.Price >= fluct.Low)
                                    {
                                        price = order.Price;
                                    }
                                }
                                else
                                {
                                    if (order.Price <= fluct.Open)
                                    {
                                        price = fluct.Open;

                                    }
                                    else if (order.Price <= fluct.High)
                                    {
                                        price = order.Price;
                                    }

                                }
                                
                            }
                            break;
                        case Order.Type.Market:
                            {
                                price = fluct.Open;                           
                            }
                            break;

                        default: { throw new ArgumentOutOfRangeException("Действие для этого типа ордера не описано"); }
                    }
                    if (price == 0) return;

                    if(order.type == Order.Type.Market)
                    order.Amount = AmountFilter(order.Amount, price);

                    if (IsAmountCorrect(order.Amount, price))
                    { //Расчёт позиции и баланса
                        if (order.Amount > 0)
                        {
                            if (positionSize >= 0)
                            {
                                playerCurrentBalance -= order.Amount * price;
                                positionSize += order.Amount;

                            }
                            else if (positionSize < 0)
                            {
                                cachedPrice = (positionSize * cachedPrice + order.Amount * price) / (positionSize + order.Amount);
                                positionSize += order.Amount;
                            }
                        }
                        else if (order.Amount < 0)
                        {
                            if (positionSize <= 0)
                            {
                                playerCurrentBalance += order.Amount * price;
                                cachedPrice = (positionSize * cachedPrice + order.Amount * price) / (positionSize + order.Amount);
                                positionSize -= order.Amount;

                            }
                            else if (positionSize > 0)
                            {
                                playerCurrentBalance -= order.Amount * price;
                                positionSize -= order.Amount;
                                cachedPrice = price;
                            }
                        }

                        if (Math.Sign(positionSize) != Math.Sign(positionSize - order.Amount))
                        {
                            tmpPlayerCap = playerCurrentBalance + positionSize * price;
                            lastTradesProfit.Add(tmpPlayerCap);
                        }
                        order.state = Order.State.Filled;
                    }
                    else
                    {
                        order.state = Order.State.Canceled;
                        Debug.Log("Недостаточно средств, для выполнения ордера");
                    }

                }

            }

            public void Buy()
            {
                switch(GameManager.Instance.gameMode)
                {
                    case GameManager.Mode.Simple:
                        {
                            
                            CreateOrder(Order.Type.Market,double.MaxValue);
                        } break;
                    default: { Debug.Log("Для этого мода игры не реализован алгоритм"); }break;
                }
            }

            public void Sell()
            {
                switch (GameManager.Instance.gameMode)
                {
                    case GameManager.Mode.Simple:
                        {

                            CreateOrder(Order.Type.Market, double.MinValue);
                        }
                        break;
                    default: { Debug.Log("Для этого мода игры не реализован алгоритм"); } break;
                }
            }

            public void StayInPosition()
            {

            }

            private IChartDataManager chartData;

            public void StartGame()
            {
                if (GameManager.Instance)
                {
                    Initialize();
                    GameManager.Mode mode = GameManager.Mode.Simple;
                    chartData = GameManager.Instance.GenerateGame(mode);
                    GameManager.Instance.GoToNextFluctuation += CheckPosition;
                }
                else
                {
                    Debug.Log("GameManager не может сформировать игру");
                }
            }


            private void OnDestroy()
            {
                GameManager.Instance.GoToNextFluctuation -= CheckPosition;
            }
        }
    }
}