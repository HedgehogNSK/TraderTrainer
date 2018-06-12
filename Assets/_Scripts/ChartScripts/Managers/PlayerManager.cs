using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

            public Text txtBalance;
            public Text txtPosition;
            public Text txtTotal;
            public Text txtProfit;
            public Text txtPrice;
            
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
            List<Order> playerOrders = new List<Order>();

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
                tmpPlayerCap = initialCap = PlayerCurrentBalance = (decimal)PlayerPrefs.GetFloat("Deposit", 10000);
                PositionSize = 0;
                tradeDirection = Position.None;
                playerOrders = new List<Order>();


            }

            public decimal Total(decimal price)
            {
                if (chartData != null)
                {
                    PriceFluctuation fluct = chartData.GetPriceFluctuation(chartData.DataEndTime);
                    if (PositionSize >= 0)
                        return
                            (PlayerCurrentBalance + PositionSize * price);
                    else
                        return PlayerCurrentBalance + PositionSize*( price - 2*openPositionPrice);
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

            List<decimal> lastTradesProfit = new List<decimal>();


            decimal playerCurrentBalance;
            public decimal PlayerCurrentBalance
            {
                get { return playerCurrentBalance; }
                set
                {
                    playerCurrentBalance = value;
                    txtBalance.text = playerCurrentBalance.ToString("F4");
                   
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
                    txtPosition.text = positionSize.ToString("F4");                  
                }
            }

            decimal positionOpenCost = 0;

            
            public void CreateOrder(Order.Type type, decimal amount, decimal price =0)
            {
                Order newOrder = new Order(type, amount,  price);
                playerOrders.Add(newOrder);
            }

            //Проверка количества
            public bool IsAmountCorrect(decimal amount, decimal price)
            {
                                             
                return (amount!= decimal.MaxValue && amount!= decimal.MinValue && Math.Abs(amount + PositionSize) <= Total(price) / price);
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
            decimal openPositionPrice;
            
            public void UpdateSomeData(decimal price)
            {
                txtTotal.text = Total(price).ToString("F2");
                txtProfit.text = TotalProfit(price).ToString("F2");
                txtPrice.text = openPositionPrice.ToString("F2");
            }
            //Сейчас функция не учитывает объём 
            public void UpdatePosition()
            {
                PriceFluctuation fluct = chartData.GetPriceFluctuation(chartData.DataEndTime);
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
                                price = (decimal) fluct.Open;
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
                            openPositionPrice = price;
                        }
                        else if (PositionSize < 0)
                        {
                            if (order.Amount > PositionSize)
                            {
                                PlayerCurrentBalance += PositionSize * (price - 2 * openPositionPrice) - (order.Amount + PositionSize) * price;
                                openPositionPrice = price;
                            }
                            else
                            {
                                //PlayerCurrentBalance +=  order.Amount* price;
                                openPositionPrice = (PositionSize * openPositionPrice + order.Amount * price) / (PositionSize + order.Amount);

                            }
                        }
                    }
                    else if (order.Amount < 0)
                    {
                        if (PositionSize <= 0)
                        {
                            PlayerCurrentBalance += order.Amount * price;
                            openPositionPrice = (PositionSize * openPositionPrice + order.Amount * price) / (PositionSize + order.Amount);

                        }
                        else if (PositionSize > 0)
                        {
                            PlayerCurrentBalance += -order.Amount > PositionSize ?
                                PositionSize * price + (order.Amount + PositionSize) * price :
                                -order.Amount * price;
                            openPositionPrice = price;

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

            public void Buy()
            {
                switch(GameManager.Instance.gameMode)
                {
                    case GameManager.Mode.Simple:
                        {
                            
                            CreateOrder(Order.Type.Market,decimal.MaxValue);
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

                            CreateOrder(Order.Type.Market, decimal.MinValue);
                        }
                        break;
                    default: { Debug.Log("Для этого мода игры не реализован алгоритм"); } break;
                }
            }

            public void StayInPosition()
            {

            }

            private IChartDataManager chartData;

            public void Start()
            {
                StartGame();
            }
            public void StartGame()
            {
                if (GameManager.Instance)
                {
                    Initialize();
                    GameManager.Mode mode = GameManager.Mode.Simple;
                    chartData = GameManager.Instance.GenerateGame(mode);
                    UpdateSomeData((decimal)chartData.GetPriceFluctuation(chartData.DataEndTime).Close);

                    GameManager.Instance.GoToNextFluctuation += UpdatePosition;
                    GameManager.Instance.GoToNextFluctuation += ()=>{
                        UpdateSomeData((decimal)chartData.GetPriceFluctuation(chartData.DataEndTime).Close);
                    };
                }
                else
                {
                    Debug.Log("GameManager не может сформировать игру");
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
}