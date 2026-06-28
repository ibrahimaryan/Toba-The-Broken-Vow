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
        if (popupPanel != null) popupPanel.SetActive(false);

        if (tontonButton != null)
            tontonButton.onClick.AddListener(OnTontonClicked);

        if (tutupButton != null)
            tutupButton.onClick.AddListener(OnTutupClicked);
    }

    public void UnlockShard(string shardID)
    {
        foreach (var shard in allShards)
        {
            if (shard == null) continue; // Mencegah error jika ada slot kosong (None) di Inspector

            if (shard.shardID == shardID)
            {
                shard.isUnlocked = true;
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
        
        // Panggil animasi Intro Chapter (siluet zoom & fade) sebelum dialog
        if (targetChapterManager != null && currentActiveShard != null)
        {
            if (!targetChapterManager.gameObject.scene.IsValid())
            {
                Debug.LogError("[MemoryShardManager] ERROR FATAL: Target Chapter Manager yang Anda masukkan di Inspector adalah PREFAB dari folder Project, BUKAN GameObject yang ada di Scene (Hierarchy)! Tolong seret ChapterManager dari Hierarchy ke kotak Target Chapter Manager!");
                return;
            }

            Debug.Log("[MemoryShardManager] Memutar Intro Chapter...");
            targetChapterManager.TriggerChapterIntro(currentActiveShard.dialogueData);
        }
        else if (currentActiveShard != null)
        {
            // Jika tidak ada ChapterManager, langsung putar dialognya
            PlayShardDialogue(currentActiveShard);
        }
    }

    private void OnTutupClicked()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
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
