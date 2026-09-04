using System;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private Interactor interactor;

    public event Action<IInteractable> CurrentInteractableChanged;
    public event Action<IInteractable> Interacted;

    public IInteractable CurrentInteractable { get; private set; }
    public string CurrentPrompt => "[E] " + CurrentInteractable?.InteractionPrompt;

    private void Awake()
    {
        if (!interactor)
            interactor = GetComponent<Interactor>();
    }

    public void SetCurrentInteractable(IInteractable interactable)
    {
        if (ReferenceEquals(CurrentInteractable, interactable))
            return;

        CurrentInteractable = interactable;
        CurrentInteractableChanged?.Invoke(CurrentInteractable);
    }

    public void Interact(IInteractable interactable, Interactor interactor)
    {
        if (!ReferenceEquals(CurrentInteractable, interactable) || !interactable.CanInteract(interactor))
            return;

        interactable.Interact(interactor);
        Interacted?.Invoke(interactable);
    }
}
