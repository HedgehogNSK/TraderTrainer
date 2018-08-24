using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Chart;
using Chart.Entity;

namespace ChartGame
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

        List<Order> playerOrders = new List<Order>();
        private IScalableDataManager chartData;

        List<decimal> lastTradesProfit = new List<decimal>();


        public event Action<decimal> CurrentBalanceChanged;
        public event Action<decimal> PositionSizeIsChanged;
        decimal playerCurrentBalance;
        public decimal PlayerCurrentBalance
        {
            get { return playerCurrentBalance; }
            set
            {
                playerCurrentBalance = value;
                if (CurrentBalanceChanged != null) CurrentBalanceChanged(value);

            }
        }

        decimal initialCap;
        decimal tmpPlayerCap = 0;


        decimal positionSize;
        public decimal PositionSize
        {
            get { return positionSize; }
            set
            {
                positionSize = value;
                if (PositionSizeIsChanged != null) PositionSizeIsChanged(value);
            }
        }

        public decimal OpenPositionPrice { get; private set; }


        public decimal Total(decimal price)
        {
            if (chartData != null)
            {
                if (PositionSize >= 0)
                    return
                        (PlayerCurrentBalance + PositionSize * price);
                else
                    return PlayerCurrentBalance + PositionSize * (price - 2 * OpenPositionPrice);
            }
            else return 0;

        }

        public decimal TotalProfit(decimal price)
        {

            if (chartData != null)
            {
                return Total(price) - initialCap;
            }
            else return 0;
        }

        public void CreateOrder(Order.Type type, decimal amount, decimal price = 0)
        {
            Order newOrder = new Order(type, amount, price);
            playerOrders.Add(newOrder);
        }

        public void CloseByMarket()
        {
            PlayerManager.Instance.CreateOrder(Order.Type.Market, -PositionSize);
        }

        //Проверка количества
        public bool IsAmountCorrect(decimal amount, decimal price)
        {

            return (amount != decimal.MaxValue && amount != decimal.MinValue && Math.Abs(amount + PositionSize) <= Total(price) / price);
        }

        //Фильтр размера ордера   
        public decimal AmountFilter(decimal amount, decimal price)
        {
            if (!IsAmountCorrect(amount, price))
            {

                if (Math.Sign(amount) != Math.Sign(PositionSize))
                {
                    amount = Math.Sign(amount) * (Total(price) / price) - PositionSize;
                }
                else
                {
                    amount = Math.Sign(amount) * (PlayerCurrentBalance / price);
                }
            }
            return amount;
        }

        public void InitializeData(IScalableDataManager chartData)
        {
            this.chartData = chartData;
            tmpPlayerCap = initialCap = PlayerCurrentBalance = (decimal)PlayerPrefs.GetFloat("Deposit", 10000);
            PositionSize = 0;
            playerOrders = new List<Order>();
        }

        //Данный метод не учитывает объём 
        public void UpdatePosition()
        {
            PriceFluctuation fluct = chartData.GetPriceFluctuation(chartData.WorkEndTime);
            var orders = playerOrders.Where(order => order.state == Order.State.Waiting);
            decimal price;

            foreach (Order order in orders)
            {
                switch (order.type)
                {
                    case Order.Type.Limit:
                        {
                            price = 0;
                            if (order.Amount > 0)
                            {
                                if (order.Price >= (decimal)fluct.Open)
                                {
                                    price = (decimal)fluct.Open;

                                }
                                else if (order.Price >= (decimal)fluct.Low)
                                {
                                    price = order.Price;
                                }
                            }
                            else
                            {
                                if (order.Price <= (decimal)fluct.Open)
                                {
                                    price = (decimal)fluct.Open;

                                }
                                else if (order.Price <= (decimal)fluct.High)
                                {
                                    price = order.Price;
                                }

                            }
                            if (price == 0) return;

                        }
                        break;
                    case Order.Type.Market:
                        {
                            price = (decimal)fluct.Open;
                            order.Amount = AmountFilter(order.Amount, price);
                        }
                        break;

                    default: { throw new ArgumentOutOfRangeException("Действие для этого типа ордера не описано"); }
                }

                RecalculatePosition(order, price);


            }

        }

        //Расчёт позиции и баланса
        private void RecalculatePosition(Order order, decimal price)
        {
            if (IsAmountCorrect(order.Amount, price))
            {
                if (order.Amount > 0)
                {
                    if (PositionSize >= 0)
                    {
                        PlayerCurrentBalance -= order.Amount * price;
                        OpenPositionPrice = price;
                    }
                    else if (PositionSize < 0)
                    {
                        if (order.Amount > PositionSize)
                        {
                            PlayerCurrentBalance += PositionSize * (price - 2 * OpenPositionPrice) - (order.Amount + PositionSize) * price;
                            OpenPositionPrice = price;
                        }
                        else
                        {
                            //PlayerCurrentBalance +=  order.Amount* price;
                            OpenPositionPrice = (PositionSize * OpenPositionPrice + order.Amount * price) / (PositionSize + order.Amount);

                        }
                    }
                }
                else if (order.Amount < 0)
                {
                    if (PositionSize <= 0)
                    {
                        PlayerCurrentBalance += order.Amount * price;
                        OpenPositionPrice = (PositionSize * OpenPositionPrice + order.Amount * price) / (PositionSize + order.Amount);

                    }
                    else if (PositionSize > 0)
                    {
                        PlayerCurrentBalance += -order.Amount > PositionSize ?
                            PositionSize * price + (order.Amount + PositionSize) * price :
                            -order.Amount * price;
                        OpenPositionPrice = price;

                    }

                }
                PositionSize += order.Amount;

                if (Math.Sign(PositionSize) != Math.Sign(PositionSize - order.Amount))
                {
                    tmpPlayerCap = PlayerCurrentBalance + PositionSize * price;
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

        private void OnDestroy()
        {
            if (GameManager.Instance)
            {
                GameManager.Instance.GoToNextFluctuation -= UpdatePosition;
                //GameManager.Instance.GoToNextFluctuation -= UpdateSomeData;
            }
        }
    }
}
