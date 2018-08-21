using UnityEngine;
using UnityEditor;
using System.Collections;

public class DeveloperHelper : MonoBehaviour {

    [MenuItem("Tools/Dev/Clear PlayerPrefs")]
    public static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
