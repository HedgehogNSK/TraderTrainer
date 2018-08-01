using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Chart.Controllers;
using Chart.Entity;
public class SelectedFluctuationOutput : MonoBehaviour {

    [SerializeField] Text openText;
    [SerializeField] Text highText;
    [SerializeField] Text lowText;
    [SerializeField] Text closText;
    [SerializeField] Text volumeText;
	// Use this for initialization
	void Start () {
        NavigationController.Instance.OnFluctuationSelect += ChangeText;

    }
	
    void ChangeText(PriceFluctuation fluct)
    {
        if (fluct != null)
        {
            openText.text = fluct.Open.ToString();
            highText.text = fluct.High.ToString();
            lowText.text = fluct.Low.ToString();
            closText.text = fluct.Close.ToString();
            volumeText.text = fluct.Volume.ToString();
        }
    }

    private void OnDestroy()
    {
        if (NavigationController.Instance != null) NavigationController.Instance.OnFluctuationSelect -= ChangeText;
    }
}
