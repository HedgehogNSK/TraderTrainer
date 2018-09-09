using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Chart
{
    public interface IChartDrawer
    {
        void DrawGrid(IEnumerable<DateTime> datesList, IEnumerable<decimal> pricesList);
    }
}

