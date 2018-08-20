using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

[CustomEditor(typeof(ColorPickerButton))]
public class PickerButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ColorPickerButton pickerBtn = (ColorPickerButton)target;

        pickerBtn.colorPickerPrefab =(GameObject) EditorGUILayout.ObjectField("Префаб ColorPicker",pickerBtn.colorPickerPrefab, typeof(GameObject),true);

        // Show default inspector property editor
        DrawDefaultInspector();
    }
}