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
        ChartGame.GamePreferences gamePrefs;
        private void Awake()
        {
            cam = GetComponent<Camera>();

        }
        private void Start()
        {
            gamePrefs = ChartGame.GamePreferences.Instance;   
        }

        private void OnPostRender()
        {
            ChartDrawer.Instance.DrawPointArray(0, gamePrefs.Fast_ma_color);
            ChartDrawer.Instance.DrawPointArray(1, gamePrefs.Slow_ma_color);
        }
    }
}