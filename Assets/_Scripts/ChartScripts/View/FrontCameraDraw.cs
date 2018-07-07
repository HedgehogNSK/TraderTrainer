using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;
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
            ChartDrawer.Instance.DrawCross();
        }
    }
}