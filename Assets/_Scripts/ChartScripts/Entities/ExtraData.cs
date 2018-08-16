using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Chart
{
    namespace Entity
    {
        public class ExtraData
        {
            object data;
            System.Type dataType;
            float time;
            public float Time { get { return time; } }

            public ExtraData(object fluct)
            {
                data = fluct;
                dataType =  fluct.GetType();
            }

            public ExtraData(float t)
            {
                time = t;
            }
        }
    }
}
