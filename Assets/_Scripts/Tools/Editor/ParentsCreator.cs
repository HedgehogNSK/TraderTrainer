using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections;
using System.Collections.Generic;

public class ParentsCreator : EditorWindow
{
    [MenuItem("Tools/Objects/Create parents...")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        ParentsCreator window = (ParentsCreator)GetWindow(typeof(ParentsCreator));
        window.Show();
    }

    [SerializeField]
    public GameObject[] gameobjects;

    void OnEnable()
    {
        gameobjects = Selection.gameObjects;
    }

    void OnGUI()
    {
        if (GUILayout.Button("Reload selection"))
        {
            OnEnable();
        }

        GUILayout.Label("Selected objects:", EditorStyles.boldLabel);

        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty stringsProperty = so.FindProperty("gameobjects");
        EditorGUILayout.PropertyField(stringsProperty, true);
        so.ApplyModifiedProperties();
        
        if (GUILayout.Button("Create parents for them!"))
        {
            CreateParents();
        }
    }

    void CreateParents()
    {
        Undo.RecordObjects(gameobjects, "Create parents");
        foreach (var go in gameobjects)
        {
            Transform newParent = new GameObject().transform;
            newParent.parent = go.transform.parent;
            newParent.name = go.name + " (Parent)";

            newParent.localPosition = go.transform.localPosition;
            newParent.localRotation = go.transform.localRotation;

            go.transform.parent = newParent;
        }
    }
}
