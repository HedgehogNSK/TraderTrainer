using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hedge.UnityUI;
namespace ChartGame
{
    public class EffectManager : MonoBehaviour
    {
        [SerializeField] GameObject winTrade;
        [SerializeField] GameObject loseTrade;
        [SerializeField] GameObject winTxt;
        [SerializeField] GameObject loseTxt;

        public void TradeResultEffect(bool good)
        {
            Instantiate(good?winTrade:loseTrade, transform);
        }

        public void ChangeResultEffect(string str, bool good)
        {
            GameObject go = Instantiate(good ? winTxt : loseTxt, transform);
            go.GetComponent<TextHideEffect>().SetText(str);
        }
    }
}
