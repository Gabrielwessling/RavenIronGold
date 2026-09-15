using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponInput : MonoBehaviour
{
    [SerializeField] private InputActionReference fireAction;
    [SerializeField] private InputActionReference reloadAction;

    private WeaponController weaponController;

    private void Awake()
    {
        weaponController = GetComponent<WeaponController>();
    }

    private void OnEnable()
    {
        fireAction.action.Enable();
        fireAction.action.performed += OnFire;

        reloadAction.action.Enable();
        reloadAction.action.performed += OnReload;

        fireAction.action.canceled += OnFireCanceled;
    }

    private void OnDisable()
    {
        fireAction.action.performed -= OnFire;
        fireAction.action.Disable();

        reloadAction.action.performed -= OnReload;
        reloadAction.action.Disable();

        fireAction.action.canceled -= OnFireCanceled;
    }

    private void OnFire(InputAction.CallbackContext context)
    {
        weaponController.StartFiring();
    }

    private void OnReload(InputAction.CallbackContext context)
    {
        weaponController.TryReload();
    }

    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        weaponController.StopFiring();
    }
}