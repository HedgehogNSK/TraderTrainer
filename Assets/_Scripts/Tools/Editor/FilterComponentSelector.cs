using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.IO;

public class FilterComponentSelector : EditorWindow
{
    //[MenuItem("Tools/Components/Select objects  meshes...")]
    [MenuItem("CONTEXT/Component/Select with filter...")]
    private static void Start(MenuCommand data)
    {
        UnityEngine.Object context = data.context;
        if (context)
        {
            var comp = context as Component;
            var type = context.GetType();
            Debug.Log(context.name);
            if (comp)
                Init(comp, type);
        }
    }
    static void Init(Component _target, System.Type _type)
    {
        // Get existing open window or if none, make a new one:
        FilterComponentSelector window = (FilterComponentSelector)GetWindow(typeof(FilterComponentSelector));

        Component newComponent = window.CopyComponent(_target);
        window.serialized = new SerializedObject(newComponent);

        window.sourceComponent = _target;
        window.targetComponent = newComponent;

        window.fields = _target.GetType().GetFields();
        window.properties = _target.GetType().GetProperties();
        window.members = _target.GetType().GetMembers();

        window.fieldsFilters = new bool[window.fields.Length];
        window.propertiesFilters = new bool[window.properties.Length];

        window.targetType = _type;

        /*
        StreamReader sr = new StreamReader("%AppData%//..//Local//Unity//Editor//Editor.log");
        window.logFile = sr.ReadToEnd();
        */
        window.Show();

        

        //ScriptableObject scriptable = new ScriptableObject();
    }
    static GameObject tempGameObject;

    [SerializeField]
    public Component sourceComponent;
    public Component targetComponent;
    public System.Type targetType;

    public FieldInfo[] fields;
    public PropertyInfo[] properties;
    public MemberInfo[] members;

    public SerializedObject serialized;
    public bool[] fieldsFilters;
    public bool[] propertiesFilters;
    public bool[] membersFilters;
    public bool includeInactive;

    Vector2 scrollPos;

    //public string logFile;


    void OnGUI()
    {
        if (targetComponent == null || fields == null)
        {
            return;
        }

        //EditorGUILayout.TextArea(logFile);

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("Fields:", EditorStyles.boldLabel);


        for (int i = 0; i < fields.Length; i++)
        {
            fieldsFilters[i] = EditorGUILayout.BeginToggleGroup(fields[i].Name, fieldsFilters[i]);

            SerializedProperty stringsProperty = serialized.FindProperty(fields[i].Name);
            if (stringsProperty != null)
            {
                EditorGUILayout.PropertyField(stringsProperty, true);
            }

            serialized.ApplyModifiedProperties();

            EditorGUILayout.EndToggleGroup();
        }


        GUILayout.Label("Properties:", EditorStyles.boldLabel);
        for (int i = 0; i < properties.Length; i++)
        {
            //Debug.Log(properties[i].Name);
            //SerializedProperty stringsProperty = serialized.FindProperty(properties[i].Name);
            if (!properties[i].IsDefined(typeof(ObsoleteAttribute), true))
            {
                if (properties[i].Name != "mesh" && !properties[i].Name.Contains("preferred"))
                {
                    propertiesFilters[i] = EditorGUILayout.BeginToggleGroup(properties[i].Name, propertiesFilters[i]);
                    object tempObject = properties[i].GetValue(targetComponent, null);

                    if (tempObject != null)
                    {
                        UnityEngine.Object unityTempObject = (tempObject as UnityEngine.Object);
                        switch (properties[i].PropertyType.FullName)
                        {
                            case "System.Boolean":
                                EditorGUILayout.Toggle(unityTempObject);
                                break;
                            case "System.String":
                                EditorGUILayout.TextField(tempObject as string);
                                break;
                            case "UnityEngine.Color":
                                EditorGUILayout.ColorField((Color)tempObject);
                                break;
                            case "System.Single":
                                EditorGUILayout.FloatField((float)tempObject);
                                break;
                            case "System.Int32":
                                EditorGUILayout.IntField((int)tempObject);
                                break;
                            default:
                                //object temp = EditorGUILayout.ObjectField(unityTempObject, properties[i].PropertyType,true);
                                EditorGUILayout.TextField(properties[i].PropertyType.FullName);
                                /*
                                if (properties[i].GetSetMethod() != null)
                                {
                                    properties[i].SetValue(unityTempObject, temp, null);
                                }
                                */
                                break;
                        }
                    }
                    EditorGUILayout.EndToggleGroup();
                }
            }

            //serialized.ApplyModifiedProperties();

        }

        GUILayout.EndScrollView();

        includeInactive = GUILayout.Toggle(includeInactive, "Include inactive");
        if (GUILayout.Button("Select!"))
        {
            Select();
        }
    }

    void OnDestroy()
    {
        if (tempGameObject != null)
        {
            DestroyImmediate(tempGameObject);
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
            if (o.hideFlags != HideFlags.None && PrefabUtility.GetCorrespondingObjectFromSource(o) == null && PrefabUtility.GetPrefabObject(o.transform) != null)
            {
                continue;
            }

            Component comp = o.GetComponent(targetType);
            if (comp != null)
            {

                bool check = true;
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fieldsFilters[i])
                    {
                        object targetValue = fields[i].GetValue(targetComponent);
                        object checkValue = fields[i].GetValue(comp);
                        if (string.Compare(targetValue.ToString(), checkValue.ToString()) != 0)
                        {
                            check = false;
                        }
                    }
                }
                for (int i = 0; i < properties.Length; i++)
                {
                    if (propertiesFilters[i])
                    {
                        object targetValue = properties[i].GetValue(targetComponent, null);
                        object checkValue = properties[i].GetValue(comp, null);
                        if (!targetValue.Equals(checkValue))
                        {
                            check = false;
                        }
                    }
                }
                if (check)
                {
                    objectsList.Add(o);
                }
            }
        }
        Selection.objects = objectsList.ToArray();
    }

    T CopyComponent<T>(T original) where T : Component
    {
        if (tempGameObject != null)
        {
            DestroyImmediate(tempGameObject);
        }
        tempGameObject = new GameObject("Temp");
        tempGameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
        System.Type type = original.GetType();



        Component copy = tempGameObject.AddComponent(type);
        FieldInfo[] fields = type.GetFields();
        PropertyInfo[] properties = type.GetProperties();
        foreach (FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        foreach (var property in properties)
        {
            if (property.GetSetMethod() != null && property.CanWrite)
            {
                if (property.Name != "mesh")
                {
                    property.SetValue(copy, property.GetValue(original, null), null);
                }
            }
        }
        return copy as T;
    }
}
