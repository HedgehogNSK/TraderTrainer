using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Chart
{
    public class ChartInterface:MonoBehaviour
    {
        [SerializeField] Button toTheEnd;
        [SerializeField] Button settings;
        [SerializeField] Toggle autoscale;


        public void InterfaceSetActive(bool isOn)
        {
            toTheEnd.interactable = isOn;
            settings.interactable = isOn;
            autoscale.interactable = isOn;
        }
    }
}
