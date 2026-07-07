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
                if (InteractionPromptUI.Instance != null)
                {
                    InteractionPromptUI.Instance.HidePrompt();
                }

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

                var player = FindAnyObjectByType<PlayerControllerScript>();
                if (doorOpenDialogue != null)
                {
                    if (player != null) player.ToggleInput(false); // Lock gerakan player
                    DialogueManager.instance.StartDialogue(doorOpenDialogue);
                    StartCoroutine(ShowUIAfterDialogue(player));
                }
                else
                {
                    ShowMemoryShardUI();
                }

                Debug.Log("Patung telah terpasang kail! Pintu terbuka.");
            }
            else
            {
                Debug.Log("Patung membutuhkan alat pancing...");
            }
        }
    }

    private void ShowMemoryShardUI()
    {
        if (MemoryShardManager.Instance != null && !string.IsNullOrEmpty(memoryShardID))
        {
            MemoryShardManager.Instance.UnlockShard(memoryShardID);
        }
        else if (MemoryShardManager.Instance != null)
        {
            MemoryShardManager.Instance.ShowShardPopup();
        }
    }

    private IEnumerator ShowUIAfterDialogue(PlayerControllerScript player)
    {
        yield return null; // Tunggu satu frame agar dialog sempat aktif

        while (DialogueManager.instance != null && 
               ((DialogueManager.instance.screenBoxPanel != null && DialogueManager.instance.screenBoxPanel.activeInHierarchy) ||
                (DialogueManager.instance.bubblePanel != null && DialogueManager.instance.bubblePanel.activeInHierarchy) ||
                (DialogueManager.instance.cutsceneBoxPanel != null && DialogueManager.instance.cutsceneBoxPanel.activeInHierarchy)))
        {
            yield return null;
        }

        ShowMemoryShardUI();

        // Kembalikan input player jika tidak tertahan oleh popup memory shard
        if (player != null && (MemoryShardManager.Instance == null || string.IsNullOrEmpty(memoryShardID)))
        {
            player.ToggleInput(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            bool isPuzzleSolved = GameManager.Instance != null && GameManager.Instance.IsFlagSet("chapter1_puzzle_solved");
            if (!isSolved && isPuzzleSolved && InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk pasang item");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.HidePrompt();
            }
        }
    }
}