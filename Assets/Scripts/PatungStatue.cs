using UnityEngine;
using System.Collections;

public class PatungStatue : MonoBehaviour
{
    [Header("Aset & Target")]
    [SerializeField] private Sprite fullColorSprite; 
    [SerializeField] private DoorController door; 
    [SerializeField] private string memoryShardID = "Chapter1"; // ID shard yang mau di-unlock

    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 1.5f; 
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.4f; 

    private SpriteRenderer spriteRenderer;
    private bool isSolved = false;
    private bool isPlayerInRange = false;
    private Coroutine blinkCoroutine;

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

    // FUNGSI BARU: Dipanggil dari PasswordTerminal saat password benar
    public void StartBlinkEffect()
    {
        // Otomatis aktifkan game object jika masih mati (agar tidak error coroutine)
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        if (!isSolved && blinkCoroutine == null && spriteRenderer != null && gameObject.activeInHierarchy)
        {
            blinkCoroutine = StartCoroutine(BlinkEffect());
        }
    }

    // FUNGSI BARU: Untuk menghentikan kedip dan mengembalikan warna normal
    private void StopBlinkEffect()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        ResetSpriteColor();
    }

    private IEnumerator BlinkEffect()
    {
        while (!isSolved && spriteRenderer != null)
        {
            float lerpTime = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            float alpha = Mathf.Lerp(minAlpha, 1f, lerpTime);
            spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
    }

    private void ResetSpriteColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    private void PlaceItem()
    {
        if (isPlayerInRange && !isSolved)
        {
            if (InventoryManager.Instance.hasFishingRod)
            {
                isSolved = true;
                InventoryManager.Instance.UseFishingRod(); 

                StopBlinkEffect(); // TAMBAHAN: Matikan kedip karena patung sudah selesai!

                if (fullColorSprite != null)
                    spriteRenderer.sprite = fullColorSprite;

                if (door != null)
                    door.OpenDoor();

                Debug.Log("Patung telah terpasang kail! Pintu terbuka.");

                // MUNCULKAN UI MEMORY SHARD
                if (MemoryShardManager.Instance != null && !string.IsNullOrEmpty(memoryShardID))
                {
                    MemoryShardManager.Instance.UnlockShard(memoryShardID);
                }
                else if (MemoryShardManager.Instance != null)
                {
                    // Jika lupa ngisi ID, langsung munculkan saja
                    MemoryShardManager.Instance.ShowShardPopup();
                }
            }
            else
            {
                Debug.Log("Patung membutuhkan alat pancing...");
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