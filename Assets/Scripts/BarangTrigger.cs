using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BarangTrigger : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private SisikPuzzleManager puzzleManager; 

    [Header("Spawn Settings")]
    [Tooltip("Daftar semua ID Lokasi Sisik yang telah dipasang di Scene (chapter2_ruang_tamu, chapter2_gudang, chapter2_dapur)")]
    [SerializeField] private string[] allLocationIDs = new string[] {
        "sisik_loc_0", "sisik_loc_1", "sisik_loc_2", "sisik_loc_3", "sisik_loc_4",
        "sisik_loc_5", "sisik_loc_6", "sisik_loc_7", "sisik_loc_8", "sisik_loc_9",
        "sisik_loc_10", "sisik_loc_11", "sisik_loc_12", "sisik_loc_13", "sisik_loc_14"
    };

    [Header("Dialogue (Optional)")]
    [SerializeField] private Dialogue startScatteringDialogue;
    [SerializeField] private Dialogue notEnoughScalesDialogue;

    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 1.5f; 
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.4f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip opensound; 

    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;
    private bool isPlayerInRange = false;
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
        // Mulai berkedip jika puzzle belum diselesaikan
        if (GameManager.Instance != null && !GameManager.Instance.IsFlagSet("sisik_puzzle_solved"))
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

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager tidak ditemukan!");
            return;
        }

        if (audioSource != null && opensound != null)
        {
            audioSource.PlayOneShot(opensound);
        }

        // Cek apakah quest mengumpulkan sisik sudah dimulai
        bool isSpawningActive = GameManager.Instance.IsFlagSet("sisik_spawning_active");
        int collectedCount = 0;
        if (InventoryManager.Instance != null)
        {
            collectedCount = InventoryManager.Instance.GetItemCount("sisik");
        }

        if (!isSpawningActive)
        {
            // 1. Acak & Tebarkan Sisik
            StartScattering();

            // 2. Buka panel dengan 0 item (belum ada yang terkumpul)
            if (puzzleManager != null)
            {
                puzzleManager.OpenPuzzle(0);
            }
        }
        else
        {
            // Buka panel dengan menampilkan jumlah item yang terkumpul
            if (puzzleManager != null)
            {
                puzzleManager.OpenPuzzle(collectedCount);
            }

            if (collectedCount < 7)
            {
                // Tampilkan pesan bahwa sisik belum lengkap
                if (notEnoughScalesDialogue != null && DialogueManager.instance != null)
                {
                    DialogueManager.instance.StartDialogue(notEnoughScalesDialogue);
                }
            }
        }
    }

    private void StartScattering()
    {
        if (ToDoManager.Instance != null)
                {
                    // Angka 1 berarti mencoret misi urutan KEDUA di daftar misi Chapter tersebut
                    ToDoManager.Instance.SelesaikanMisi(1); 
                }
        Debug.Log("Memulai penyebaran 7 sisik secara acak...");

        // Gunakan daftar allLocationIDs dari Inspector agar bisa mengacak lintas scene (karena scene lain tidak sedang diload)
        if (allLocationIDs == null || allLocationIDs.Length < 8)
        {
            Debug.LogError("[BarangTrigger] Error: Daftar 'All Location IDs' di Inspector kurang dari 7! Minimal butuh 7 lokasi.");
            return;
        }

        List<string> locationPool = new List<string>(allLocationIDs);
        List<string> selectedLocations = new List<string>();

        // Mengocok list menggunakan Fisher-Yates shuffle
        for (int i = 0; i < locationPool.Count; i++)
        {
            string temp = locationPool[i];
            int randomIndex = Random.Range(i, locationPool.Count);
            locationPool[i] = locationPool[randomIndex];
            locationPool[randomIndex] = temp;
        }

        // Ambil 7 lokasi pertama
        int countToSelect = Mathf.Min(7, locationPool.Count);
        string debugMsg = "Lokasi terpilih: ";
        for (int i = 0; i < countToSelect; i++)
        {
            selectedLocations.Add(locationPool[i]);
            // Tandai lokasi ini aktif di GameManager
            GameManager.Instance.SetFlag("sisik_active_" + locationPool[i], true);
            debugMsg += locationPool[i] + (i < countToSelect - 1 ? ", " : "");
        }
        Debug.Log(debugMsg);

        // Set status pencarian sisik menjadi aktif
        GameManager.Instance.SetFlag("sisik_spawning_active", true);

        // Putar dialog pemulaian quest jika ada
        if (startScatteringDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(startScatteringDialogue);
        }
        else
        {
            Debug.Log("Sisik telah disebar! Cari 7 sisik yang berkedip di Chapter 2.");
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
