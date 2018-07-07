using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedge.Tools;
namespace Chart
{
    [RequireComponent(typeof(Camera))]
    public class VolumeCameraDraw : MonoBehaviour
    {
        Camera cam;
        Vector2 leftDownPixel, rightUpPixel;
        private void Awake()
        {
            cam = GetComponent<Camera>();
            
        }
        // Use this for initialization
        void Start()
        {
        }

        private void OnPostRender()
        {
            ChartDrawer.Instance.DrawVolume(cam.pixelRect);
        }
    }
}