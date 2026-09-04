using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GameObject menuObject;
    [SerializeField] private Behaviour playerController;
    [SerializeField] private bool pauseOnStart;

    private InputAction pauseAction;
    private bool isPaused;
    private bool playerControllerWasEnabled;

    private void Awake()
    {
        if (inputActions == null && SettingsManager.Instance != null)
            inputActions = SettingsManager.Instance.inputManager;

        if (playerController != null)
            playerControllerWasEnabled = playerController.enabled;

        SetPaused(pauseOnStart);
    }

    private void OnEnable()
    {
        if (inputActions == null)
        {
            Debug.LogWarning("PauseMenu: Input Action Asset não atribuído.");
            return;
        }

        pauseAction = inputActions.FindAction("Player/Pause", true);
        pauseAction.performed += TogglePause;
        pauseAction.Enable();
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= TogglePause;
            pauseAction.Disable();
        }

        Time.timeScale = 1f;
    }

    private void TogglePause(InputAction.CallbackContext context)
    {
        SetPaused(!isPaused);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (paused)
        {
            if (playerController != null)
            {
                playerControllerWasEnabled = playerController.enabled;
                playerController.enabled = false;
            }

            if (pauseAction != null)
                pauseAction.Enable();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (playerController != null)
                playerController.enabled = playerControllerWasEnabled;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (menuObject != null)
            menuObject.SetActive(paused);
    }
}
