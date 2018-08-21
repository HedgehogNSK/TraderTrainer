using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ReferencesHelper {
    [MenuItem("CONTEXT/Component/Find references to this")]
    private static void FindReferences(MenuCommand data)
    {
        Object context = data.context;
        if (context)
        {
            var comp = context as Component;
            if (comp)
                FindReferencesTo(comp);
        }
    }


    [MenuItem("Assets/Find references to this %.")]
    private static void FindReferencesToAsset(MenuCommand data)
    {
        var selected = Selection.activeObject;
        GameObject go = selected as GameObject;
        Component[] components = go.GetComponents<Component>();
        if (selected)
        {
            foreach (Component comp in components)
            {
                FindReferencesTo(comp);
            }
        }
    }

    private static void FindReferencesTo(Object to)
    {
        var referencedBy = new List<Object>();
        var allObjects = Object.FindObjectsOfType<GameObject>();
        for (int j = 0; j < allObjects.Length; j++)
        {
            var go = allObjects[j];

            if (PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance)
            {
                if (PrefabUtility.GetCorrespondingObjectFromSource(go) == to)
                {
                    Debug.Log(string.Format(to.name + "/" + to.GetType() + ": referenced by {0}, {1}", go.name, go.GetType()), go);
                    referencedBy.Add(go);
                }
            }

            var components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                var c = components[i];
                if (!c) continue;

                var so = new SerializedObject(c);
                var sp = so.GetIterator();

                while (sp.NextVisible(true))
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (sp.objectReferenceValue == to)
                        {
                            Debug.Log(string.Format(to.name + "/" + to.GetType() + ": referenced by {0}, {1}", c.name, c.GetType()), c);
                            referencedBy.Add(c.gameObject);
                        }
                    }
            }
        }

        if (referencedBy.Any())
            Selection.objects = referencedBy.ToArray();
        else Debug.Log(to.name + "/" + to.GetType() + ": no references in scene");
    }
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
