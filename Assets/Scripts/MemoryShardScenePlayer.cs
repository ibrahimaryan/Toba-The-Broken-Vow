using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MemoryShardScenePlayer : MonoBehaviour
{
    // Static bridge untuk mengoper data dari Main Menu ke scene ini
    public static MemoryShardData shardToPlay;

    [Header("References")]
    [Tooltip("Seret GameObject ChapterManager yang ada di scene ini")]
    public ChapterManager chapterManager;
    [Tooltip("Seret GameObject DialogueManagerCS yang ada di scene ini")]
    public DialogueManagerCS dialogueManager;

    void Start()
    {
        // Pastikan waktu berjalan normal agar WaitForSeconds tidak membeku
        Time.timeScale = 1f;

        if (shardToPlay == null)
        {
            Debug.LogError("[MemoryShardScenePlayer] Data Shard kosong! Kembali ke Main Menu.");
            ReturnToMainMenu();
            return;
        }

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // Hubungkan otomatis jika kosong di Inspector
        if (chapterManager == null)
        {
            chapterManager = FindFirstObjectByType<ChapterManager>();
        }

        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueManagerCS>();
        }

        // 1. Jalankan intro chapter jika ada ChapterManager
        if (chapterManager != null)
        {
            Debug.Log("[MemoryShardScenePlayer] Memutar Intro dan Cutscene via ChapterManager...");
            chapterManager.TriggerChapterIntro(shardToPlay);
        }
        else if (dialogueManager != null && shardToPlay.dialogueData != null)
        {
            Debug.Log("[MemoryShardScenePlayer] Memutar Cutscene langsung via DialogueManagerCS...");
            dialogueManager.PlayDialogue(shardToPlay.dialogueData);
        }
        else
        {
            Debug.LogError("[MemoryShardScenePlayer] Tidak ada ChapterManager maupun DialogueManagerCS di scene ini!");
            ReturnToMainMenu();
            yield break;
        }

        // Tunggu satu frame agar inisialisasi awal aman
        yield return null;

        // 2. Cari ulang DialogueManagerCS jika tadi belum terhubung
        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueManagerCS>();
        }

        // 3. Tunggu sampai Visual Novel mulai diputar (IsPlaying menjadi true)
        if (dialogueManager != null)
        {
            // Karena ada intro hitam + dongeng mengetik terlebih dahulu sebelum VN dimulai,
            // kita beri toleransi waktu tunggu (timeout) agar sistem tidak langsung menganggap dialog sudah selesai.
            float maxWaitTime = 40f; 
            while (!dialogueManager.IsPlaying && maxWaitTime > 0f)
            {
                maxWaitTime -= Time.unscaledDeltaTime;
                yield return null;
            }

            Debug.Log("[MemoryShardScenePlayer] Visual Novel terdeteksi mulai berputar. Sekarang menunggu hingga selesai...");

            // 4. Setelah VN mulai diputar, tunggu sampai VN selesai sepenuhnya (IsPlaying menjadi false)
            while (dialogueManager.IsPlaying)
            {
                yield return null;
            }
        }

        Debug.Log("[MemoryShardScenePlayer] Cutscene selesai. Kembali ke Main Menu.");

        // 5. Kembali ke Main Menu
        ReturnToMainMenu();
    }

    private void ReturnToMainMenu()
    {
        shardToPlay = null; // Bersihkan data bridge
        SceneManager.LoadScene("MainMenu");
    }
}
