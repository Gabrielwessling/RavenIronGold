using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Probe
{
    /// <summary>
    /// Saves and loads [DebugTweak] slider values as named JSON presets — so you
    /// can capture a tuning ("floaty jump", "hard mode") and bring it back later.
    /// Files live under Application.persistentDataPath/ProbePresets.
    /// </summary>
    public static class ProbePresets
    {
        [System.Serializable] class Entry { public string label; public float value; }
        [System.Serializable] class Data { public List<Entry> entries = new List<Entry>(); }

        public static string Folder => Path.Combine(Application.persistentDataPath, "ProbePresets");

        /// <summary>Save the current value of every registered tweak under <paramref name="name"/>.</summary>
        public static bool Save(string name)
        {
            var data = new Data();
            foreach (var t in ProbeTweaks.All)
            {
                try { data.entries.Add(new Entry { label = t.Label, value = t.Get() }); }
                catch { /* skip a tweak whose getter throws */ }
            }

            try
            {
                Directory.CreateDirectory(Folder);
                File.WriteAllText(PathFor(name), JsonUtility.ToJson(data, true));
                return true;
            }
            catch (System.Exception e) { Debug.LogWarning($"[Probe] Couldn't save preset '{name}': {e.Message}"); return false; }
        }

        /// <summary>Apply a saved preset to any tweaks whose label matches. Returns false if missing.</summary>
        public static bool Load(string name)
        {
            var path = PathFor(name);
            if (!File.Exists(path)) return false;

            Data data;
            try { data = JsonUtility.FromJson<Data>(File.ReadAllText(path)); }
            catch (System.Exception e) { Debug.LogWarning($"[Probe] Couldn't read preset '{name}': {e.Message}"); return false; }
            if (data?.entries == null) return false;

            var byLabel = new Dictionary<string, float>();
            foreach (var e in data.entries) byLabel[e.label] = e.value;

            foreach (var t in ProbeTweaks.All)
                if (byLabel.TryGetValue(t.Label, out var v))
                    try { t.Set(v); } catch { }

            return true;
        }

        /// <summary>Reset every tweak to the value it had when first registered.</summary>
        public static void Reset()
        {
            foreach (var t in ProbeTweaks.All)
                try { t.Set(t.Default); } catch { }
        }

        public static List<string> List()
        {
            var result = new List<string>();
            if (!Directory.Exists(Folder)) return result;
            foreach (var f in Directory.GetFiles(Folder, "*.json"))
                result.Add(Path.GetFileNameWithoutExtension(f));
            result.Sort();
            return result;
        }

        public static bool Delete(string name)
        {
            var path = PathFor(name);
            if (!File.Exists(path)) return false;
            try { File.Delete(path); return true; }
            catch { return false; }
        }

        static string PathFor(string name) => Path.Combine(Folder, Sanitize(name) + ".json");

        static string Sanitize(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "preset";
            foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name.Trim();
        }
    }
}
