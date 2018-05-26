using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Chart
{
    public interface INavigator : IDragHandler, IScrollHandler, IGrid
    {
        void GoToLastPoint();
        void ChangeScale(float stretch_coeffitient);
    }
}