using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChartGame
{
    public class TradeInfo
    {
        public decimal OpenPrice { get; private set; }
        public decimal ClosePrice { get; private set; }
        public decimal AbsoluteProfit { get; private set; }
        public decimal RelativeProfit { get; private set; }
        public decimal PositionSize { get; private set; }
        public List<int> order_ids;

        public TradeInfo(decimal open_price, decimal close_price, decimal position_size)
        {
            OpenPrice = open_price;
            ClosePrice = close_price;
            PositionSize = position_size;
            if (position_size != 0)
            {
                RelativeProfit = position_size>0? close_price / open_price : open_price -close_price/open_price;
                AbsoluteProfit = RelativeProfit * position_size;
            }
            else
            {
                Debug.LogError("Неправильный трейд закрался");
            }
        }

    }
}

