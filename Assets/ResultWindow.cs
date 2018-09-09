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
            decimal curProfit = PlayerManager.Instance.PapperProfit(PlayerManager.Instance.LastPrice);
            totalProfit.text =PlayerManager.Instance.TotalProfit.ToString("F2");
            profitPrecentage.text = float.IsNaN(PlayerManager.Instance.WinRate) ? "No Trades":PlayerManager.Instance.WinRate.ToString("F0")+"%";

            bestTrade.text = (PlayerManager.Instance.BestTrade ==decimal.MinValue)?"No trades": PlayerManager.Instance.BestTrade.ToString("F2");
            worstTrade.text = (PlayerManager.Instance.WorstTrade == decimal.MaxValue)? "No trades" : PlayerManager.Instance.WorstTrade.ToString("F2");
        }

        private void OnDestroy()
        {
            nextGameButton.onClick.RemoveListener(loadNewGame);
        }

    }
}
