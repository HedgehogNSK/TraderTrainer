﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;
using System;

namespace Chart
{
    namespace Entity
    {
        public class Candle : MonoBehaviour
        {
            static float scale = 0.1f;
            static public float Scale
            {
                get { return scale; }
                set
                {
                    OnScaleChange(value/scale);
                    scale = value;
                }
            }
            static public event Action<float> OnScaleChange;
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

            public DateTime PeriodBegin { get; set; }

            public bool Set(int position, PriceFluctuation fluctuation)
            {
                float bodyHeight = scale*((float)(fluctuation.Close - fluctuation.Open));
                float shadowHeight = (float)(scale *(fluctuation.High - fluctuation.Low));
                body.size = new Vector2(body.size.x, bodyHeight);
                borders.size = body.size + scale * new Vector2( 0.2f, bodyHeight>0?0.2f:-0.2f);
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

                transform.position = new Vector2(position, bodyHeight / 2 + scale * (float)fluctuation.Open);
                shadow.transform.position = new Vector2(position, shadowHeight / 2 + scale * (float)fluctuation.Low);
                PeriodBegin = fluctuation.PeriodBegin;
                OnScaleChange += ChangeScale;
                return true;
            }

            private void ChangeScale(float multiplier)
            {
                body.size = new Vector2(body.size.x, body.size.y *multiplier);
                borders.size = body.size + multiplier * new Vector2(0.2f, body.size.y>0? 0.2f:-0.2f);
                shadow.size = new Vector2(shadow.size.x, shadow.size.y * multiplier);
                transform.position = new Vector2(transform.position.x, transform.position.y* multiplier);
                shadow.transform.localPosition = new Vector2(shadow.transform.localPosition.x, shadow.transform.localPosition.y* multiplier);
            }

            public bool Set(PriceFluctuation fluctuation)
            {
                float bodySize =(float)(scale * (fluctuation.Open - fluctuation.Close));
                float shadowSize = (float)(scale * (fluctuation.High - fluctuation.Low));
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
                OnScaleChange += ChangeScale;
                return true;
            }


            private void OnDestroy()
            {
                OnScaleChange -= ChangeScale;
            }
        }


    }

}
