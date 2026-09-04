using UnityEngine;

public class ItemInteractable : MonoBehaviour, IInteractable
{
    public AudioClip pickupSound;
    public AudioClip pickupFailSound;

    public Item item;
    public string InteractionPrompt => "Pick Up" + item.itemName;

    public bool CanInteract(Interactor interactor)
    {
        return true;
    }

    public void Interact(Interactor interactor)
    {
        if (InventoryManager.instance.AddItem(item))
        {
            Debug.Log($"Picked up {item.itemName}!");
            Destroy(gameObject);
            AudioManager.instance.PlaySFX(pickupSound);
        }
        else
        {
            AudioManager.instance.PlaySFX(pickupFailSound, 3f);
        }
    }
}
