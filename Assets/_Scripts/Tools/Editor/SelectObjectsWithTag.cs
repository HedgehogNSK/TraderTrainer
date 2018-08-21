using UnityEngine;
using UnityEditor;
using System.Collections;

public class SelectObjectsWithTag : Editor {
    public static string tagName = "botout";


    [MenuItem("Tools/SelectWithTag")]
    public static void SelectWithTag()
    {
        Selection.objects = GameObject.FindGameObjectsWithTag(tagName);
    }
}
