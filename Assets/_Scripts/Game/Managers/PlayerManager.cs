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
                if (PositionSize != value)
                {                    
                    positionSize = value;
                    if (PositionSizeIsChanged != null) PositionSizeIsChanged(value);
                }
            }
        }

        public decimal OpenPositionPrice { get; private set; }
        public decimal LastPrice {
            get
            {
                return (decimal)LastFluctuation.Close;
            }
        }

        public float WinRate
        {
            get
            {
                if (PapperProfit(LastPrice) != 0)
                    return ((float)posTradesCount + (PapperProfit(LastPrice)>0? 1:0)) / (posTradesCount + negTradesCount + 1) * 100;


                return
                  ((float)posTradesCount) / (posTradesCount + negTradesCount)*100;
            }
        }
        
        enum Fluct
        {
            Open,
            High,
            Low,
            Close           
        }
        public PriceFluctuation LastFluctuation
        {
            get
            {
                if (chartData != null)
                    return chartData.GetPriceFluctuation(chartData.WorkEndTime);
                else
                    return new PriceFluctuation(new DateTime(), 0, 0);
            }
        }
        public decimal PapperProfit(decimal price)
        {
            {
                if (PositionSize == 0)
                {                    
                    return 0;
                }

                return (price - OpenPositionPrice) * PositionSize;
            }
        }
        public decimal TotalProfit
        {
            get
            {
                decimal total_profit=0;
                decimal pos_size = 0;
                var filledOrders = playerOrders.Where(p_order => p_order.state == Order.State.Filled);
                foreach (var order in filledOrders)
                {
                    pos_size += order.Amount;
                    total_profit -= order.Amount * order.FillPrice;

                }                
                total_profit += pos_size * LastPrice;
                return total_profit;
            }
        }

        decimal bestTrade = decimal.MinValue;
        public decimal BestTrade
        {
            get
            {
                return PapperProfit(LastPrice) > bestTrade ? PapperProfit(LastPrice) : bestTrade;
            }
            private set { bestTrade = value; }
        }
        decimal worstTrade = decimal.MaxValue;
        public decimal WorstTrade
        {
            get
            {
                return PapperProfit(LastPrice) < worstTrade ? PapperProfit(LastPrice) : worstTrade;
            }
            private set { worstTrade = value; }
        }

        private int posTradesCount, negTradesCount;

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

        public void CreateOrder(Order.Type type, decimal amount, decimal price = 0)
        {
            Order newOrder = new Order(type, amount, price);
            playerOrders.Add(newOrder);
        }

        public void CloseByMarket()
        {
           CreateOrder(Order.Type.Market, -PositionSize);
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
                                if (order.Price >= (decimal)LastFluctuation.Open)
                                {
                                    price = (decimal)LastFluctuation.Open;

                                }
                                else if (order.Price >= (decimal)LastFluctuation.Low)
                                {
                                    price = order.Price;
                                }
                            }
                            else
                            {
                                if (order.Price <= (decimal)LastFluctuation.Open)
                                {
                                    price = (decimal)LastFluctuation.Open;

                                }
                                else if (order.Price <= (decimal)LastFluctuation.High)
                                {
                                    price = order.Price;
                                }

                            }
                            if (price == 0) return;

                        }
                        break;
                    case Order.Type.Market:
                        {
                            price = (decimal)LastFluctuation.Open;
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
                if (PositionSize!=0 && Math.Abs(order.Amount) >= Math.Abs(PositionSize) && Math.Sign(order.Amount) != Math.Sign(PositionSize))
                {
                    decimal trade_profit = PapperProfit(price);
                    if (BestTrade < trade_profit) BestTrade = trade_profit;
                    if (WorstTrade > trade_profit) WorstTrade = trade_profit;

                    if (trade_profit > 0)
                        posTradesCount++;
                    else
                        negTradesCount++;
                    Debug.Log(posTradesCount + " :" + negTradesCount);
                }
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
                order.FillPrice = price;
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
