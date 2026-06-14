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

    private void Start()
    {
        // Mulai efek berkedip jika puzzle belum diselesaikan
        if (GameManager.Instance != null && !GameManager.Instance.IsFlagSet("chapter3_puzzle_solved"))
        {
            StartBlink();
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
    }

    private void OnDisable()
    {
        PlayerControllerScript.OnInteractPressed -= HandleInteraction;
    }

    private void HandleInteraction()
    {
        if (!isPlayerInRange) return;

        // Buka panel puzzle
        if (puzzleManager != null)
        {
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
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}
