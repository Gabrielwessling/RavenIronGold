using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class GenerateAudioDatabases
{
    private const string SourceFolder = "Assets/Audio/BGM";
    private const string OutputFolder = "Assets/Resources/Audio/Databases";

    [MenuItem("Tools/Audio/Generate Audio Databases")]
    public static void Generate()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        if (!AssetDatabase.IsValidFolder("Assets/Resources/Audio"))
            AssetDatabase.CreateFolder("Assets/Resources", "Audio");

        if (!AssetDatabase.IsValidFolder(OutputFolder))
            AssetDatabase.CreateFolder("Assets/Resources/Audio", "Databases");

        if (!AssetDatabase.IsValidFolder(SourceFolder))
        {
            Debug.LogWarning($"Source folder not found: {SourceFolder}");
            return;
        }

        string[] folders = AssetDatabase.GetSubFolders(SourceFolder);
        foreach (string folder in folders)
        {
            string sceneName = Path.GetFileName(folder);

            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folder });
            string assetPath = $"{OutputFolder}/{sceneName}AudioDatabase.asset";
            var db = AssetDatabase.LoadAssetAtPath<AudioDatabase>(assetPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<AudioDatabase>();
                AssetDatabase.CreateAsset(db, assetPath);
            }

            db.sceneName = sceneName;

            var newEntries = new List<AudioDatabase.ClipEntry>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null) continue;

                AudioDatabase.ClipEntry existing = null;
                if (db.entries != null)
                    existing = db.entries.Find(e => e != null && e.clip == clip);

                var entry = new AudioDatabase.ClipEntry();
                entry.clip = clip;
                entry.volume = existing != null ? existing.volume : 1f;
                newEntries.Add(entry);
            }

            db.entries = newEntries;
            EditorUtility.SetDirty(db);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Audio databases generated.");
    }
}
