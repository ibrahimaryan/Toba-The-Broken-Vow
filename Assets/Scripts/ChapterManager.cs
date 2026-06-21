using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ChapterManager : MonoBehaviour
{
    [Header("Dialogue Configuration")]
    public VNDialogueData chapterIntroData;
    public DialogueManagerCS dialogueManager;

    [Header("Intro Sequence UI")]
    [Tooltip("Panel hitam penutup layar")]
    public GameObject introPanel; 
    public CanvasGroup introCanvasGroup;
    public TextMeshProUGUI chapterNameText;
    public Image silhouetteImage;

    [Header("Intro Settings")]
    public string chapterName = "Chapter 1";
    public Sprite silhouetteSprite;
    public float waitBeforeSilhouette = 1.5f;
    public float zoomAndFadeDuration = 2.0f;
    public float zoomSpeed = 0.5f;

    [Header("Lore Text Settings (Opsional)")]
    public GameObject loreBackgroundUI;
    public TextMeshProUGUI loreTextUI;
    [TextArea(3, 5)]
    public string loreTextContent = "Dahulu, di sebuah lembah kering yang subur, hiduplah seorang pemuda yatim piatu bernama Toba. Ia menyambung hidup dengan bertani dan memancing.";
    public float waitBeforeLore = 0.5f;
    public float loreReadingDuration = 4.0f;

    [Header("Behavior Settings")]
    [Tooltip("Centang jika ingin chapter langsung diputar saat game mulai (untuk testing)")]
    public bool playOnStart = false;

    private void Start()
    {
        if (playOnStart)
        {
            TriggerChapterIntro();
        }
    }

    // Fungsi publik yang bisa dipanggil oleh MemoryShardManager saat pemain menyentuh shard
    public void TriggerChapterIntro()
    {
        if (chapterIntroData != null && dialogueManager != null)
        {
            StartCoroutine(PlayIntroSequence());
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        // 1. Setup awal: Layar Hitam + Nama Chapter Aktif
        if (introPanel != null) introPanel.SetActive(true);
        if (introCanvasGroup != null) introCanvasGroup.alpha = 1f;
        
        if (chapterNameText != null) 
        {
            chapterNameText.text = chapterName;
            chapterNameText.gameObject.SetActive(true);
        }

        if (loreTextUI != null) loreTextUI.gameObject.SetActive(false);
        if (loreBackgroundUI != null) loreBackgroundUI.SetActive(false);
        
        if (silhouetteImage != null)
        {
            if (silhouetteSprite != null) silhouetteImage.sprite = silhouetteSprite;
            silhouetteImage.gameObject.SetActive(false); // Sembunyikan dulu
            silhouetteImage.transform.localScale = Vector3.one; // Reset zoom
        }

        // 2. Tunggu sebentar (Hanya teks dan hitam)
        yield return new WaitForSeconds(waitBeforeSilhouette);

        // 3. Tampilkan gambar siluet
        if (silhouetteImage != null && silhouetteSprite != null)
        {
            silhouetteImage.gameObject.SetActive(true);
        }

        // --- Tampilkan Teks Lore (Cerita Pengantar) ---
        if (loreTextUI != null && !string.IsNullOrEmpty(loreTextContent))
        {
            if (loreBackgroundUI != null) loreBackgroundUI.SetActive(true);
            loreTextUI.gameObject.SetActive(true);
            loreTextUI.text = "";
            yield return new WaitForSeconds(waitBeforeLore);

            // Efek ngetik Teks Lore
            foreach (char c in loreTextContent)
            {
                loreTextUI.text += c;
                yield return new WaitForSeconds(0.04f); // Kecepatan ketik
            }

            // Tunggu sebentar agar pemain selesai membaca
            yield return new WaitForSeconds(loreReadingDuration);

            // Matikan teks lore sebelum animasi zoom & memudar (Fade Out)
            loreTextUI.gameObject.SetActive(false);
            if (loreBackgroundUI != null) loreBackgroundUI.SetActive(false);
        }

        // 4. Mulai Dialog Visual Novel BERSAMAAN dengan transisi Fade Out
        // Ini menciptakan efek transisi menyilang (crossfade) yang mulus tanpa jeda!
        if (dialogueManager != null && chapterIntroData != null)
        {
            dialogueManager.PlayDialogue(chapterIntroData);
        }

        // 5. Proses Animasi Zoom In dan Menghilang (Fade Out)
        float t = 0;
        while (t < zoomAndFadeDuration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / zoomAndFadeDuration; // 0 hingga 1

            // Animasi Zoom In pada Siluet
            if (silhouetteImage != null)
            {
                float scale = Mathf.Lerp(1f, 1f + zoomSpeed, normalizedTime);
                silhouetteImage.transform.localScale = new Vector3(scale, scale, 1f);
            }

            // Animasi Menghilang (Fade Out) pada seluruh Panel
            if (introCanvasGroup != null)
            {
                introCanvasGroup.alpha = Mathf.Lerp(1f, 0f, normalizedTime);
            }

            yield return null;
        }

        // 6. Selesai transisi, matikan panel intro sepenuhnya
        if (introPanel != null) introPanel.SetActive(false);
    }
}
