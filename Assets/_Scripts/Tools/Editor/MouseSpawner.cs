using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections;
using System.Collections.Generic;

public class MouseSpawner : EditorWindow
{
    [MenuItem("Tools/Objects/Spawn objects...")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MouseSpawner window = (MouseSpawner)GetWindow(typeof(MouseSpawner));
        window.Show();
    }

    [SerializeField]
    public GameObject[] targetObjects;
    public bool includeInactive;
    public bool randomYRotation;
    public int index;
    public LayerMask raycastMask;
    public LayerMask excludeLayers;
    public GameObject selectedObject;

    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += SceneGUI;
        targetObjects = Selection.gameObjects;

        raycastMask = Physics.DefaultRaycastLayers;
        excludeLayers = 0;
    }
    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= SceneGUI;
    }

    void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= SceneGUI;

    }

    void SceneGUI(SceneView sceneView)
    {
        // This will have scene events including mouse down on scenes objects
        Event cur = Event.current;
        AddPoints();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Reload objects"))
        {
            OnEnable();
        }

        GUILayout.Label("Selected objects:", EditorStyles.boldLabel);

        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty stringsProperty = so.FindProperty("targetObjects");
        EditorGUILayout.PropertyField(stringsProperty, true);

        SerializedProperty raycastProperty = so.FindProperty("raycastMask");
        EditorGUILayout.PropertyField(raycastProperty, true);

        SerializedProperty excludeProperty = so.FindProperty("excludeLayers");
        EditorGUILayout.PropertyField(excludeProperty, true);

        so.ApplyModifiedProperties();

        randomYRotation = GUILayout.Toggle(randomYRotation, "Random Y rotation");

        includeInactive = GUILayout.Toggle(includeInactive, "Include inactive meshes");
    }


    private void AddPoints()
    {
        if (Event.current != null && Event.current.type == EventType.MouseDown && Event.current.button == 0 && targetObjects != null && targetObjects.Length > 0)
        {
            UseEvent();
            Ray ray = Camera.current.ScreenPointToRay(new Vector2(Event.current.mousePosition.x, Camera.current.pixelHeight - Event.current.mousePosition.y));
            RaycastHit hit;
            LayerMask totalMask = raycastMask | excludeLayers;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, totalMask))
            {
                int layers = (1 << hit.collider.gameObject.layer) & excludeLayers;
                if (layers != 0)
                {
                    Selection.activeGameObject = selectedObject != null ? selectedObject : targetObjects[0];
                }
                else
                {
                    Quaternion rotation = Quaternion.identity;
                    if (randomYRotation)
                    {
                        rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    }
                    GameObject random = targetObjects.RandomItem();
                    GameObject go = null;
                    Object prefab = PrefabUtility.GetCorrespondingObjectFromSource(random);
                    Object prefabObject = PrefabUtility.GetPrefabObject(random);
                    if (prefab == null || prefab != prefabObject)
                    // if (prefab == null || )
                    {
                        go = Instantiate(random, hit.point, rotation) as GameObject;
                        go.transform.parent = random.transform.parent;
                    }
                    else
                    {
                        Object tempp = PrefabUtility.InstantiatePrefab(prefab);
                        go = tempp as GameObject;

                        go.transform.position = hit.point;
                        go.transform.rotation = rotation;

                        Selection.activeGameObject = go;

                        if (PrefabUtility.GetPrefabObject(random) != random)
                        {
                            go.transform.parent = random.transform.parent;
                        }
                    }
                    go.name = random.name;
                    Undo.RegisterCreatedObjectUndo(go, "Spawn object");
                    selectedObject = go;
                    Selection.activeGameObject = go;
                }
            }
        }
    }

    private void UseEvent()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
        if (Event.current != null && Event.current.button == 0)// && (Event.current.type == EventType.mouseUp || Event.current.type == EventType.mouseDrag))
        {
            Event.current.Use();
        }
    }

}
