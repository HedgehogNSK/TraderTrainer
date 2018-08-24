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

        }

        
    }
}
