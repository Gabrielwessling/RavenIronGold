using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private InteractionManager interactionManager;
    [SerializeField] private Animator animator;
    [SerializeField] private string interactTriggerName = "Interact";
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableLayers = ~0;

    private InputAction interactAction;
    private IInteractable currentInteractable;

    public IInteractable CurrentInteractable => currentInteractable;

    private void Awake()
    {
        if (!camera)
            camera = Camera.main;

        if (!interactionManager)
            interactionManager = GetComponent<InteractionManager>();

        if (!animator)
            animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (!inputActions)
            return;

        interactAction = inputActions.FindAction("Player/Interact", true);
        interactAction.Enable();
    }

    private void OnDisable()
    {
        interactAction?.Disable();
    }

    private void Update()
    {
        SetCurrentInteractable(FindInteractable());

        if (currentInteractable != null && interactAction != null && interactAction.WasPressedThisFrame())
        {
            interactionManager?.Interact(currentInteractable, this);

            if (animator != null && !string.IsNullOrEmpty(interactTriggerName))
                animator.SetTrigger(interactTriggerName);
        }
    }

    private IInteractable FindInteractable()
    {
        if (!camera)
            return null;

        Ray ray = new Ray(camera.transform.position, camera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayers))
            return null;

        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
        return interactable != null && interactable.CanInteract(this) ? interactable : null;
    }

    private void SetCurrentInteractable(IInteractable interactable)
    {
        if (ReferenceEquals(currentInteractable, interactable))
            return;

        currentInteractable = interactable;
        interactionManager?.SetCurrentInteractable(interactable);
    }

    private void OnDrawGizmosSelected()
    {
        if (!camera)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(camera.transform.position, camera.transform.forward * interactionDistance);
    }
}
