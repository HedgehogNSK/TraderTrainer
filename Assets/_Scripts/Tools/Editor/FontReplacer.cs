using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.IO;
using UnityEngine.UI;
using System.Linq;

public class FontReplacer : EditorWindow
{
    //[MenuItem("Tools/Components/Select objects  meshes...")]
    [MenuItem("Tools/UI/Replace font...")]
    [MenuItem("CONTEXT/Component/Replace font...")]
    private static void Start(MenuCommand data)
    {
        UnityEngine.Object context = data.context;
        //if (context)
        {
            var comp = context as Component;
            Type type = null;
            if (context != null)
            {
                type = context.GetType();
            }
           // Debug.Log(context.name);
            //if (comp)
                Init(comp, type);
        }
    }
    static void Init(Component _target, System.Type _type)
    {
        Text textComponent = _target as Text;
        FontReplacer window = (FontReplacer)GetWindow(typeof(FontReplacer));
        if (textComponent != null)
        {
            window.fontToReplace = textComponent.font;
        }
        window.Show();
    }

    public Font fontToReplace;
    public Font targetFont;
    
    public bool includeInactive;

    Vector2 scrollPos;

    //public string logFile;


    void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("Font to replace:", EditorStyles.boldLabel);
        fontToReplace = EditorGUILayout.ObjectField(fontToReplace, typeof(Font),true) as Font;
        GUILayout.Label("Target font:", EditorStyles.boldLabel);
        targetFont = EditorGUILayout.ObjectField(targetFont, typeof(Font), true) as Font;


        GUILayout.EndScrollView();

        includeInactive = GUILayout.Toggle(includeInactive, "Include inactive");
        if (GUILayout.Button("Replace all"))
        {
            Select();
        }
    }

    void Select()
    {
        //List<GameObject> objectsList = new List<GameObject>();
        Text[] obj;

        if (includeInactive)
        {
            obj = Resources.FindObjectsOfTypeAll(typeof(Text)) as Text[];
        }
        else
        {
            obj = FindObjectsOfType<Text>();
        }
        Text[] targetTexts = obj.Where(o => o.font == fontToReplace).ToArray();

        Undo.RegisterCompleteObjectUndo(targetTexts.ToArray(), "Replace font");
        foreach (var o in obj)
        {
            if (o.font == fontToReplace)
            {
                o.font = targetFont;
                EditorUtility.SetDirty(o);
            }
        }
    }
}
