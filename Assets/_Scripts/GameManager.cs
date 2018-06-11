using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

using Hedge.Tools;

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
            SimpleChartViewer db;
            SQLChartViewer sqlDB;
            IChartDataManager chartDataManager;
            IDateWorkFlow dateWorkFlow;
            IGrid grid;
            public event Action GoToNextFluctuation;
            public event Func<bool> CanWeGoToNextFluctuation;
            public int firstFluctuationID = 200;
            public int fluctuationsCountToLoad = 100;

            // Use this for initialization
            private void Awake()
            {
                if (!candleDummy || !candlesParent)
                    Debug.LogError("[GameObject]"+name + ": Задай все параметры");
            }
            void Start()
            {
                chartDataManager = new CryptoCompareDataManager(tframe: new TimeFrame(Period.Hour, 1));
                dateWorkFlow = chartDataManager as IDateWorkFlow;
                dateWorkFlow.SetWorkDataRange(firstFluctuationID, fluctuationsCountToLoad);
                if (dateWorkFlow != null)
                {
                    CanWeGoToNextFluctuation += dateWorkFlow.AddTimeStep;
                }
                grid = new CoordinateGrid(chartDataManager.DataBeginTime, chartDataManager.TFrame);
                chartDrawer.ChartDataManager = chartDataManager;
                chartDrawer.CoordGrid = grid;
                NavigationController.Instance.ChartDataManager = chartDataManager;
                NavigationController.Instance.CoordGrid = grid;
               
                //sqlDB = new SQLChartViewer(new TimeFrame(Period.Hour,2));
                //DateTime dt = sqlDB.GetPrice(0);
                //Debug.Log(DateTime.SpecifyKind(dt, DateTimeKind.Local).ToString());
                //Debug.Log(sqlDB.TyrToSetPairByAcronym("Test", "Test2"));
                //Debug.Log(sqlDB.ChartBeginTime);
                //Debug.Log(sqlDB.ChartEndTime);
            }

            internal IChartDataManager GenerateGame(Mode mode = Mode.Simple)
            {
                gameMode = mode;
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
                    if (dateWorkFlow.AddTimeStep())
                    GoToNextFluctuation();
                    else
                    {
                        Debug.Log("Больше нечего отрисовывать");
                    }
                }
            }
        }
    }
}
