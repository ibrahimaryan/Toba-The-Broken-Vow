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
        }
    }

    private void OnTontonClicked()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
        
        // Mainkan Cutscene / Chapter Intro
        if (targetChapterManager != null)
        {
            targetChapterManager.TriggerChapterIntro();
        }
        else
        {
            // Fallback
            ChapterManager chapter = FindFirstObjectByType<ChapterManager>();
            if (chapter != null) chapter.TriggerChapterIntro();
        }
    }

    private void OnTutupClicked()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    public void PlayShardDialogue(MemoryShardData shard)
    {
        if (shard.isUnlocked && shard.dialogueData != null && dialogueManager != null)
        {
            dialogueManager.PlayDialogue(shard.dialogueData);
        }
        else if (!shard.isUnlocked)
        {
            Debug.LogWarning("Cannot play dialogue: Memory Shard is locked.");
        }
    }
}
