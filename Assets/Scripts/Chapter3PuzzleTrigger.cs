using UnityEngine;

public class Chapter3PuzzleTrigger : MonoBehaviour
{
    [Header("Puzzle Reference")]
    [SerializeField] private Chapter3PuzzleManager puzzleManager;

    [Header("Blink Settings (Optional)")]
    [SerializeField] private float blinkSpeed = 1.5f;
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.4f;

    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;
    private bool isPlayerInRange = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    [Header("Activation Flag (Optional)")]
    [SerializeField] private string activationFlag = "chapter3_npc_sequence_played";

    private bool IsActivated()
    {
        if (GameManager.Instance == null) return true;
        // Direct hardcode check to avoid stale serialized inspector values, fallback to the activationFlag field
        return GameManager.Instance.IsFlagSet("chapter3_npc_sequence_played") || 
               (!string.IsNullOrEmpty(activationFlag) && GameManager.Instance.IsFlagSet(activationFlag));
    }

    private void Start()
    {
        // Mulai efek berkedip jika puzzle belum diselesaikan dan sudah diaktifkan oleh alur cerita
        if (GameManager.Instance != null && !GameManager.Instance.IsFlagSet("chapter3_puzzle_solved"))
        {
            if (IsActivated())
            {
                StartBlink();
            }
        }
    }

    public void StartBlink()
    {
        if (blinkCoroutine == null && spriteRenderer != null)
        {
            blinkCoroutine = StartCoroutine(BlinkEffect());
        }
    }

    public void StopBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    private System.Collections.IEnumerator BlinkEffect()
    {
        while (spriteRenderer != null)
        {
            float lerpTime = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            float alpha = Mathf.Lerp(minAlpha, 1f, lerpTime);
            spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
    }

    private void OnEnable()
    {
        PlayerControllerScript.OnInteractPressed += HandleInteraction;

        // Mulai kembali efek berkedip saat aktif jika puzzle belum diselesaikan dan sudah aktif
        if (GameManager.Instance != null && !GameManager.Instance.IsFlagSet("chapter3_puzzle_solved"))
        {
            if (IsActivated())
            {
                StartBlink();
            }
        }
    }

    private void OnDisable()
    {
        PlayerControllerScript.OnInteractPressed -= HandleInteraction;
        StopBlink();
    }

    private bool IsDialogueActive()
    {
        bool standardDialogueActive = DialogueManager.instance != null && 
               ((DialogueManager.instance.screenBoxPanel != null && DialogueManager.instance.screenBoxPanel.activeInHierarchy) ||
                (DialogueManager.instance.bubblePanel != null && DialogueManager.instance.bubblePanel.activeInHierarchy) ||
                (DialogueManager.instance.cutsceneBoxPanel != null && DialogueManager.instance.cutsceneBoxPanel.activeInHierarchy));

        bool vnDialogueActive = false;
        var vnManager = FindAnyObjectByType<DialogueManagerCS>();
        if (vnManager != null)
        {
            vnDialogueActive = vnManager.IsPlaying;
        }

        return standardDialogueActive || vnDialogueActive;
    }

    private void HandleInteraction()
    {
        if (!isPlayerInRange) return;

        // Cegah membuka puzzle jika masih ada dialog yang sedang aktif
        if (IsDialogueActive()) return;

        // Cegah jika belum diaktifkan (belum berdialog dengan Opung)
        if (!IsActivated()) return;

        // Buka panel puzzle
        if (puzzleManager != null)
        {
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.HidePrompt();
            }

            puzzleManager.OpenPuzzle();
            
            // Matikan kedipan saat panel dibuka
            StopBlink();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (IsActivated() && InteractionPromptUI.Instance != null && GameManager.Instance != null && !GameManager.Instance.IsFlagSet("chapter3_puzzle_solved"))
            {
                InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk periksa");
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

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // Tampilkan/perbarui prompt interaksi secara dinamis ketika pemain berada di dalam range
        if (isPlayerInRange && !GameManager.Instance.IsFlagSet("chapter3_puzzle_solved"))
        {
            if (IsActivated() && !IsDialogueActive())
            {
                if (InteractionPromptUI.Instance != null)
                {
                    InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk periksa");
                }
            }
            else
            {
                if (InteractionPromptUI.Instance != null)
                {
                    InteractionPromptUI.Instance.HidePrompt();
                }
            }
        }
    }
}
