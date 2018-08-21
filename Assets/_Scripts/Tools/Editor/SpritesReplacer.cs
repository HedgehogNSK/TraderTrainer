using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections;
using System.Collections.Generic;

public class SpritesReplacer : EditorWindow
{
    [MenuItem("Tools/Images/Select identical sprites...")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        SpritesReplacer window = (SpritesReplacer)GetWindow(typeof(SpritesReplacer));
        window.Show();
    }

    [SerializeField]
    public Image[] images;
    public Sprite[] targetSprites;
    public bool includeInactive;

    void OnEnable()
    {
        //if (targetMeshes == null)
        {
            List<Sprite> spritesList = new List<Sprite>();
            Object[] gos = Selection.objects;
            foreach (var go in gos)
            {
                GameObject tempGo = go as GameObject;
                if (tempGo != null)
                {
                    Image temp = tempGo.GetComponent<Image>();
                    if (temp != null && !spritesList.Contains(temp.sprite))
                    {
                        spritesList.Add(temp.sprite);
                    }
                }
                else
                {
                    Sprite tempSprite = (go as Sprite);
                    if (tempSprite != null && !spritesList.Contains(tempSprite))
                    {
                        spritesList.Add(tempSprite);
                    }
                }
            }
            targetSprites = spritesList.ToArray();
        }

    }

    void OnGUI()
    {
        if (GUILayout.Button("Reload sprites"))
        {
            OnEnable();
        }

        GUILayout.Label("Selected sprites:", EditorStyles.boldLabel);

        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty stringsProperty = so.FindProperty("targetSprites");
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
            Image image = o.GetComponent<Image>();
            if (image != null)
            {
                foreach (var targetSprite in targetSprites)
                {
                    if (image.sprite == targetSprite)
                    {
                        objectsList.Add(o);
                    }
                }
            }
        }
        Selection.objects = objectsList.ToArray();
    }
}
