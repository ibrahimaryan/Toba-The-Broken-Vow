using UnityEngine;
using System.Collections;

public class EquipmentPickupTrigger : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 1.5f;
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.4f;

    [Header("Dialogue (Optional)")]
    [SerializeField] private Dialogue pickupDialogue;

    [Header("UI PopUp Settings")]
    [SerializeField] private GameObject popupPanel;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip pickupSound;
    private AudioSource audioSource;

    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;
    private bool isPlayerInRange = false;
    private bool isPanelActive = false;
    private bool justOpened = false;

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
        // Cek jika equipment sudah dikumpulkan sebelumnya
        if (GameManager.Instance != null && GameManager.Instance.IsFlagSet("chapter4_pickaxe_collected"))
        {
            gameObject.SetActive(false);
            return;
        }

        // Mulai efek berkedip
        if (spriteRenderer != null)
        {
            blinkCoroutine = StartCoroutine(BlinkEffect());
        }
    }

    private void OnEnable()
    {
        PlayerControllerScript.OnInteractPressed += HandleInteraction;
    }

    private void OnDisable()
    {
        PlayerControllerScript.OnInteractPressed -= HandleInteraction;
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
    }

    private void Update()
    {
        if (isPanelActive)
        {
            if (justOpened)
            {
                justOpened = false;
                return;
            }

            if (UnityEngine.InputSystem.Keyboard.current != null && 
                (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame || 
                 UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame ||
                 UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                ClosePanel();
            }
        }
    }

    private void HandleInteraction()
    {
        if (isPanelActive)
        {
            ClosePanel();
            return;
        }

        if (!isPlayerInRange) return;

        CollectEquipment();
    }

    private void CollectEquipment()
    {
        Debug.Log("Alat Pemecah Batu berhasil diambil dan dipasang sebagai Equipment 2!");

        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt();
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.EquipEquipment2();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag("chapter4_pickaxe_collected", true);
        }

        // Update Misi
        if (Chapter4StoryManager.Instance != null)
        {
            Chapter4StoryManager.Instance.UpdateQuestStatus();
        }

        if (pickupDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(pickupDialogue);
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            isPanelActive = true;
            justOpened = true;

            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null) player.ToggleInput(false);

            if (spriteRenderer != null) spriteRenderer.enabled = false;
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void ClosePanel()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        isPanelActive = false;

        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null) player.ToggleInput(true);

        gameObject.SetActive(false);
    }

    private IEnumerator BlinkEffect()
    {
        while (spriteRenderer != null)
        {
            float lerpTime = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            float alpha = Mathf.Lerp(minAlpha, 1f, lerpTime);
            spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (InteractionPromptUI.Instance != null && GameManager.Instance != null && !GameManager.Instance.IsFlagSet("chapter4_pickaxe_collected"))
            {
                InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk mengambil Alat Pemecah Batu");
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
