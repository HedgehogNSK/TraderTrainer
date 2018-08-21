using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;
using System;
using Chart.Controllers;
namespace Chart
{
    namespace Entity
    {
        public class Candle : MonoBehaviour
        {
#pragma warning disable 0649
            [SerializeField] SpriteRenderer body, shadow, borders;
            [SerializeField] static float borderWidth = 0.15f;
            [SerializeField] private Color downColor;
            [SerializeField] private Color upColor;
            [SerializeField] private Color shadowColor;
#pragma warning restore 0649

            public Color DownColor { get { return downColor; } set { downColor = value; } }
            public Color UpColor { get { return upColor; } set { upColor = value; } }
            public Color ShadowColor { get { return shadowColor; } set { shadowColor = value; } }
            public Color BorderDownColor { get; set; }
            public Color BorderUpColor { get; set; }

            public DateTime PeriodBegin { get; set; }

            private IGrid grid;
            public IGrid Grid {
                get { return grid; }
                set
                {
                    if (grid != null)
                        grid.OnScaleChange -= ChangeScale;
                    grid = value;
                    grid.OnScaleChange += ChangeScale;
                }
            }
            
            public bool Set(PriceFluctuation fluctuation)
            {
                if(Grid==null)
                {
                    Debug.LogError("Сначала задайте Grid для свечки");
                }
                float bodyHeight = Grid.Scale*((float)(fluctuation.Close - fluctuation.Open));
                float shadowHeight = (float)(Grid.Scale * (fluctuation.High - fluctuation.Low));
                body.size = new Vector2(body.size.x, bodyHeight);
                borders.size = body.size + new Vector2(borderWidth, bodyHeight>0? borderWidth : -borderWidth);
                shadow.size = new Vector2(shadow.size.x, shadowHeight);

                if (fluctuation.Open > fluctuation.Close)
                {
                    body.color = downColor;
                }
                else
                {
                    body.color = upColor;

                }
                shadow.color = shadowColor;

                transform.position = new Vector2(Grid.FromDateToXAxis(fluctuation.PeriodBegin), bodyHeight / 2 + Grid.Scale * (float)fluctuation.Open);
                shadow.transform.position = new Vector2(shadow.transform.position.x, shadowHeight / 2 + Grid.Scale * (float)fluctuation.Low);
                PeriodBegin = fluctuation.PeriodBegin;
                return true;
            }

            public void ChangeScale(float multiplier)
            {
                body.size = new Vector2(body.size.x, body.size.y *multiplier);
                borders.size = body.size + new Vector2(borderWidth, body.size.y>0? borderWidth : -borderWidth);
                shadow.size = new Vector2(shadow.size.x, shadow.size.y * multiplier);
                transform.position = new Vector2(transform.position.x, transform.position.y* multiplier);
                shadow.transform.localPosition = new Vector2(shadow.transform.localPosition.x, shadow.transform.localPosition.y* multiplier);
            }

            private void OnDestroy()
            {
                grid.OnScaleChange -= ChangeScale;
            }
        }


    }

}
