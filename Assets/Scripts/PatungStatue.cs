using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PatungStatue : MonoBehaviour
{
    [Header("Aset & Target")]
    [SerializeField] private Sprite fullColorSprite; 
    [SerializeField] private DoorController door; 
    [SerializeField] private string memoryShardID = "Chapter1"; // ID shard yang mau di-unlock
    [SerializeField] private Dialogue doorOpenDialogue;
    [SerializeField] private string statueSolvedFlag = "chapter1_statue_solved";

    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 1.5f; 
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.4f; 

    [Header("Audio Settings")]
    [SerializeField] private AudioClip opendoorsound;

    private SpriteRenderer spriteRenderer;
    private bool isSolved = false;
    private bool isPlayerInRange = false;
    private Coroutine blinkCoroutine;
    private AudioSource audioSource;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsFlagSet(statueSolvedFlag))
        {
            isSolved = true;
            StopBlinkEffect();
            if (fullColorSprite != null)
                spriteRenderer.sprite = fullColorSprite;
        }
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
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetFlag(statueSolvedFlag, true);
                }
                InventoryManager.Instance.UseFishingRod(); 
                if (ToDoManager.Instance != null)
                {
                    // Angka 1 berarti mencoret misi urutan KEDUA di daftar misi Chapter tersebut
                    ToDoManager.Instance.SelesaikanMisi(2); 
                }

                if (audioSource != null && opendoorsound != null)
                {
                    audioSource.PlayOneShot(opendoorsound);
                }

                StopBlinkEffect(); // TAMBAHAN: Matikan kedip karena patung sudah selesai!

                if (fullColorSprite != null)
                    spriteRenderer.sprite = fullColorSprite;

                if (door != null)
                    door.OpenDoor();

                if (doorOpenDialogue != null)
                {
                    DialogueManager.instance.StartDialogue(doorOpenDialogue);
                }

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