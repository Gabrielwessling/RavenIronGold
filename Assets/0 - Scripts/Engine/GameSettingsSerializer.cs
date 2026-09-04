using UnityEngine;

public static class SettingsSerializer
{
    public static string Serialize(GameSettings settings)
    {
        return JsonUtility.ToJson(settings, true);
    }

    public static GameSettings Deserialize(string json)
    {
        return JsonUtility.FromJson<GameSettings>(json);
    }
}