using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class MemoryShardUIElement
{
    public MemoryShardData shardData;        // Seret file .asset MemoryShardData ke sini
    public Image shardImage;                 // Gambar/Thumbnail Shard di UI
    public Button playButton;                // Tombol untuk memutar cutscene
}

public class MainMenuMemoryGallery : MonoBehaviour
{
    // Flag statis untuk menandai apakah panel galeri harus langsung terbuka saat kembali ke Main Menu
    public static bool shouldOpenGalleryOnStart = false;

    [Header("Gallery Items")]
    public List<MemoryShardUIElement> galleryElements;

    [Header("Scene Loading Settings")]
    [Tooltip("Nama scene khusus untuk memutar cutscene")]
    public string cutsceneSceneName = "MemoryShardPlayerScene";

    [Header("Color Settings")]
    public Color lockedColor = Color.black;
    public Color unlockedColor = Color.white;

    void OnEnable()
    {
        // Setup galeri setiap kali panel Memory Shard dibuka
        SetupGallery();
    }

    public void SetupGallery()
    {
        foreach (var element in galleryElements)
        {
            if (element == null || element.shardData == null) continue;

            // Cek apakah shard sudah didapatkan (1 = sudah didapatkan, 0 = belum)
            bool isUnlocked = PlayerPrefs.GetInt("MemoryShard_" + element.shardData.shardID, 0) == 1;

            if (isUnlocked)
            {
                // Tampilkan gambar asli (terang)
                if (element.shardImage != null)
                {
                    element.shardImage.sprite = element.shardData.thumbnail; // Pasang thumbnail asli
                    element.shardImage.color = unlockedColor;
                }

                // Tampilkan tombol dan daftarkan listener klik
                if (element.playButton != null)
                {
                    element.playButton.gameObject.SetActive(true);
                    element.playButton.onClick.RemoveAllListeners();
                    element.playButton.onClick.AddListener(() => PlayCutscene(element.shardData));
                }
            }
            else
            {
                // Silhouette hitam untuk gambar shard (selalu pakai thumbnail agar bentuknya sama persis)
                if (element.shardImage != null)
                {
                    element.shardImage.sprite = element.shardData.thumbnail;
                    element.shardImage.color = lockedColor;
                }

                // Sembunyikan tombol
                if (element.playButton != null)
                {
                    element.playButton.gameObject.SetActive(false);
                }
            }
        }
    }

    private void PlayCutscene(MemoryShardData shard)
    {
        if (shard == null || shard.dialogueData == null)
        {
            Debug.LogError("[Gallery] Memory Shard atau Dialogue Data kosong!");
            return;
        }

        // Tandai agar panel ini langsung dibuka saat kembali ke Main Menu
        shouldOpenGalleryOnStart = true;

        // Pasang data ke static bridge agar bisa dibaca di scene khusus cutscene
        MemoryShardScenePlayer.shardToPlay = shard;

        // Muat scene pemutar cutscene
        SceneManager.LoadScene(cutsceneSceneName);
    }
}
