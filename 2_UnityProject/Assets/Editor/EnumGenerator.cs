using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

public class EnumGenerator : EditorWindow
{
    private string folderPath = "";
    private string SavePath = "Assets/Enums";
    private string enumName = "";

    private bool addEnumPrefix = true;

    private bool ignoreMetaFiles = true;

    private string excludedFile = "";
    private List<string> excludedFiles = new List<string>();

    [MenuItem("Window/Enum Generator")]
    public static void ShowWindow()
    {
        GetWindow<EnumGenerator>("Enum Generator");
    }

    private void OnEnable()
    {
        Reset();
    }

    private void OnGUI()
    {
        //FolderPath Field
        EditorGUILayout.TextField("Folder Path", folderPath);
        Rect folderPathRect = GUILayoutUtility.GetLastRect();

        //ExcludedFiles Field
        GUILayout.BeginHorizontal();
        excludedFile = EditorGUILayout.TextField("Exclude File:", excludedFile);
        Rect excludedRect = GUILayoutUtility.GetLastRect();
        if (GUILayout.Button("+", GUILayout.Width(25)))
        {
            AddToExcluded();
        }
        GUILayout.EndHorizontal();

        if (excludedFiles.Count > 0)
        {
            EditorGUILayout.LabelField("Excluded Files:");
            for (int i = 0; i < excludedFiles.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(excludedFiles[i]);
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    excludedFiles.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
        }

        //EnumName Field
        EditorGUILayout.TextField("Enum Name", enumName);

        //SaveFolder Field
        EditorGUILayout.TextField("Enum Save Path", SavePath);
        Rect enumSaveFolderRect = GUILayoutUtility.GetLastRect();

        //Bool AddEnumToName
        addEnumPrefix = EditorGUILayout.Toggle("Add Enum Prefix", addEnumPrefix);

        //ignoreMetaFiles
        ignoreMetaFiles = EditorGUILayout.Toggle("Ignore Meta Files",ignoreMetaFiles);

        //Handle Drag and Drop
        Event currentEvent = Event.current;
        switch (currentEvent.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:

                bool bFolderPath = folderPathRect.Contains(currentEvent.mousePosition);
                bool bEnumSaveFolder = enumSaveFolderRect.Contains(currentEvent.mousePosition);
                bool bExcludedFile = excludedRect.Contains(currentEvent.mousePosition);

                if (!bFolderPath && !bEnumSaveFolder && !bExcludedFile)
                {
                    break;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (currentEvent.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    UnityEngine.Object[] objects = DragAndDrop.objectReferences;

                    if (bFolderPath)
                    {
                        folderPath = AssetDatabase.GetAssetPath(objects[0]);
                        string temp = Path.GetFileName(folderPath);
                        if (temp != null)
                        {
                            enumName = MakeValidCSharpIdentifier(temp);
                        }
                    }
                    else if (bEnumSaveFolder)
                    { 
                        SavePath = AssetDatabase.GetAssetPath(objects[0]);
                    }
                    else
                    {
                        for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                        {
                            excludedFile = DragAndDrop.objectReferences[i].name;
                            AddToExcluded();
                        }
                    }
                }

                Event.current.Use();

                break;
            default:
                break;
        }

        if (GUILayout.Button("Generate"))
        {
            GenerateEnum();
        }
        if (GUILayout.Button("Reset"))
        {
            Reset();
        }
    }

    private void AddToExcluded()
    {
        if (!excludedFiles.Contains(excludedFile))
        {
            excludedFiles.Add(excludedFile);
            excludedFile = "";
        }
    }

    private void Reset()
    {
        folderPath = enumName = excludedFile = "";
        SavePath = "Assets/Enums";
        excludedFiles.Clear();
    }


    public void GenerateEnum()
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.Log("Folderpath Empty!");
            return;
        }

        // Ensure that the enum save folder exists, create it if it doesn't.
        if (!AssetDatabase.IsValidFolder(SavePath))
        {
            AssetDatabase.CreateFolder("Assets", "Enums");
        }

        if (string.IsNullOrEmpty(this.enumName))
        {
            string temp = Path.GetFileName(folderPath);
            this.enumName = MakeValidCSharpIdentifier(temp);
        }

        string enumName = this.enumName;
        if (addEnumPrefix)
        {
            enumName = "E" + enumName;
        }

        string enumFilePath = Path.Combine(SavePath, enumName + ".cs");
        using (StreamWriter writer = new StreamWriter(enumFilePath))
        {
            writer.WriteLine("public enum " + enumName);
            writer.WriteLine("{");

            string[] fileNames = Directory.GetFiles(folderPath);

            //fileNames = fileNames.Where(filePath => !filePath.EndsWith(".meta"))
            //    .Select(Path.GetFileNameWithoutExtension)
            //    .Select(MakeValidCSharpIdentifier)
            //    .ToArray();

            if (ignoreMetaFiles)
                fileNames = RemoveMetaFiles(fileNames);

            for (int i = 0; i < fileNames.Length; i++)
            {
                fileNames[i] = Path.GetFileNameWithoutExtension(fileNames[i]);
                fileNames[i] = MakeValidCSharpIdentifier(fileNames[i]);
            }
            
            fileNames = RemoveDuplicatesAndExcluded(fileNames);



            for (int i = 0; i < fileNames.Length; i++)
            {
                writer.WriteLine("    " + fileNames[i] + ",");
            }

            writer.WriteLine("}");
        }

        AssetDatabase.Refresh(); 
        Debug.Log($"Enum Generated - {enumName}");
    }

    private string[] RemoveDuplicatesAndExcluded(string[] fileNames)
    {
        //Removes Duplicates
        for (int i = 0; i < fileNames.Length; i++)
        {
            if (fileNames[i] == null)
            {
                continue;
            }

            for (int j = i+1; j < fileNames.Length; j++)
            {
                if (fileNames[i] == fileNames[j])
                {
                    fileNames[j] = null;
                }
            }
        }

        List<string> names = new List<string>();
        for (int i = 0; i < fileNames.Length; i++)
        {
            if (fileNames[i] != null)
            {
                if (fileNames[i] == enumName)
                {
                    fileNames[i] = Char.IsUpper(fileNames[i][0]) ? "Default" : "default";
                }
                if (!excludedFiles.Contains(fileNames[i]))
                {
                    names.Add(fileNames[i]);
                }
            }
        }

        return names.ToArray();
    }

    private string[] RemoveMetaFiles(string[] fileNames)
    {
        //Removes Duplicates
        for (int i = 0; i < fileNames.Length; i++)
        {
            if (fileNames[i].Contains(".meta"))
            {
                fileNames[i] = null;
            }

        }

        List<string> names = new List<string>();
        for (int i = 0; i < fileNames.Length; i++)
        {
           if (fileNames[i]!=null)
            names.Add(fileNames[i]);
        }

        return names.ToArray();
    }


    private string MakeValidCSharpIdentifier(string input)
    {
        // Remove any characters that are not valid in C# identifiers.
        input = new string(input.Where(c => Char.IsLetterOrDigit(c) || c == '_').ToArray());

        // Ensure the identifier starts with a letter or underscore.
        if (!Char.IsLetter(input[0]) && input[0] != '_')
        {
            input = "_" + input;
        }

        return input;
    }
}
