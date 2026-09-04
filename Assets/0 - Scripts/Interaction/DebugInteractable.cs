using UnityEngine;

public class DebugInteractable : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Debug Interactable";
    public AudioClip debugSound;

    public bool CanInteract(Interactor interactor)
    {
        return true;
    }

    public void Interact(Interactor interactor)
    {
        Debug.Log("Interacted with DebugInteractable!");
        AudioManager.instance.PlaySFX(debugSound, 3f);
    }
}
