using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Chart;
using Chart.Entity;
using Hedge.Tools;

namespace ChartGame
{
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
                    return _instance;
                }
                set
                {
                    if(!_instance)
                    {
                        _instance = FindObjectOfType<GameManager>();
                        if(!_instance)
                        {
                            Debug.LogError("Создай объект GameManager на сцене");
                        }
                    }
                }
            }
            #endregion

            [SerializeField]Candle candleDummy;
            [SerializeField] Transform candlesParent;
            [SerializeField]ChartDrawer chartDrawer;
            SimpleChartViewer db;
            SQLChartViewer sqlDB;
            IChartDataManager chartDataManager;
            // Use this for initialization
            private void Awake()
            {
                if (!candleDummy || !candlesParent)
                    Debug.LogError("[GameObject]"+name + ": Задай все параметры");
            }
            void Start()
            {

                chartDrawer.ChartDataManager = new CryptoCompareDataManager(tframe: new TimeFrame(Period.Minute, 5));
                Chart.Controllers.NavigationController.Instance.Initialize();
                // chartDrawer.DrawChart();
                //sqlDB = new SQLChartViewer(new TimeFrame(Period.Hour,2));
                //DateTime dt = sqlDB.GetPrice(0);
                //Debug.Log(DateTime.SpecifyKind(dt, DateTimeKind.Local).ToString());
                //Debug.Log(sqlDB.TyrToSetPairByAcronym("Test", "Test2"));
                //Debug.Log(sqlDB.ChartBeginTime);
                //Debug.Log(sqlDB.ChartEndTime);
            }

            // Update is called once per frame
            void Update()
            {

            }
        }
    }
}
