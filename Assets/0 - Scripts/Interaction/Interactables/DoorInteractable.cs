using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Open Door";
    public float openSpeed = 4f;

    public bool getIsClosedPosition = true;
    public bool getIsClosedRotation = true;
    public bool getIsOpenPosition = true;
    public bool getIsOpenRotation = true;

    public AudioClip openSound;
    public AudioClip closeSound;

    public Transform doorTransform;

    public Vector3 originalPosition;
    public Vector3 originalRotation;

    public Vector3 openPosition;
    public Vector3 openRotation;

    public bool isOpen;

    void Awake()
    {
        if (getIsClosedPosition)
            originalPosition = doorTransform.position;
        if (getIsClosedRotation)
            originalRotation = doorTransform.rotation.eulerAngles;
        if (getIsOpenPosition)
            openPosition = doorTransform.position;
        if (getIsOpenRotation)
            openRotation = doorTransform.rotation.eulerAngles;

        if (isOpen)
        {
            doorTransform.position = openPosition;
            doorTransform.rotation = Quaternion.Euler(openRotation);
        } else
        {
            doorTransform.position = originalPosition;
            doorTransform.rotation = Quaternion.Euler(originalRotation);
        }
    }

    public bool CanInteract(Interactor interactor)
    {
        return true;
    }

    public void Interact(Interactor interactor)
    {
        isOpen = !isOpen;
        AudioManager.instance.PlaySFX(isOpen ? openSound : closeSound);
    }

    void Update()
    {
        doorTransform.position = Vector3.Lerp(doorTransform.position, isOpen ? openPosition : originalPosition, Time.deltaTime * openSpeed);
        doorTransform.rotation = Quaternion.Slerp(doorTransform.rotation, isOpen ? Quaternion.Euler(openRotation) : Quaternion.Euler(originalRotation), Time.deltaTime * openSpeed);
    }
}
