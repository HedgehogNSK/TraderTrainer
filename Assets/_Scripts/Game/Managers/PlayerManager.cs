using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Chart;
using Chart.Entity;
using ChartGame.Entity;

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
                    total_profit -= order.Amount * order.ExecutionPrice;

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
            if(newOrder!=null)playerOrders.Add(newOrder);
        }

        public void CloseByMarket()
        {
           CreateOrder(Order.Type.Market, -PositionSize);
        }

 
        public void InitializeData(IScalableDataManager chartData)
        {
            this.chartData = chartData;
            tmpPlayerCap = initialCap = PlayerCurrentBalance = (decimal)PlayerPrefs.GetFloat("Deposit", 10000);
            PositionSize = 0;
            bestTrade = decimal.MinValue;
            worstTrade = decimal.MaxValue;
            posTradesCount =negTradesCount = 0;
            tradeIsClosed = null;
            tradeIsOpened = null;
            playerOrders = new List<Order>();
        }

        //Данный метод не учитывает объём 
        public void UpdatePosition()
        {
            var orders = playerOrders.Where(order => order.state == Order.State.Waiting);

            foreach (Order order in orders)
            {
                          
                if(order.TryToExecute(LastFluctuation))
                {
                    CollectTradeStats(order);

                    if (order.Amount > 0)
                    {
                        if (PositionSize >= 0)
                        {
                            PlayerCurrentBalance -= order.Amount * order.ExecutionPrice;
                            OpenPositionPrice = order.ExecutionPrice;
                        }
                        else if (PositionSize < 0)
                        {
                            if (order.Amount > PositionSize)
                            {
                                PlayerCurrentBalance += PositionSize * (order.ExecutionPrice - 2 * OpenPositionPrice) - (order.Amount + PositionSize) * order.ExecutionPrice;
                                OpenPositionPrice = order.ExecutionPrice;
                            }
                            else
                            {
                                //PlayerCurrentBalance +=  order.Amount* price;
                                OpenPositionPrice = (PositionSize * OpenPositionPrice + order.Amount * order.ExecutionPrice) / (PositionSize + order.Amount);

                            }
                        }
                    }
                    else
                    {
                        if (PositionSize <= 0)
                        {
                            PlayerCurrentBalance += order.Amount * order.ExecutionPrice;
                            OpenPositionPrice = (PositionSize * OpenPositionPrice + order.Amount * order.ExecutionPrice) / (PositionSize + order.Amount);

                        }
                        else if (PositionSize > 0)
                        {
                            PlayerCurrentBalance += -order.Amount > PositionSize ?
                                PositionSize * order.ExecutionPrice + (order.Amount + PositionSize) * order.ExecutionPrice :
                                -order.Amount * order.ExecutionPrice;
                            OpenPositionPrice = order.ExecutionPrice;

                        }

                    }

                    if (PositionSize == 0 || (Math.Abs(order.Amount) >= Math.Abs(PositionSize) && Math.Sign(order.Amount) != Math.Sign(PositionSize)))
                    {
                        tradeIsOpened(OpenPositionPrice, order.Amount);
                    }
                    PositionSize += order.Amount;

                }
            }

        }

        private void CollectTradeStats(Order order)
        {
            //IF trade is going to close
            if (PositionSize != 0 && Math.Abs(order.Amount) >= Math.Abs(PositionSize) && Math.Sign(order.Amount) != Math.Sign(PositionSize))
            {
                decimal trade_profit = PapperProfit(order.ExecutionPrice);
                if (BestTrade < trade_profit) BestTrade = trade_profit;
                if (WorstTrade > trade_profit) WorstTrade = trade_profit;

                if (trade_profit > 0)
                    posTradesCount++;
                else
                    negTradesCount++;

                if (tradeIsClosed != null) tradeIsClosed(new TradeInfo(OpenPositionPrice, order.ExecutionPrice, PositionSize));
                //Debug.Log(posTradesCount + " :" + negTradesCount);
            }
        }

        public event Action<TradeInfo> tradeIsClosed;
        public event Action<decimal, decimal> tradeIsOpened;

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
