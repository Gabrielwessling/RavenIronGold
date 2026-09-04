public interface IInteractable
{
    string InteractionPrompt { get; }
    bool CanInteract(Interactor interactor);
    void Interact(Interactor interactor);
}
