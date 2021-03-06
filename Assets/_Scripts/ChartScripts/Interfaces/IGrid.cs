﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Hedge.Tools;
namespace Chart
{
    public interface IGrid
    {
        float Scale { get; set; }
        event Action<float> OnScaleChange;
        DateTime ZeroPoint { get; set; }
        TimeFrame Step { get; set; }
        DateTime FromXAxisToDate(float x);
        int FromDateToXAxis(DateTime dateTime);
        float FromYAxisToPrice(float y);
        float FromPriceToYAxis(float price);
    }
}