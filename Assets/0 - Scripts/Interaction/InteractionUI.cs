using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private InteractionManager interactionManager;
    [SerializeField] private TMP_Text interactionPrompt;
    [SerializeField] private Image crosshair;
    [SerializeField] private Sprite interactableCrosshair;
    [SerializeField] private Color interactableCrosshairColor = Color.white;

    private Sprite defaultCrosshair;
    private Color defaultCrosshairColor;

    private void Awake()
    {
        if (!interactionManager)
            interactionManager = FindFirstObjectByType<InteractionManager>();

        if (crosshair)
        {
            defaultCrosshair = crosshair.sprite;
            defaultCrosshairColor = crosshair.color;
        }

        SetInteractableState(null);
    }

    private void OnEnable()
    {
        if (interactionManager)
            interactionManager.CurrentInteractableChanged += SetInteractableState;
    }

    private void OnDisable()
    {
        if (interactionManager)
            interactionManager.CurrentInteractableChanged -= SetInteractableState;
    }

    private void Start()
    {
        if (interactionManager)
            SetInteractableState(interactionManager.CurrentInteractable);
    }

    private void SetInteractableState(IInteractable interactable)
    {
        bool canInteract = interactable != null;

        if (interactionPrompt)
        {
            interactionPrompt.gameObject.SetActive(canInteract);
            interactionPrompt.text = canInteract ? interactionManager.CurrentPrompt : string.Empty;
        }

        if (!crosshair)
            return;

        crosshair.sprite = canInteract && interactableCrosshair
            ? interactableCrosshair
            : defaultCrosshair;
        crosshair.color = canInteract
            ? interactableCrosshairColor
            : defaultCrosshairColor;
    }
}