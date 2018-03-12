using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Chart
{
    namespace Entity
    {
        public class ExtraData
        {

            float time;
            public float Time { get { return time; } }

            public ExtraData(float t)
            {
                time = t;
            }
        }
    }
}
