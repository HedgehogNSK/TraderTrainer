using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;
namespace Chart
{
    [RequireComponent(typeof(Camera))]
    public class ExtraDataCamera : MonoBehaviour
    {
        Camera cam;
        Vector2 leftDownPixel, rightUpPixel;
        private void Awake()
        {
            cam = GetComponent<Camera>();

        }

        private void OnPostRender()
        {
            ChartDrawer.Instance.DrawPointArray(0, Color.blue);
            ChartDrawer.Instance.DrawPointArray(1, Color.cyan);
        }
    }
}