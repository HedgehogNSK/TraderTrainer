using System.Collections;
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
                            Debug.LogWarning("Создай объект GameManager на сцене");
                        }
                        return _instance;
                    }
                    return _instance;
                }
                
            }
            #endregion

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
            SimpleChartViewer db;
            SQLChartViewer sqlDB;
            IChartDataManager chartDataManager;
            IDateWorkFlow dateWorkFlow;
            IGrid grid;
            public event Action GoToNextFluctuation;
            public int firstFluctuationID = 200;
            public int fluctuationsCountToLoad = 100;
            public float pressButtonDelay = 0.2f;

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

            BaseEventData baseEventData = new BaseEventData(EventSystem.current);
            
            void Start()
            {
                SellButton.onClick.AddListener(Sell);     
                BuyButton.onClick.AddListener(Buy);

                ExtraButton.onPressHold += TryLoadNextFluctuation;

                //trigger = ExtraButton.gameObject.AddComponent<EventTrigger>();
                //var pointerDown = new EventTrigger.Entry();
                //pointerDown.eventID = EventTriggerType.PointerDown;
                //pointerDown.callback.AddListener((e) => { StayInPosition(true, pressButtonDelay); });
                //trigger.triggers.Add(pointerDown);

                //var pointerUp = new EventTrigger.Entry();
                //pointerUp.eventID = EventTriggerType.PointerUp;
                //pointerUp.callback.AddListener((e) => { StayInPosition(false); });
                //trigger.triggers.Add(pointerUp);
            }

            internal IChartDataManager GenerateGame(Mode mode = Mode.Simple)
            {
                gameMode = mode;
                chartDataManager = new CryptoCompareDataManager(tframe: new TimeFrame(Period.Hour, 1));
                dateWorkFlow = chartDataManager as IDateWorkFlow;
                dateWorkFlow.SetWorkDataRange(firstFluctuationID, fluctuationsCountToLoad);

                grid = new CoordinateGrid(chartDataManager.DataBeginTime, chartDataManager.TFrame);
                chartDrawer.ChartDataManager = chartDataManager;
                chartDrawer.CoordGrid = grid;
                NavigationController.Instance.ChartDataManager = chartDataManager;
                NavigationController.Instance.CoordGrid = grid;

                return chartDataManager;
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

            Coroutine load;
            public void StayInPosition(bool isDown,float time = 0)
            {
                if (isDown)
                {
                    load = StartCoroutine(FluctuationDelayLoader(time));
                }
                else
                {
                    StopCoroutine(load);
                }
            }

            IEnumerator FluctuationDelayLoader(float time)
            {
                while (TryLoadNextFluctuation())
                {
                    yield return new WaitForSecondsRealtime(time);
                }

            }
        }
    }
}
