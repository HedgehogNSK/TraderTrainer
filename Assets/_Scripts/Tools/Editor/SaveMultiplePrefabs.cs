using System.Collections;
using UnityEngine;
using UnityEditor;

public class SaveMultiplePrefabs : EditorWindow
{

    [MenuItem("Tools/Save all selected prefabs")]
    public static void SaveAllPrefabs()
    {
        GameObject[] objects = Selection.gameObjects;
        Undo.RecordObjects(objects, "Save selected prefabs");
        foreach (var go in objects)
        {
            GameObject target = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject;
            if (target != null)
            {
                PrefabUtility.ReplacePrefab(go, target);
                PrefabUtility.RevertPrefabInstance(go);
            }
        }
    }
    [MenuItem("Tools/Revert all selected prefabs")]
    public static void RevertAllPrefabs()
    {
        GameObject[] objects = Selection.gameObjects;
        Undo.RecordObjects(objects, "Revert selected prefabs");
        foreach (var go in objects)
        {
            GameObject target = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject;
            if (target != null)
            {
                //PrefabUtility.ReplacePrefab(go, target);
                PrefabUtility.RevertPrefabInstance(go);
            }
        }
    }
}