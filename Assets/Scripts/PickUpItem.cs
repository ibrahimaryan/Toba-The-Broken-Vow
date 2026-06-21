using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    [SerializeField] private string itemID;
    [SerializeField] private int amount = 1;
    [SerializeField] private AudioClip collectSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(itemID, amount);
                
                // Update quest UI jika Chapter 4 sedang aktif
                if (Chapter4StoryManager.Instance != null)
                {
                    Chapter4StoryManager.Instance.UpdateQuestStatus();
                }
            }

            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, Camera.main != null ? Camera.main.transform.position : transform.position);
            }

            Destroy(gameObject);
        }
    }
}
