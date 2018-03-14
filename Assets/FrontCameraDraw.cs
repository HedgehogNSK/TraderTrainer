using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chart
{
    public class FrontCameraDraw : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        private void OnPostRender()
        {
            ChartDrawer.Instnace.DrawCross();
        }
    }
}