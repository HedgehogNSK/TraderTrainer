using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections;
using System.Collections.Generic;

public class IdenticalMeshSelector : EditorWindow
{
    [MenuItem("Tools/Meshes/Select identical meshes...")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        IdenticalMeshSelector window = (IdenticalMeshSelector)GetWindow(typeof(IdenticalMeshSelector));
        window.Show();
    }

    [SerializeField]
    public MeshFilter[] meshFilters;
    public Mesh[] targetMeshes;
    public bool includeInactive;

    void OnEnable()
    {
        //if (targetMeshes == null)
        {
            List<Mesh> meshesList = new List<Mesh>();
            Object[] gos = Selection.objects;
            foreach (var go in gos)
            {
                GameObject tempGo = go as GameObject;
                if (tempGo != null)
                {
                    MeshFilter temp = tempGo.GetComponent<MeshFilter>();
                    if (temp != null && !meshesList.Contains(temp.sharedMesh))
                    {
                        meshesList.Add(temp.sharedMesh);
                    }
                }
                else
                {
                    Mesh tempMesh = (go as Mesh);
                    if (tempMesh != null && !meshesList.Contains(tempMesh))
                    {
                        meshesList.Add(tempMesh);
                    }
                }
            }
            targetMeshes = meshesList.ToArray();
        }

    }

    void OnGUI()
    {
        if (GUILayout.Button("Reload meshes"))
        {
            OnEnable();
        }

        GUILayout.Label("Selected meshes:", EditorStyles.boldLabel);

        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty stringsProperty = so.FindProperty("targetMeshes");
        EditorGUILayout.PropertyField(stringsProperty, true);
        so.ApplyModifiedProperties();

        //targetMeshes = EditorGUILayout.ObjectField(targetMeshes, typeof(Mesh)) as Mesh;


        includeInactive = GUILayout.Toggle(includeInactive, "Include inactive meshes");
        if (GUILayout.Button("Select!"))
        {
            Select();
        }
    }
    void Select()
    {
        List<GameObject> objectsList = new List<GameObject>();
        GameObject[] obj;
        if (includeInactive)
        {
            obj = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
        }
        else
        {
            obj = FindObjectsOfType<GameObject>();
        }
        foreach (var o in obj)
        {
            if(PrefabUtility.GetCorrespondingObjectFromSource(o) == null && PrefabUtility.GetPrefabObject(o.transform) != null)
            {
                continue;
            }
            MeshFilter filter = o.GetComponent<MeshFilter>();
            if (filter != null)
            {
                foreach (var targetMesh in targetMeshes)
                {
                    if (filter.sharedMesh == targetMesh)
                    {
                        objectsList.Add(o);
                    }
                }
            }
        }
        Selection.objects = objectsList.ToArray();
    }
}
