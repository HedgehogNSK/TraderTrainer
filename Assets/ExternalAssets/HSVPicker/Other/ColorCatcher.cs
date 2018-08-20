using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image),typeof(Button))]
public class ColorCatcher : MonoBehaviour {
    [SerializeField] ColorPicker picker;
    Image img;
    [SerializeField] Button accept;
    public Color PickerColor { get {return img.color; } }
    // Use this for initialization
    private void Awake()
    {
        img = GetComponent<Image>();
    }
    void Start () {
        picker.onValueChanged.AddListener(ChangeColor);

    }

    void ChangeColor(Color color)
    {
        img.color = color;
    }

    private void OnDestroy()
    {
        picker.onValueChanged.RemoveListener(ChangeColor);
    }

}
