using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;


public class FileManagerEditor : EditorWindow
{
    [MenuItem("Tools/File Manager", false, 101)]
    static void Init()
    {
        FileManagerEditor window = GetWindow<FileManagerEditor>("File Manager") as FileManagerEditor;
        window.Show();
    }

    private EditorBuildSettingsScene[] scenes;
    public List<Object> objects = new List<Object>();

    public List<Object> usedObjects = new List<Object>();
    public List<string> usedFiles = new List<string>();

    public string[] allFilesPaths;
    public List<Object> allFilesList = new List<Object>();
    Vector2 scrollPos;

    public string[] ignoreContains;

    public List<string> fileSize = new List<string>();

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos,
                                                           false,
                                                           false,
                                                           GUILayout.Width(Screen.width),
                                                           GUILayout.Height(Screen.height - EditorGUIUtility.singleLineHeight * 1.5f));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        GUILayout.Label("Used Files");
        if (usedObjects.Count > 0)
        {
            for (int i = 0; i < usedObjects.Count; i++)
            {
                EditorGUILayout.ObjectField(usedObjects[i], typeof(Object), true);
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        GUILayout.Label("");
        if (usedObjects.Count > 0)
        {
            for (int i = 0; i < usedObjects.Count; i++)
            {
                if (GUILayout.Button("Select", GUILayout.Height(EditorGUIUtility.singleLineHeight / 1.07f)))
                {
                    Selection.activeObject = usedObjects[i];
                }
            }
        }
        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        if (true && allFilesList.Count > 0)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label("");
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight * allFilesPaths.Length) });
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("");
            if (fileSize.Count > 0)
            {
                for (int i = 0; i < fileSize.Count; i++)
                {
                    GUILayout.Label(fileSize[i]);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("");
            if (allFilesList.Count > 0)
            {
                for (int i = 0; i < allFilesList.Count; i++)
                {
                    if (GUILayout.Button("Delete", GUILayout.Height(EditorGUIUtility.singleLineHeight / 1.07f)))
                    {
                        FileUtil.DeleteFileOrDirectory(allFilesPaths[i]);
                        AssetDatabase.Refresh();
                    }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("All Files");
            if (allFilesList.Count > 0)
            {
                for (int i = 0; i < allFilesList.Count; i++)
                {
                    if (usedObjects.Count > 0)
                    {
                        if (!usedObjects.Contains(allFilesList[i]))
                        {
                            GUI.backgroundColor = Color.red;
                        }
                        else
                        {
                            GUI.backgroundColor = Color.white;
                        }
                    }
                    EditorGUILayout.ObjectField(allFilesList[i], typeof(Object),true);
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("ShowAllUsedFiles"))
        {
            usedFiles.Clear();
            usedObjects.Clear();
            objects.Clear();
            scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                objects.Add(AssetDatabase.LoadAssetAtPath(scenes[i].path, typeof(Object)));
                usedObjects.Add(AssetDatabase.LoadAssetAtPath(scenes[i].path, typeof(Object)));
            }

            Object[] collectedFiles = EditorUtility.CollectDependencies(objects.ToArray());
            for (int i = 0; i < collectedFiles.Length; i++)
            {
                string path = AssetDatabase.GetAssetPath(collectedFiles[i]);
                if (path.Contains("Asset"))
                {
                    //if (!CheckFilePath(path))
                    //{
                    if (CheckType(collectedFiles[i]))
                    {
                        if (!usedFiles.Contains(path))
                        {
                            usedFiles.Add(path);
                            Object t = (Object)AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                            usedObjects.Add(t);
                        }
                    }
                    //}
                }
            }
        }

        if (GUILayout.Button("GetAllFiles"))
        {
            fileSize.Clear();
            allFilesList.Clear();
            string[] info = Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories);
            List<string> infoPaths = new List<string>();
            infoPaths = info.ToList();
            for (int o = 0; o < infoPaths.Count; o++)
            {
                if (CheckIgnoreContains(infoPaths[o]))
                {
                    infoPaths.Remove(infoPaths[o]);
                    o--;
                }
            }
            allFilesPaths = new string[infoPaths.Count];
            for (int i = 0; i < allFilesPaths.Length; i++)
            {
                allFilesPaths[i] = infoPaths[i];
                Object t = (Object)AssetDatabase.LoadAssetAtPath(allFilesPaths[i], typeof(Object));
                allFilesList.Add(t);
                FileInfo fileInfo = new FileInfo(infoPaths[i]);
                //float size = ((int)(fileInfo.Length) / 100f;
                fileSize.Add((fileInfo.Length / 1000000f).ToString() + " mb");
            }
        }

        if (GUILayout.Button("Clear"))
        {
            fileSize.Clear();
            allFilesList.Clear();
            usedFiles.Clear();
            usedObjects.Clear();
            objects.Clear();
            scenes = new EditorBuildSettingsScene[0];
        }
        EditorGUILayout.EndScrollView();
    }

    private bool CheckFilePath (string filePath)
    {
        bool already = false;
        foreach (string p in usedFiles)
        {
            if (p == filePath)
            {
                already = true;
            }
        }
        return already;
    }

    private bool CheckType(Object ob)
    {
        bool normal = true;
        if (ob as Component != null)
        {
            normal = false;
        }
        return normal;
    }

    private bool CheckIgnoreContains(string p)
    {
        ignoreContains = new string[] { "Editor", "Plugins", "Analytics", ".meta", "GBNHZ", "Fyber", "StreamingAssets", "GURLs" };
        for (int i = 0; i < ignoreContains.Length; i++)
        {
            if (p.Contains(ignoreContains[i]))
            {
                return true;
            }
        }
        return false;
    }
}
