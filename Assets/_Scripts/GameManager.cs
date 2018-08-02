﻿using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hedge.Tools;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Hedge.Tools.UnityUI;

namespace Chart
{
    using Entity;
    using Controllers;

    namespace Managers
    {
        public class GameManager : MonoBehaviour
        {
            #region INSTANCE
            static GameManager _instance;
            public static GameManager Instance
            {
                get
                {
                    if (!_instance)
                    {
                        _instance = FindObjectOfType<GameManager>();
                        if (!_instance)
                        {
                            Debug.LogWarning("Объект GameManager отсутствует на сцене");
                        }
                        return _instance;
                    }
                    return _instance;
                }
                
            }
            #endregion

            
            const int MIN_HISTORICAL_FLUCTUATIONS_AMOUNT = 25;
            const int MAX_HISTORICAL_FLUCTUATIONS_AMOUNT = 50;
            const int MIN_FLUCTUATIONS_AMOUNT_TOPLAY = 50;
            const int MIN_FLUCTUATIONS_AMOUNT = MIN_HISTORICAL_FLUCTUATIONS_AMOUNT + MIN_FLUCTUATIONS_AMOUNT_TOPLAY;
            AssetId[] assets = new AssetId[]
            {
               new AssetId ("BTC","USD","Bitfinex" ),
               new AssetId ("ETH","USD","Bitfinex" ),
               new AssetId ("ETC","USD","Bitfinex" ),
               new AssetId ("LTC","USD","Bitfinex" ),
               new AssetId ("DASH","USD","Bitfinex"),
               new AssetId ("EOS","USD","Bitfinex" ),
               new AssetId ("XRP","USD","Bitfinex" ),
               new AssetId ("XLM","USD","Bitfinex" ),
               new AssetId ("XMR","USD","Bitfinex" ),
               new AssetId ("OMG","USD","Bitfinex" ),
               new AssetId ("ZEC","USD","Bitfinex" ),
               new AssetId ("NEO","USD","Bitfinex" ),
               new AssetId ("BTG","USD","Bitfinex" ),
               new AssetId ("SAN","USD","Bitfinex" ),
               new AssetId ("QTUM","USD","Bitfinex" ),
               new AssetId ("TRX","USD","Bitfinex" ),
               new AssetId ("BCH","USD","Bitfinex" ),
               new AssetId ("REP","USD","Bitfinex" ),
               new AssetId ("XVG","USD","Bitfinex" ),
               new AssetId ("ETH","BTC","Bitfinex" ),
               new AssetId ("LTC","BTC","Bitfinex" ),
               new AssetId ("BCH","BTC","Bitfinex" ),
               new AssetId ("EOS","BTC","Bitfinex" ),
               new AssetId ("XRP","BTC","Bitfinex" ),
               new AssetId ("ZEC","BTC","Bitfinex" ),
               new AssetId ("SAN","BTC","Bitfinex" ),
               new AssetId ("OMG","BTC","Bitfinex" ),
               new AssetId ("NEO","BTC","Bitfinex" ),
               new AssetId ("GNT","BTC","Bitfinex" ),
               new AssetId ("BCH","ETH","Bitfinex" ),
               new AssetId ("NEO","ETH","Bitfinex" ),
               new AssetId ("QTM","ETH","Bitfinex" ),
               new AssetId ("EOS","ETH","Bitfinex" ),
               new AssetId ("ZRX","ETH","Bitfinex" ),

            };
            public enum Mode
            {
                Simple,
                Advanced,
                Real
            }
            public Mode gameMode;
            
            [SerializeField] Candle candleDummy;
            [SerializeField] Transform candlesParent;
            [SerializeField] ChartDrawer chartDrawer;
            [SerializeField] Button SellButton;
            [SerializeField] Button BuyButton;
            [SerializeField] AdvancedButton ExtraButton;
            [SerializeField] Button ExitButton;
            [SerializeField] Color colorLongExit, colorShortExit;

            public Text txtBalance;
            public Text txtPosition;
            public Text txtTotal;
            public Text txtProfit;
            public Text txtPrice;

            Image exitButtonImage;
            SimpleChartDataManager db;
            SQLChartDataManager sqlDB;
            IChartDataManager chartDataManager;
            IDateWorkFlow dateWorkFlow;
            IGrid grid;
            public event Action GoToNextFluctuation;
            public int fluctuation1ID = 200;
            public int fluctuationsCountToLoad = 100;

            EventTrigger trigger;
            // Use this for initialization
            private void Awake()
            {
                if (!candleDummy || !candlesParent)
                    Debug.LogError("[GameObject]"+name + ": Задай все параметры");
                if(!SellButton || !BuyButton || !ExtraButton)
                {
                    Debug.LogError("[GameObject]"+name +": Не задана одна из кнопок контроля");
                    return;
                }
               

            }
            
            void Start()
            {
                LoadGame();

                SellButton.onClick.AddListener(Sell);     
                BuyButton.onClick.AddListener(Buy);
                ExitButton.onClick.AddListener(Exit);
                exitButtonImage = ExitButton.GetComponent<Image>();

                ExtraButton.onPressHold += TryLoadNextFluctuation;

                ExitButton.gameObject.SetActive(false);
                PlayerManager.Instance.PositionSizeIsChanged += ActivateButtons;
                PlayerManager.Instance.CurrentBalanceChanged += (x) => { txtBalance.text = x.ToString("F4"); };
                
            }

            public void ActivateButtons(decimal positionSize)
            {
                ExitButton.gameObject.SetActive(positionSize != 0);
                BuyButton.gameObject.SetActive(positionSize <= 0);
                SellButton.gameObject.SetActive(positionSize >= 0);
                ExitButton.transform.position = positionSize > 0 ? BuyButton.transform.position : SellButton.transform.position;
                exitButtonImage.color = positionSize > 0 ? colorLongExit : colorShortExit;
                txtPosition.text = positionSize.ToString("F4");
            }

            public void UpdatePlayersInfoFields(decimal price)
            {
                txtTotal.text = PlayerManager.Instance.Total(price).ToString("F2");
                txtProfit.text = PlayerManager.Instance.TotalProfit(price).ToString("F2");
                txtPrice.text = PlayerManager.Instance.OpenPositionPrice.ToString("F2");
            }

            internal void LoadGame(Mode mode = Mode.Simple)
            {
                gameMode = mode;
                switch(gameMode)
                {
                    case Mode.Simple:
                        {
                            chartDataManager = CreateRandomDataManager();
                            int fluctuationCount = DateTimeTools.CountFramesInPeriod(chartDataManager.TFrame, chartDataManager.DataBeginTime, chartDataManager.DataEndTime, TimeSpan.Zero);
                            SetRandomGameTime(fluctuationCount);

                            dateWorkFlow = chartDataManager as IDateWorkFlow;
                            dateWorkFlow.SetWorkDataRange(fluctuation1ID, fluctuationsCountToLoad);

                            grid = new CoordinateGrid(chartDataManager.DataBeginTime, chartDataManager.TFrame);
                            chartDrawer.ChartDataManager = chartDataManager;
                            chartDrawer.CoordGrid = grid;
                            NavigationController.Instance.ChartDataManager = chartDataManager;
                            NavigationController.Instance.CoordGrid = grid;
                            GoToNextFluctuation += NavigationController.Instance.GoToLastPoint;

                            PlayerManager.Instance.InitializeData(chartDataManager);

                            GoToNextFluctuation += PlayerManager.Instance.UpdatePosition;

                            UpdatePlayersInfoFields((decimal)chartDataManager.GetPriceFluctuation(chartDataManager.DataEndTime).Close);
                            GoToNextFluctuation += () => {
                                UpdatePlayersInfoFields((decimal)chartDataManager.GetPriceFluctuation(chartDataManager.DataEndTime).Close);
                            };
                        }
                        break;
                    default: {
                            Debug.LogError("Не создан сценарий для данного мода");
                        } break;
                }
                
                
            }
            void SetRandomGameTime(int fluctuationCount)
            {
                if (fluctuationCount < MIN_FLUCTUATIONS_AMOUNT)
                {
                    throw new ArgumentOutOfRangeException("Количество свечей должно быть больше " + MIN_FLUCTUATIONS_AMOUNT + " Выбирите другой инструмент и попытайтесь снова");
                }
                
                int fluctuations2play = UnityEngine.Random.Range(MIN_FLUCTUATIONS_AMOUNT_TOPLAY, fluctuationCount -  MIN_HISTORICAL_FLUCTUATIONS_AMOUNT);
                int fluctuations4preload = fluctuationCount - fluctuations2play;  
                fluctuationsCountToLoad = UnityEngine.Random.Range(MIN_HISTORICAL_FLUCTUATIONS_AMOUNT, fluctuations4preload < MAX_HISTORICAL_FLUCTUATIONS_AMOUNT? fluctuations4preload: MAX_HISTORICAL_FLUCTUATIONS_AMOUNT);
                fluctuation1ID = UnityEngine.Random.Range(0, fluctuations4preload- fluctuationsCountToLoad);
            }
            IChartDataManager CreateRandomDataManager()
            {
                //В зависимости от типа мэнеджера должен выбираться нужный тайм-фрейм
                Period[] values = { Period.Minute, Period.Hour, Period.Day };
                Period randomValue = values[UnityEngine.Random.Range(0, values.Length)];

                int[] availablePeriodSizes;
                switch(randomValue)
                {
                    case Period.Minute:
                        {
                            availablePeriodSizes = new int[] { 1, 5 }; // { 1, 5 ,10,15,20,30};
                        }
                            break;
                    case Period.Hour:
                        {
                            availablePeriodSizes = new int[] { 1, 3, 4 };//{ 1, 3, 4, 6, 12 };
                        }
                        break;
                    case Period.Day:
                        {
                            availablePeriodSizes = new int[] { 1,3 };
                        } break;
                    default: { throw new ArgumentOutOfRangeException("Enum присвоено не верное значение"); }
                }
                
                int periodSize = availablePeriodSizes[UnityEngine.Random.Range(0, availablePeriodSizes.Length)];
                int randAssetId = UnityEngine.Random.Range(0, assets.Length);
                /*/Для теста рабочей области
                randomValue = Period.Day;
                periodSize = 3;
                //*/
                TimeFrame timeFrame = new TimeFrame(randomValue, periodSize);
                
               
                //TODO: Здесь должен быть случайный выбор из любых доступных менеджеров
                IChartDataManager dm = new CryptoCompareDataManager(timeFrame, assets[randAssetId].base_currency, assets[randAssetId].reciprocal_currency, assets[randAssetId].exchange);
                
                return dm;
            }

            // Update is called once per frame
            void Update()
            {
                if(Input.GetKey(KeyCode.UpArrow))
                {
                    grid.Scale *= 1.1f;
                    
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    grid.Scale /= 1.1f;                  
                }

                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    TryLoadNextFluctuation();
                }
            }

            public bool TryLoadNextFluctuation(PointerEventData eventData = null)
            {
                if (dateWorkFlow == null)
                {
                    Debug.LogError("Невозможно загрузить следующее колебание. Отсутствуют рабочее временнОе пространство");
                    return false;
                }

                if (!dateWorkFlow.AddTimeStep())
                {
                    Debug.Log(" Невозможно загрузить следующее колебание. Достигнут край рабочей области");
                    return false;
                }

                GoToNextFluctuation();

                return true;
            }

            public void Buy()
            {
                switch (gameMode)
                {
                    case Mode.Simple:
                        {

                            PlayerManager.Instance.CreateOrder(Order.Type.Market, decimal.MaxValue);
                        }
                        break;
                    default: { Debug.Log("Для этого мода игры не реализован алгоритм"); } break;
                }
                TryLoadNextFluctuation();
            }

            public void Sell()
            {
                switch (gameMode)
                {
                    case Mode.Simple:
                        {

                            PlayerManager.Instance.CreateOrder(Order.Type.Market, decimal.MinValue);
                        }
                        break;
                    default: { Debug.Log("Для этого мода игры не реализован алгоритм"); } break;
                }
                TryLoadNextFluctuation();

            }

            public void Exit()
            {
                PlayerManager.Instance.CreateOrder(Order.Type.Market, -PlayerManager.Instance.PositionSize);
                TryLoadNextFluctuation();
            }
        }

        struct AssetId
        {
            public AssetId(string base_currency, string reciprocal_currency,string exchange)
            {
                this.base_currency = base_currency;
                this.reciprocal_currency = reciprocal_currency;
                this.exchange = exchange;
            }
            public string base_currency;
            public string reciprocal_currency;
            public string exchange;
        }

    }
}
