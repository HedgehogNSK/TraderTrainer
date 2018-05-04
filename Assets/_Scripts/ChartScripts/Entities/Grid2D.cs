using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;
using System;

namespace Chart
{
    public class Grid2D
    {
        float scaleY;
        DateTime scaleX;

        public Grid2D(DateTime scaleX, float scaleY)
        {
            this.scaleX = scaleX;
            this.scaleY = scaleY;
        }

        public float ScaleY
        {
            get { return scaleY; }
            set { scaleY = value; }
        }
        public DateTime ScaleX
        {
            get { return scaleX; }
            set { scaleX = value; }
        }
        public float FromDateToXAxis(DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public float FromPriceToYAxis(float price)
        {
            throw new NotImplementedException();
        }

        public DateTime FromXAxisToDate(float x)
        {
            throw new NotImplementedException();
        }

        public float FromYAxisToPrice(float y)
        {
            throw new NotImplementedException();
        }
    }
}
