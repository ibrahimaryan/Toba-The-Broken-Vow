using UnityEngine;
using System.Collections.Generic;

public class BarangTrigger : MonoBehaviour
{
    [Header("Puzzle Panel UI")]
    [SerializeField] private GameObject puzzlePanel; 

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

    private bool isPlayerInRange = false;

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

        // Cek apakah quest mengumpulkan sisik sudah dimulai
        bool isSpawningActive = GameManager.Instance.IsFlagSet("sisik_spawning_active");

        if (!isSpawningActive)
        {
            // 1. Acak & Tebarkan Sisik
            StartScattering();
        }
        else
        {
            // 2. Cek jumlah sisik di Inventory
            int collectedCount = 0;
            if (InventoryManager.Instance != null)
            {
                collectedCount = InventoryManager.Instance.GetItemCount("sisik");
            }

            if (collectedCount < 7)
            {
                // Tampilkan pesan bahwa sisik belum lengkap
                if (notEnoughScalesDialogue != null && DialogueManager.instance != null)
                {
                    // Modifikasi kalimat dialog dinamis jika perlu, atau tampilkan dialog bawaan
                    DialogueManager.instance.StartDialogue(notEnoughScalesDialogue);
                }
                else
                {
                    Debug.Log($"Sisik baru terkumpul: {collectedCount}/7. Cari sisanya!");
                }
            }
            else
            {
                // 3. Sisik lengkap! Buka panel penyusunan Drag & Drop
                OpenPuzzlePanel();
            }
        }
    }

    private void StartScattering()
    {
        Debug.Log("Memulai penyebaran 7 sisik secara acak...");

        // Acak dan pilih 7 lokasi unik dari database lokasi
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
        for (int i = 0; i < countToSelect; i++)
        {
            selectedLocations.Add(locationPool[i]);
            // Tandai lokasi ini aktif di GameManager
            GameManager.Instance.SetFlag("sisik_active_" + locationPool[i], true);
        }

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

    private void OpenPuzzlePanel()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
            Debug.Log("Membuka Puzzle Panel Drag & Drop!");
            
            // Matikan kontrol player saat menyusun puzzle
            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null) player.ToggleInput(false);
        }
        else
        {
            Debug.LogError("Slot 'Puzzle Panel' di Inspector masih kosong!");
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
