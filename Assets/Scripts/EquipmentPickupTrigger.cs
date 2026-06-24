using UnityEngine;
using System.Collections;

public class EquipmentPickupTrigger : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 1.5f;
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.4f;

    [Header("Dialogue (Optional)")]
    [SerializeField] private Dialogue pickupDialogue;
    [SerializeField] private Dialogue approachDialogue; // Dialog saat mendekati objek (sebelum diambil)

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

        if (pickupSound != null)
        {
            PlaySoundPersistent(pickupSound);
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

    private void PlaySoundPersistent(AudioClip clip)
    {
        if (clip == null) return;
        GameObject tempGO = new GameObject("TempAudio_" + clip.name);
        AudioSource tempSource = tempGO.AddComponent<AudioSource>();
        tempSource.clip = clip;
        if (audioSource != null)
        {
            tempSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
            tempSource.volume = audioSource.volume;
            tempSource.pitch = audioSource.pitch;
            tempSource.spatialBlend = audioSource.spatialBlend;
        }
        else
        {
            tempSource.spatialBlend = 0f; // 2D Sound
        }
        tempSource.Play();
        Destroy(tempGO, clip.length);
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
            if (GameManager.Instance != null && !GameManager.Instance.IsFlagSet("chapter4_pickaxe_collected"))
            {
                bool approachTalked = GameManager.Instance.IsFlagSet("chapter4_pickaxe_approach_talked");
                
                Dialogue activeDialogue = null;
                if (Chapter4StoryManager.Instance != null && Chapter4StoryManager.Instance.PickaxeApproachDialogue != null)
                {
                    activeDialogue = Chapter4StoryManager.Instance.PickaxeApproachDialogue;
                }
                else
                {
                    activeDialogue = approachDialogue;
                }

                if (activeDialogue != null && !approachTalked)
                {
                    StartCoroutine(PlayApproachDialogueSequence(activeDialogue));
                }
                else
                {
                    ShowPickupPrompt();
                }
            }
        }
    }

    private void ShowPickupPrompt()
    {
        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk mengambil Alat Pemecah Batu");
        }
    }

    private IEnumerator PlayApproachDialogueSequence(Dialogue activeDialogue)
    {
        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null)
        {
            player.StopMovement();
            player.ToggleInput(false);
        }

        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt();
        }

        if (activeDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(activeDialogue);
            yield return new WaitForSeconds(0.1f);
            while (IsDialogueActive()) yield return null;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag("chapter4_pickaxe_approach_talked", true);
        }

        if (player != null) player.ToggleInput(true);

        if (isPlayerInRange)
        {
            ShowPickupPrompt();
        }
    }

    private bool IsDialogueActive()
    {
        if (DialogueManager.instance == null) return false;

        bool screenActive = DialogueManager.instance.screenBoxPanel != null && DialogueManager.instance.screenBoxPanel.activeSelf;
        bool bubbleActive = DialogueManager.instance.bubblePanel != null && DialogueManager.instance.bubblePanel.activeSelf;
        bool cutsceneActive = DialogueManager.instance.cutsceneBoxPanel != null && DialogueManager.instance.cutsceneBoxPanel.activeSelf;

        return screenActive || bubbleActive || cutsceneActive;
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
