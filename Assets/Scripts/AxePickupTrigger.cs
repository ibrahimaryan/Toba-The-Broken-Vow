using UnityEngine;
using System.Collections;

public class AxePickupTrigger : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 1.5f; 
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.4f; 

    [Header("Dialogue (Optional)")]
    [SerializeField] private Dialogue pickupDialogue; // Dialog yang terpicu saat kapak diambil

    [Header("UI PopUp Settings")]
    [Tooltip("Panel UI yang bertuliskan 'Anda mendapatkan Kapak!'")]
    [SerializeField] private GameObject getAxePanel;

    [Header("Story Events")]
    [SerializeField] private UnityEngine.Events.UnityEvent OnAxeCollected;

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
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        // Cek jika kapak sudah dikumpulkan sebelumnya
        if (GameManager.Instance != null && GameManager.Instance.IsFlagSet("chapter3_axe_collected"))
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

            // Jika panel aktif, tunggu player menekan E, ESC, atau Space untuk menutupnya
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame || 
                 UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame ||
                 UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                CloseAxePanel();
            }
        }
    }

    private void HandleInteraction()
    {
        // Jika sedang menampilkan panel, tombol E ditekan di luar jangkauan trigger juga tetap menutup panel
        if (isPanelActive)
        {
            CloseAxePanel();
            return;
        }

        if (!isPlayerInRange) return;

        CollectAxe();
    }

    private void CollectAxe()
    {
        Debug.Log("Kapak berhasil diambil dan dipasang sebagai Equipment!");

        // Putar suara mengambil kapak jika ada
        if (pickupSound != null)
        {
            PlaySoundPersistent(pickupSound);
        }

        // Sembunyikan prompt interaksi saat mengambil kapak
        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt();
        }

        // 1. Masukkan ke slot equipment (bukan inventory biasa)
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.EquipAxe();
        }

        // 2. Simpan status di GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag("chapter3_axe_collected", true);
        }

        if (Chapter3StoryManager.Instance != null)
        {
            Chapter3StoryManager.Instance.UpdateObjectivePointer();
        }

        // 3. Putar dialog pengambilan jika ada
        if (pickupDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(pickupDialogue);
        }

        // 4. Jika ada panel "Anda mendapatkan Kapak!", tampilkan
        if (getAxePanel != null)
        {
            getAxePanel.SetActive(true);
            isPanelActive = true;
            justOpened = true;

            // Matikan pergerakan & input player agar mereka fokus membaca panel
            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null) player.ToggleInput(false);

            // Sembunyikan sprite Kapak di tanah dan matikan colliderya
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
            // Jika tidak ada panel popup, panggil event langsung dan matikan gameobject ini
            OnAxeCollected?.Invoke();
            
            gameObject.SetActive(false);
        }
    }

    private void CloseAxePanel()
    {
        if (getAxePanel != null)
        {
            getAxePanel.SetActive(false);
        }

        isPanelActive = false;

        // Hidupkan kembali input player
        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null) player.ToggleInput(true);

        // Panggil event setelah kapak dikoleksi dan panel ditutup
        OnAxeCollected?.Invoke();

        // Setelah panel ditutup, kita baru menonaktifkan Game Object Kapak ini secara penuh
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
            if (InteractionPromptUI.Instance != null && GameManager.Instance != null && !GameManager.Instance.IsFlagSet("chapter3_axe_collected"))
            {
                InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk mengambil Kapak");
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

    public bool IsPanelActive()
    {
        return isPanelActive;
    }
}

