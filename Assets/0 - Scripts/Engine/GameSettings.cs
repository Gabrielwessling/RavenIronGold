using System;
using System.Collections.Generic;

[Serializable]
public class GameSettings
{
    public AudioSettings audio = new();
    public GraphicsSettings graphics = new();
    public ControlsSettings controls = new();
    public LanguageSettings language = new();
    public GameplaySettings gameplay = new();

    public GameSettings Clone()
    {
        string json =
            SettingsSerializer.Serialize(this);

        return SettingsSerializer.Deserialize(json);
    }
}

[Serializable]
public class AudioSettings
{
    public float master = 1.0f;
    public float music = 0.8f;
    public float sfx = 1.0f;
}

[Serializable]
public class GraphicsSettings
{
    public bool fullscreen = true;
    public ResolutionSettings resolution = new();

    public string fullscreenStyle = "borderless";
    public int graphicsQuality = 3;
}

[Serializable]
public class ResolutionSettings
{
    public int width = 1920;
    public int height = 1080;
}

[Serializable]
public class ControlsSettings
{
    public KeyboardSettings keyboard = new();
    public MouseSettings mouse = new();
}

[Serializable]
public class KeyboardSettings
{
    public string moveForward = "<Keyboard>/w";
    public string moveBackward = "<Keyboard>/s";
    public string moveLeft = "<Keyboard>/a";
    public string moveRight = "<Keyboard>/d";

    public string jump = "<Keyboard>/space";
    public string interact = "<Keyboard>/e";
    public string pause = "<Keyboard>/escape";

    public string sprint = "<Keyboard>/leftShift";
    public string crouch = "<Keyboard>/leftCtrl";
}

[Serializable]
public class MouseSettings
{
    public float sensitivity = 1.0f;
    public bool invertYAxis = false;
}

[Serializable]
public class LanguageSettings
{
    public string selected = "pt-BR";

    public List<string> available = new()
    {
        "en-US",
        "pt-BR"
    };
}

[Serializable]
public class GameplaySettings
{
    public int difficultyLevel = 1;
    public bool autoSave = true;
    public bool hints = true;
    public bool showSubtitles = true;
    public int autoSaveInterval = 10;
}
