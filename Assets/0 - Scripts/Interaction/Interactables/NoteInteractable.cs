using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NoteInteractable : MonoBehaviour, IInteractable
{
    public RawImage noteImage;
    public TMP_Text noteText;
    [TextArea] public string noteContent;
    public InputActionAsset inputActions;

    private InputAction closeAction;

    public string InteractionPrompt => "Read Note";

    public bool CanInteract(Interactor interactor)
    {
        return noteImage != null;
    }

    private void OnEnable()
    {
        if (!inputActions)
            return;

        closeAction = inputActions.FindAction("UI/Cancel", true);
        closeAction.Enable();
    }

    private void OnDisable()
    {
        closeAction?.Disable();
    }

    public void Interact(Interactor interactor)
    {
        if (!noteImage)
            return;

        if (noteText)
            noteText.text = noteContent;

        noteImage.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (noteImage && noteImage.gameObject.activeSelf && closeAction != null && closeAction.WasPressedThisFrame())
            noteImage.gameObject.SetActive(false);
    }
}
