using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;

namespace Chart
{
    namespace Entity
    {
        public class Candle : MonoBehaviour
        {

            [SerializeField] SpriteRenderer body, shadow, borders;

            [SerializeField]
            private Color downColor;
            [SerializeField]
            private Color upColor;
            [SerializeField]
            private Color shadowColor;


            public Color DownColor { get { return downColor; } set { downColor = value; } }
            public Color UpColor { get { return upColor; } set { upColor = value; } }
            public Color ShadowColor { get { return shadowColor; } set { shadowColor = value; } }
            public Color BorderDownColor { get; set; }
            public Color BorderUpColor { get; set; }

            public float PeriodBegin { get; set; }

            public bool Set(int position, PriceFluctuation fluctuation)
            {
                float bodySize = Mathf.Abs((float)(fluctuation.Open - fluctuation.Close));
                float shadowSize = (float)(fluctuation.High - fluctuation.Low);
                body.size = new Vector2(body.size.x, bodySize);
                borders.size = body.size + new Vector2(0.1f, 0.1f);
                shadow.size = new Vector2(shadow.size.x, shadowSize);

                if (fluctuation.Open > fluctuation.Close)
                {
                    body.color = downColor;
                }
                else
                {
                    body.color = upColor;

                }
                shadow.color = shadowColor;

                transform.position = new Vector2(position, bodySize / 2 + (float)fluctuation.Open);
                shadow.transform.position = new Vector2(position, shadowSize / 2 + (float)fluctuation.Low);
                PeriodBegin = (float)fluctuation.PeriodBegin;
                return true;
            }

            public bool Set(PriceFluctuation fluctuation)
            {
                float bodySize =(float) (fluctuation.Open - fluctuation.Close);
                float shadowSize = (float)(fluctuation.High - fluctuation.Low);
                body.size = new Vector2(body.size.x, Mathf.Abs(bodySize));
                borders.size = body.size + new Vector2(0.05f, 0.05f);
                shadow.size = new Vector2(shadow.size.x, shadowSize);

                if (bodySize < 0)
                {
                    body.color = downColor;
                }
                else
                {
                    body.color = upColor;

                }
                shadow.color = shadowColor;

                return true;
            }


        }
    }

}
