using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(MapController))]
public class MapControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DirectoryInfo directoryInfo = new(Application.persistentDataPath);
        FileInfo[] files = directoryInfo.GetFiles("*.mapdata");

        GUILayout.Label(string.Format("Chunks in cache : {0}", files.Length));

        if (GUILayout.Button("Clear cache"))
        {
            foreach (FileInfo file in files)
                file.Delete();
        }
    }
}