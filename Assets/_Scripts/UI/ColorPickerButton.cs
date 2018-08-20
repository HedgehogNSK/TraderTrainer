using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ColorPickerButton :Button {

    public GameObject colorPickerPrefab;
    ColorPicker picker;

    protected override void Awake()
    {
        base.Awake();
        this.onClick.AddListener(CreatePicker);
    }
    void CreatePicker()
    {
        if (!picker)
        {
            picker = Instantiate(colorPickerPrefab,FindObjectOfType<Canvas>().transform).GetComponent<ColorPicker>();
            picker.AcceptButton.onClick.AddListener(GetColor);
        }
    }

    void GetColor()
    {
        Destroy(picker.gameObject);
        this.image.color = picker.CurrentColor;
    }

}
