using UnityEngine;

public class PatungStatue : MonoBehaviour
{
    [SerializeField] private Sprite fullColorSprite; // Masukkan aset patung warna di sini
    [SerializeField] private DoorController door; // Referensi ke script pintu kamu yang kemarin

    private SpriteRenderer spriteRenderer;
    private bool isSolved = false;
    private bool isPlayerInRange = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        PlayerControllerScript.OnInteractPressed += PlaceItem;
    }

    private void OnDisable()
    {
        PlayerControllerScript.OnInteractPressed -= PlaceItem;
    }

    private void PlaceItem()
    {
        if (isPlayerInRange && !isSolved)
        {
            // CEK: Apakah player bawa kail pancing?
            if (InventoryManager.Instance.hasFishingRod)
            {
                isSolved = true;
                InventoryManager.Instance.UseFishingRod(); // Hapus item dari inventory

                // 1. Ubah aset patung jadi Full Color
                if (fullColorSprite != null)
                    spriteRenderer.sprite = fullColorSprite;

                // 2. Buka Pintu & putar suara (memakai fungsi DoorController kemarin)
                if (door != null)
                    door.OpenDoor();

                Debug.Log("Patung telah terpasang kail! Pintu terbuka.");
            }
            else
            {
                Debug.Log("Patung membutuhkan alat pancing...");
                // Di sini kamu bisa memunculkan teks UI "Butuh kail pancing" jika mau
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = false;
    }
}