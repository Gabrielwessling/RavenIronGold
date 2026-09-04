using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyBindingButton : MonoBehaviour
{
    [SerializeField] private TMP_Text actionNameText;
    [SerializeField] private TMP_Text bindingNameText;
    [SerializeField] private Button button;

    private InputAction action;
    private int bindingIndex;
    private SettingsUI settingsUI;

    public void Setup(
        InputAction action,
        int bindingIndex,
        SettingsUI settingsUI)
    {
        this.action = action;
        this.bindingIndex = bindingIndex;
        this.settingsUI = settingsUI;

        actionNameText.text = action.name;

        UpdateBindingName();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(StartRebind);
    }

    public void Refresh()
    {
        UpdateBindingName();
    }

    private void UpdateBindingName()
    {
        bindingNameText.text =
            action.GetBindingDisplayString(bindingIndex);
    }

    private void StartRebind()
    {
        if (action == null)
            return;

        bindingNameText.text = "...";

        action.Disable();

        action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>")
            .OnComplete(operation =>
            {
                operation.Dispose();

                action.Enable();

                string bindingPath =
                    action.bindings[bindingIndex].overridePath;

                if (string.IsNullOrEmpty(bindingPath))
                {
                    bindingPath =
                        action.bindings[bindingIndex].effectivePath;
                }

                settingsUI.SetKeyBinding(
                    action.name,
                    bindingIndex,
                    bindingPath
                );

                UpdateBindingName();
            })
            .OnCancel(operation =>
            {
                operation.Dispose();

                action.Enable();

                UpdateBindingName();
            })
            .Start();
    }
}