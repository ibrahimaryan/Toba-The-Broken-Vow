using UnityEngine;
using System.Collections;

public class ChoppableAssets : MonoBehaviour
{
    public enum AssetType { Tree, Rock }

    [Header("Asset Settings")]
    [SerializeField] private AssetType type = AssetType.Tree;
    [SerializeField] private int health = 3;
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private string hitAnimationTrigger = "hit"; // Parameter trigger di Animator Player

    [Header("Save Settings")]
    [Tooltip("ID unik opsional untuk menyimpan status hancur. Jika kosong, digenerate dari nama & posisi.")]
    [SerializeField] private string uniqueID;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip destroySound;

    private bool isPlayerInRange = false;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        string key = GetSavedKey();
        if (GameManager.Instance != null && GameManager.Instance.IsFlagSet(key))
        {
            Destroy(gameObject);
        }
    }

    private string GetSavedKey()
    {
        if (!string.IsNullOrEmpty(uniqueID))
        {
            return "choppable_" + uniqueID;
        }
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return "choppable_" + sceneName + "_" + gameObject.name + "_" + transform.position.x.ToString("F2") + "_" + transform.position.y.ToString("F2");
    }

    private void OnEnable()
    {
        PlayerControllerScript.OnInteractPressed += HandleInteraction;
    }

    private void OnDisable()
    {
        PlayerControllerScript.OnInteractPressed -= HandleInteraction;
        if (isPlayerInRange && InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt();
        }
    }

    private void Update()
    {
        if (!IsAxeCollected()) return;

        if (isPlayerInRange)
        {
            UpdateInteractionPrompt();
        }
    }

    private bool IsAxeCollected()
    {
        return GameManager.Instance != null && GameManager.Instance.IsFlagSet("chapter3_axe_collected");
    }

    private bool isChopSequenceRunning = false;

    private void HandleInteraction()
    {
        if (!IsAxeCollected()) return;
        if (!isPlayerInRange) return;
        if (isChopSequenceRunning) return;

        bool hasCorrectTool = false;
        if (InventoryManager.Instance != null)
        {
            string activeTool = InventoryManager.Instance.currentEquippedItem;
            if (type == AssetType.Tree && activeTool == "kapak")
            {
                hasCorrectTool = true;
            }
            else if (type == AssetType.Rock && activeTool == "equipment2") // equipment2 diisi pemecah_batu
            {
                hasCorrectTool = true;
            }
        }

        if (hasCorrectTool)
        {
            StartCoroutine(PlayChopSequenceCoroutine());
        }
        else
        {
            Debug.Log($"Tidak bisa menghancurkan. Butuh alat yang cocok untuk: {type}");
        }
    }

    private IEnumerator PlayChopSequenceCoroutine()
    {
        isChopSequenceRunning = true;

        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null)
        {
            player.StopMovement();
            player.ToggleInput(false); // Matikan input agar tidak bisa spam atau bergerak saat memukul

            // Tentukan arah hadap ke objek
            bool itemIsOnLeft = transform.position.x < player.transform.position.x;
            player.LockFacingDirection(itemIsOnLeft, 0.5f); // Kunci arah hadap selama 0.6 detik

            Animator animator = player.GetComponent<Animator>();
            if (animator != null && !string.IsNullOrEmpty(hitAnimationTrigger))
            {
                animator.ResetTrigger(hitAnimationTrigger);
                animator.SetTrigger(hitAnimationTrigger);
            }
        }

        // Hantam objek
        GetChopped(1);

        // Tunggu hingga animasi selesai sebelum mengizinkan input kembali
        yield return new WaitForSeconds(0.6f);

        if (player != null)
        {
            player.ToggleInput(true);
        }

        isChopSequenceRunning = false;
    }

    public void GetChopped(int damage)
    {
        health -= damage;

        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        if (health <= 0)
        {
            // Panggil coroutine untuk menunda kehancuran visual dan spawn drop item
            StartCoroutine(DestroyWithDelayCoroutine(0.5f));
        }
    }

    private IEnumerator DestroyWithDelayCoroutine(float delay)
    {
        // 1. Simpan status hancur ke GameManager agar tidak muncul lagi saat reload
        string key = GetSavedKey();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag(key, true);
        }

        // 2. Matikan collider agar player bisa langsung melewati objek & tidak bisa memukulnya lagi selama delay
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt();
        }

        // 3. Putar audio kehancuran tepat saat hantaman terakhir mendarat
        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, Camera.main != null ? Camera.main.transform.position : transform.position);
        }

        // 4. Tunggu selama delay (misal 0.5 detik)
        yield return new WaitForSeconds(delay);

        // 5. Spawn barang bawaan (kayu / kerikil) setelah delay selesai
        SpawnDropItem();

        // 6. Pulihkan input kontrol player
        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null)
        {
            player.ToggleInput(true);
        }

        // 7. Hancurkan GameObject pohon/batu ini sepenuhnya
        Destroy(gameObject);
    }

    private void SpawnDropItem()
    {
        if (dropPrefab != null)
        {
            Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }
    }

    private void UpdateInteractionPrompt()
    {
        if (InteractionPromptUI.Instance == null) return;

        bool hasCorrectTool = false;
        if (InventoryManager.Instance != null)
        {
            string activeTool = InventoryManager.Instance.currentEquippedItem;
            if (type == AssetType.Tree && activeTool == "kapak")
            {
                hasCorrectTool = true;
            }
            else if (type == AssetType.Rock && activeTool == "equipment2")
            {
                hasCorrectTool = true;
            }
        }

        if (type == AssetType.Tree)
        {
            if (hasCorrectTool)
                InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk menebang Pohon");
            else
                InteractionPromptUI.Instance.ShowPrompt("Butuh Kapak Aktif (Tekan Q untuk ganti equipment)");
        }
        else if (type == AssetType.Rock)
        {
            if (hasCorrectTool)
                InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk memecah Batu");
            else
                InteractionPromptUI.Instance.ShowPrompt("Butuh Pemecah Batu Aktif (Tekan Q untuk ganti equipment)");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAxeCollected()) return;

        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            UpdateInteractionPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsAxeCollected()) return;

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