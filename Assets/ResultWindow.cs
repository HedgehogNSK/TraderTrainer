using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
namespace ChartGame
{

    public class ResultWindow : MonoBehaviour {

        [SerializeField] Text totalProfit;
        [SerializeField] Text profitPrecentage;
        [SerializeField] Text bestTrade;
        [SerializeField] Text worstTrade;

        [SerializeField] Button nextGameButton;
        UnityAction loadNewGame;
        // Use this for initialization
        void Start()
        {
            loadNewGame += () => { GameManager.Instance.LoadGame(GameManager.Instance.gameMode); };
            nextGameButton.onClick.AddListener(loadNewGame);
            gameObject.SetActive(false);

        }
        private void OnEnable()
        {
            totalProfit.text = (PlayerManager.Instance.TotalProfit + PlayerManager.Instance.CurrentProfit(PlayerManager.Instance.CurrentPrice)).ToString("F2");
            profitPrecentage.text = PlayerManager.Instance.WinRate.ToString("F0")+"%";
            bestTrade.text = PlayerManager.Instance.BestTrade.ToString("F2");
            worstTrade.text = PlayerManager.Instance.WorstTrade.ToString("F2");
        }


    }
}
