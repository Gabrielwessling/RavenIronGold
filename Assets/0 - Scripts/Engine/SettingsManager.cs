using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public GameSettings Settings { get; private set; }

    public InputActionAsset inputManager;

    private string SettingsPath =>
        Path.Combine(Application.persistentDataPath, "settings.json");


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        Load();
    }

    public void ApplySettings(GameSettings settings)
    {
        Settings = settings;

        Apply();
        Save();
    }

    public void ApplyTemporary(GameSettings settings)
    {
        Settings = settings;

        Apply();
    }

    public void Load()
    {
        if (!File.Exists(SettingsPath))
        {
            Settings = new GameSettings();

            Save();
            Apply();

            return;
        }

        string json = File.ReadAllText(SettingsPath);

        Settings = SettingsSerializer.Deserialize(json);

        Apply();
    }


    public void Save()
    {
        string json =
            SettingsSerializer.Serialize(Settings);

        File.WriteAllText(
            SettingsPath,
            json
        );
    }


    public void Apply()
    {
        AudioListener.volume =
            Settings.audio.master;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMasterVolume(
                Settings.audio.master
            );

            AudioManager.instance.SetMusicVolume(
                Settings.audio.music
            );

            AudioManager.instance.SetSfxVolume(
                Settings.audio.sfx
            );
        }


        Screen.SetResolution(
            Settings.graphics.resolution.width,
            Settings.graphics.resolution.height,
            Settings.graphics.fullscreen
        );


        ApplyKeybindings();
    }


    private void ApplyBinding(
        string actionName,
        int bindingIndex,
        string bindingPath)
    {
        InputAction action =
            inputManager.FindAction(actionName);

        if (action == null)
        {
            Debug.LogWarning(
                $"InputAction não encontrada: {actionName}"
            );

            return;
        }

        action.ApplyBindingOverride(
            bindingIndex,
            bindingPath
        );
    }


    private void ApplyKeybindings()
    {
        ApplyBinding(
            "Player/Move",
            2,
            Settings.controls.keyboard.moveForward
        );

        ApplyBinding(
            "Player/Move",
            4,
            Settings.controls.keyboard.moveBackward
        );

        ApplyBinding(
            "Player/Move",
            6,
            Settings.controls.keyboard.moveLeft
        );

        ApplyBinding(
            "Player/Move",
            8,
            Settings.controls.keyboard.moveRight
        );

        ApplyBinding(
            "Player/Jump",
            0,
            Settings.controls.keyboard.jump
        );

        ApplyBinding(
            "Player/Interact",
            0,
            Settings.controls.keyboard.interact
        );

        ApplyBinding(
            "Player/Pause",
            0,
            Settings.controls.keyboard.pause
        );

        ApplyBinding(
            "Player/Sprint",
            0,
            Settings.controls.keyboard.sprint
        );

        ApplyBinding(
            "Player/Crouch",
            1,
            Settings.controls.keyboard.crouch
        );
    }


    public void SaveKeyBinding(
        string actionName,
        int bindingIndex,
        string bindingPath)
    {
        switch (actionName)
        {
            case "Move" when bindingIndex == 2:
                Settings.controls.keyboard.moveForward =
                    bindingPath;
                break;

            case "Move" when bindingIndex == 4:
                Settings.controls.keyboard.moveBackward =
                    bindingPath;
                break;

            case "Move" when bindingIndex == 6:
                Settings.controls.keyboard.moveLeft =
                    bindingPath;
                break;

            case "Move" when bindingIndex == 8:
                Settings.controls.keyboard.moveRight =
                    bindingPath;
                break;

            case "Jump":
                Settings.controls.keyboard.jump =
                    bindingPath;
                break;

            case "Interact":
                Settings.controls.keyboard.interact =
                    bindingPath;
                break;

            case "Pause":
                Settings.controls.keyboard.pause =
                    bindingPath;
                break;

            case "Sprint":
                Settings.controls.keyboard.sprint =
                    bindingPath;
                break;

            case "Crouch":
                Settings.controls.keyboard.crouch =
                    bindingPath;
                break;

            default:
                Debug.LogWarning(
                    $"Keybinding não configurado: {actionName}"
                );

                return;
        }

        Save();
    }
}