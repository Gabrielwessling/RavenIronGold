using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider sfxVolume;

    [Header("Graphics")]
    [SerializeField] private TMP_Dropdown resolution;
    [SerializeField] private Toggle fullscreen;
    [SerializeField] private TMP_Dropdown fullscreenStyle;
    [SerializeField] private TMP_Dropdown graphicsQuality;

    [Header("Controls")]
    [SerializeField] private Slider mouseSensitivity;
    [SerializeField] private Toggle invertYAxis;

    [Header("Language")]
    [SerializeField] private TMP_Dropdown language;

    [Header("Gameplay")]
    [SerializeField] private TMP_Dropdown difficulty;
    [SerializeField] private Toggle autoSave;
    [SerializeField] private Toggle hints;
    [SerializeField] private Toggle subtitles;
    [SerializeField] private Slider autoSaveInterval;

    [Header("Buttons")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button cancelButton;

    [Header("Key Bindings")]
    [SerializeField] private KeyBindingButton keyBindingPrefab;
    [SerializeField] private Transform keyBindingContainer;

    private GameSettings temporarySettings;

    private readonly List<KeyBindingButton> keyBindingButtons =
        new List<KeyBindingButton>();


    private void Awake()
    {
        SetupButtons();
        SetupDropdowns();
        SetupSliders();
        CreateKeyBindings();
    }


    private void OnEnable()
    {
        temporarySettings =
            SettingsManager.Instance.Settings.Clone();

        LoadSettingsIntoUI();
    }


    private void SetupButtons()
    {
        applyButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        applyButton.onClick.AddListener(Apply);
        cancelButton.onClick.AddListener(Cancel);
    }


    private void SetupDropdowns()
    {
        SetupResolutionDropdown();
        SetupFullscreenStyleDropdown();
        SetupGraphicsQualityDropdown();
        SetupLanguageDropdown();
        SetupDifficultyDropdown();
    }


    private void SetupSliders()
    {
        masterVolume.minValue = 0f;
        masterVolume.maxValue = 1f;

        musicVolume.minValue = 0f;
        musicVolume.maxValue = 1f;

        sfxVolume.minValue = 0f;
        sfxVolume.maxValue = 1f;

        mouseSensitivity.minValue = 0.1f;
        mouseSensitivity.maxValue = 5f;

        autoSaveInterval.minValue = 1f;
        autoSaveInterval.maxValue = 60f;
        autoSaveInterval.wholeNumbers = true;
    }


    private void SetupResolutionDropdown()
    {
        resolution.ClearOptions();

        List<string> options = new List<string>();

        foreach (Resolution res in Screen.resolutions)
        {
            options.Add(
                $"{res.width}x{res.height}"
            );
        }

        resolution.AddOptions(options);
    }


    private void SetupFullscreenStyleDropdown()
    {
        fullscreenStyle.ClearOptions();

        fullscreenStyle.AddOptions(
            new List<string>
            {
                "Exclusive",
                "Borderless",
                "Windowed"
            }
        );
    }


    private void SetupGraphicsQualityDropdown()
    {
        graphicsQuality.ClearOptions();

        graphicsQuality.AddOptions(
            new List<string>
            {
                "Low",
                "Medium",
                "High",
                "Ultra"
            }
        );
    }


    private void SetupLanguageDropdown()
    {
        language.ClearOptions();

        language.AddOptions(
            SettingsManager.Instance.Settings
                .language.available
        );
    }


    private void SetupDifficultyDropdown()
    {
        difficulty.ClearOptions();

        difficulty.AddOptions(
            new List<string>
            {
                "Easy",
                "Normal",
                "Hard",
                "Very Hard"
            }
        );
    }


    private void LoadSettingsIntoUI()
    {
        GameSettings settings =
            temporarySettings;


        masterVolume.SetValueWithoutNotify(
            settings.audio.master
        );

        musicVolume.SetValueWithoutNotify(
            settings.audio.music
        );

        sfxVolume.SetValueWithoutNotify(
            settings.audio.sfx
        );


        fullscreen.SetIsOnWithoutNotify(
            settings.graphics.fullscreen
        );

        graphicsQuality.SetValueWithoutNotify(
            settings.graphics.graphicsQuality
        );

        fullscreenStyle.SetValueWithoutNotify(
            GetFullscreenStyleIndex(
                settings.graphics.fullscreenStyle
            )
        );

        resolution.SetValueWithoutNotify(
            FindResolutionIndex(
                settings.graphics.resolution.width,
                settings.graphics.resolution.height
            )
        );


        mouseSensitivity.SetValueWithoutNotify(
            settings.controls.mouse.sensitivity
        );

        invertYAxis.SetIsOnWithoutNotify(
            settings.controls.mouse.invertYAxis
        );


        int languageIndex =
            settings.language.available.IndexOf(
                settings.language.selected
            );

        language.SetValueWithoutNotify(
            Mathf.Max(languageIndex, 0)
        );


        difficulty.SetValueWithoutNotify(
            settings.gameplay.difficultyLevel - 1
        );

        autoSave.SetIsOnWithoutNotify(
            settings.gameplay.autoSave
        );

        hints.SetIsOnWithoutNotify(
            settings.gameplay.hints
        );

        subtitles.SetIsOnWithoutNotify(
            settings.gameplay.showSubtitles
        );

        autoSaveInterval.SetValueWithoutNotify(
            settings.gameplay.autoSaveInterval
        );


        RefreshKeyBindings();
    }


    public void Apply()
    {
        GameSettings settings =
            temporarySettings;


        settings.audio.master =
            masterVolume.value;

        settings.audio.music =
            musicVolume.value;

        settings.audio.sfx =
            sfxVolume.value;


        settings.graphics.fullscreen =
            fullscreen.isOn;

        settings.graphics.graphicsQuality =
            graphicsQuality.value;

        settings.graphics.fullscreenStyle =
            GetFullscreenStyleValue(
                fullscreenStyle.value
            );

        SetResolutionFromDropdown(
            settings
        );


        settings.controls.mouse.sensitivity =
            mouseSensitivity.value;

        settings.controls.mouse.invertYAxis =
            invertYAxis.isOn;


        if (language.value >= 0 &&
            language.value <
            settings.language.available.Count)
        {
            settings.language.selected =
                settings.language.available[
                    language.value
                ];
        }


        settings.gameplay.difficultyLevel =
            difficulty.value + 1;

        settings.gameplay.autoSave =
            autoSave.isOn;

        settings.gameplay.hints =
            hints.isOn;

        settings.gameplay.showSubtitles =
            subtitles.isOn;

        settings.gameplay.autoSaveInterval =
            Mathf.RoundToInt(
                autoSaveInterval.value
            );


        SettingsManager.Instance.ApplySettings(
            temporarySettings
        );


        temporarySettings =
            SettingsManager.Instance.Settings.Clone();
    }


    public void Cancel()
    {
        SettingsManager.Instance.Load();

        temporarySettings =
            SettingsManager.Instance.Settings.Clone();

        LoadSettingsIntoUI();
    }


    private int FindResolutionIndex(
        int width,
        int height)
    {
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            Resolution resolution =
                Screen.resolutions[i];

            if (resolution.width == width &&
                resolution.height == height)
            {
                return i;
            }
        }

        return 0;
    }


    private void SetResolutionFromDropdown(
        GameSettings settings)
    {
        if (resolution.value < 0 ||
            resolution.value >=
            Screen.resolutions.Length)
        {
            return;
        }

        Resolution selected =
            Screen.resolutions[
                resolution.value
            ];

        settings.graphics.resolution.width =
            selected.width;

        settings.graphics.resolution.height =
            selected.height;
    }


    private string GetFullscreenStyleValue(
        int index)
    {
        switch (index)
        {
            case 0:
                return "exclusive";

            case 1:
                return "borderless";

            case 2:
                return "windowed";

            default:
                return "borderless";
        }
    }


    private int GetFullscreenStyleIndex(
        string value)
    {
        switch (value.ToLower())
        {
            case "exclusive":
                return 0;

            case "borderless":
                return 1;

            case "windowed":
                return 2;

            default:
                return 1;
        }
    }


    private void CreateKeyBindings()
    {
        InputActionMap playerMap =
            SettingsManager.Instance
                .inputManager
                .FindActionMap("Player");

        if (playerMap == null)
        {
            Debug.LogWarning(
                "InputActionMap 'Player' não encontrada."
            );

            return;
        }


        foreach (InputAction action in playerMap.actions)
        {
            for (int i = 0;
                 i < action.bindings.Count;
                 i++)
            {
                InputBinding binding =
                    action.bindings[i];


                if (binding.isComposite)
                    continue;


                if (binding.isPartOfComposite)
                    continue;


                if (!binding.path.StartsWith(
                    "<Keyboard>"))
                {
                    continue;
                }

                if (action.name == "Move" &&
                    !IsPrimaryMoveBinding(action, i))
                {
                    continue;
                }


                KeyBindingButton button =
                    Instantiate(
                        keyBindingPrefab,
                        keyBindingContainer
                    );


                button.Setup(
                    action,
                    i,
                    this
                );


                keyBindingButtons.Add(
                    button
                );
            }
        }
    }


    public void SetKeyBinding(
        string actionName,
        int bindingIndex,
        string bindingPath)
    {
        switch (actionName)
        {
            case "Move" when bindingIndex == 2:
                temporarySettings
                    .controls.keyboard.moveForward =
                    bindingPath;
                break;

            case "Move" when bindingIndex == 4:
                temporarySettings
                    .controls.keyboard.moveBackward =
                    bindingPath;
                break;

            case "Move" when bindingIndex == 6:
                temporarySettings
                    .controls.keyboard.moveLeft =
                    bindingPath;
                break;

            case "Move" when bindingIndex == 8:
                temporarySettings
                    .controls.keyboard.moveRight =
                    bindingPath;
                break;

            case "Jump":
                temporarySettings
                    .controls.keyboard.jump =
                    bindingPath;
                break;

            case "Interact":
                temporarySettings
                    .controls.keyboard.interact =
                    bindingPath;
                break;

            case "Pause":
                temporarySettings
                    .controls.keyboard.pause =
                    bindingPath;
                break;

            case "Sprint":
                temporarySettings
                    .controls.keyboard.sprint =
                    bindingPath;
                break;

            case "Crouch":
                temporarySettings
                    .controls.keyboard.crouch =
                    bindingPath;
                break;

            default:
                Debug.LogWarning(
                    $"Keybinding não configurado: {actionName}"
                );
                break;
        }
    }


    private bool IsPrimaryMoveBinding(
        InputAction action,
        int bindingIndex)
    {
        if (bindingIndex != 2 &&
            bindingIndex != 4 &&
            bindingIndex != 6 &&
            bindingIndex != 8)
        {
            return false;
        }

        string bindingName =
            action.bindings[bindingIndex].name;

        for (int i = 0; i < bindingIndex; i++)
        {
            InputBinding previousBinding =
                action.bindings[i];

            if (previousBinding.isPartOfComposite &&
                previousBinding.name == bindingName &&
                previousBinding.path.StartsWith(
                    "<Keyboard>"))
            {
                return false;
            }
        }

        return true;
    }


    private void RefreshKeyBindings()
    {
        foreach (KeyBindingButton button
                 in keyBindingButtons)
        {
            button.Refresh();
        }
    }
}