using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MemoryShardManager : MonoBehaviour
{
    public static MemoryShardManager Instance { get; private set; }

    [Header("Memory Shards")]
    public List<MemoryShardData> allShards = new List<MemoryShardData>();
    
    [Header("References")]
    public DialogueManagerCS dialogueManager;
    public ChapterManager targetChapterManager;

    [Header("UI Popup Settings")]
    [Tooltip("Panel UI popup pemberitahuan Memory Shard")]
    public GameObject popupPanel;
    [Tooltip("Komponen Image untuk menampilkan gambar Memory Shard di popup")]
    public Image popupThumbnail;
    [Tooltip("Komponen Text (TMP) untuk menampilkan judul Memory Shard di popup")]
    public TextMeshProUGUI popupTitle;
    public Button tontonButton;
    public Button tutupButton;

    private MemoryShardData currentActiveShard; // Menyimpan memori shard mana yang sedang aktif di popup

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Load status memory shard yang tersimpan
        LoadShardStatus();

        if (popupPanel != null) popupPanel.SetActive(false);

        if (tontonButton != null)
            tontonButton.onClick.AddListener(OnTontonClicked);

        if (tutupButton != null)
            tutupButton.onClick.AddListener(OnTutupClicked);
    }

    public void LoadShardStatus()
    {
        foreach (var shard in allShards)
        {
            if (shard == null) continue;
            int status = PlayerPrefs.GetInt("MemoryShard_" + shard.shardID, 0);
            shard.isUnlocked = (status == 1);
        }
    }

    public void UnlockShard(string shardID)
    {
        foreach (var shard in allShards)
        {
            if (shard == null) continue; // Mencegah error jika ada slot kosong (None) di Inspector

            if (shard.shardID == shardID)
            {
                shard.isUnlocked = true;
                PlayerPrefs.SetInt("MemoryShard_" + shardID, 1);
                PlayerPrefs.Save();
                currentActiveShard = shard; // Simpan memori shard yang baru didapat
                Debug.Log($"Memory Shard Unlocked: {shard.title}");
                ShowShardPopup(); // Tampilkan popup
                break;
            }
        }
    }

    public void ShowShardPopup()
    {
        if (popupPanel != null)
        {
            // Update UI secara dinamis! 1 Panel untuk semua Shard!
            if (currentActiveShard != null)
            {
                if (popupThumbnail != null && currentActiveShard.thumbnail != null) 
                    popupThumbnail.sprite = currentActiveShard.thumbnail;
                
                if (popupTitle != null) 
                    popupTitle.text = currentActiveShard.title;
            }

            popupPanel.SetActive(true);
            Debug.Log($"[MEMORY SHARD] Popup Panel '{popupPanel.name}' berstatus: {popupPanel.activeInHierarchy}. Posisi di Hierarchy: di bawah '{popupPanel.transform.parent.name}'. Apakah terhalang UI lain?");
        }
        else
        {
            Debug.LogError("[MEMORY SHARD ERROR] Popup Panel belum dimasukkan ke dalam Inspector MemoryShardManager!");
        }
    }

    private void OnTontonClicked()
    {
        Debug.Log("[MemoryShardManager] Tombol 'Tonton' diklik!");
        if (popupPanel != null) popupPanel.SetActive(false);

        // Jika Chapter 5, teleport player saat popup Memory Shard ditutup
        if (currentActiveShard != null && currentActiveShard.shardID == "Chapter5")
        {
            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null)
            {
                player.transform.position = new Vector3(-3f, 78f, player.transform.position.z);
            }
        }

        // Jika kotak Target Chapter Manager lupa diisi di Inspector, cari otomatis di Scene
        if (targetChapterManager == null)
        {
            targetChapterManager = FindFirstObjectByType<ChapterManager>();
            if (targetChapterManager != null)
            {
                Debug.Log("[MemoryShardManager] Menemukan ChapterManager secara otomatis di Scene!");
            }
        }
        
        // Panggil animasi Intro Chapter (siluet zoom & fade) sebelum dialog
        if (targetChapterManager != null && currentActiveShard != null)
        {
            if (!targetChapterManager.gameObject.scene.IsValid())
            {
                Debug.LogError("[MemoryShardManager] ERROR FATAL: Target Chapter Manager yang Anda masukkan di Inspector adalah PREFAB dari folder Project, BUKAN GameObject yang ada di Scene (Hierarchy)! Tolong seret ChapterManager dari Hierarchy ke kotak Target Chapter Manager!");
                return;
            }

            Debug.Log("[MemoryShardManager] Memutar Intro Chapter...");
            targetChapterManager.TriggerChapterIntro(currentActiveShard);
            StartCoroutine(WaitVNDialogueAndUnlockPlayer());
        }
        else if (currentActiveShard != null)
        {
            Debug.LogWarning("[MemoryShardManager] Tidak ada ChapterManager di Scene! Memutar dialog secara langsung tanpa animasi Intro.");
            // Jika tidak ada ChapterManager, langsung putar dialognya
            PlayShardDialogue(currentActiveShard);
            StartCoroutine(WaitVNDialogueAndUnlockPlayer());
        }
    }

    private void OnTutupClicked()
    {
        if (popupPanel != null) popupPanel.SetActive(false);

        // Jika Chapter 5, teleport player saat popup Memory Shard ditutup
        if (currentActiveShard != null && currentActiveShard.shardID == "Chapter5")
        {
            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null)
            {
                player.transform.position = new Vector3(-3f, 78f, player.transform.position.z);
            }
        }

        // Kembalikan input player
        var activePlayer = FindAnyObjectByType<PlayerControllerScript>();
        if (activePlayer != null) activePlayer.ToggleInput(true);
    }

    private System.Collections.IEnumerator WaitVNDialogueAndUnlockPlayer()
    {
        yield return null; // Tunggu satu frame agar dialogManager sempat disetup/mulai

        if (dialogueManager != null)
        {
            while (dialogueManager.IsPlaying)
            {
                yield return null;
            }
        }

        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null) player.ToggleInput(true);
    }

    public void PlayShardDialogue(MemoryShardData shard)
    {
        if (!shard.isUnlocked)
        {
            Debug.LogWarning("[MemoryShardManager] Cannot play dialogue: Memory Shard is locked.");
            return;
        }

        if (shard.dialogueData == null)
        {
            Debug.LogError($"[MemoryShardManager] GAGAL: File 'Dialogue Data' pada Memory Shard '{shard.name}' KOSONG! Masukkan data VN ke dalamnya.");
            return;
        }

        if (dialogueManager == null)
        {
            Debug.LogError("[MemoryShardManager] GAGAL: Kolom 'Dialogue Manager' di Inspector MemoryShardManager KOSONG! Masukkan objek DialogueManager dari Hierarchy.");
            return;
        }

        // Jika semua aman, putar dialognya
        dialogueManager.PlayDialogue(shard.dialogueData);
    }
}
