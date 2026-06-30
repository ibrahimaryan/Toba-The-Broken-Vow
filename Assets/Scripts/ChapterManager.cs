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

    public void TriggerChapterIntro(VNDialogueData customData = null)
    {
        Debug.Log($"[ChapterManager] TriggerChapterIntro dipanggil! Apakah GameObject ini aktif? {gameObject.activeInHierarchy}");
        
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError("[ChapterManager] ERROR FATAL: ChapterManager berada di GameObject yang sedang MATI (Inactive)! StartCoroutine TIDAK AKAN JALAN! Pindahkan komponen ChapterManager ke GameObject yang selalu hidup (misal: GameManager).");
            return;
        }

        VNDialogueData dataToPlay = customData != null ? customData : chapterIntroData;
        
        if (dataToPlay != null && dialogueManager != null)
        {
            StartCoroutine(PlayIntroSequence(dataToPlay));
        }
        else
        {
            if (dataToPlay == null) Debug.LogError("[ChapterManager] GAGAL: Data Dialog KOSONG! (Tidak ada VN Dialogue Data yang dimasukkan)");
            if (dialogueManager == null) Debug.LogError("[ChapterManager] GAGAL: Kolom 'Dialogue Manager' di Inspector ChapterManager KOSONG!");
        }
    }

    private IEnumerator PlayIntroSequence(VNDialogueData dataToPlay)
    {
        Debug.Log($"[ChapterManager] Memeriksa isi Inspector... introPanel = {introPanel != null}, introCanvasGroup = {introCanvasGroup != null}, dialogueManager = {dialogueManager != null}");

        // Pengecekan krusial: Apakah introPanel adalah Prefab dari folder?
        if (introPanel != null && !introPanel.scene.IsValid())
        {
            Debug.LogError("[ChapterManager] ERROR FATAL: Intro Panel yang dimasukkan di Inspector adalah PREFAB dari folder Project! Tolong seret IntroPanel dari Hierarchy (ScreenUiCanvas) ke kotak Intro Panel.");
            yield break; // Hentikan animasi agar tidak error
        }

        // AUTO-RECOVERY: Jika referensi hilang (karena DontDestroyOnLoad), kita cari ulang otomatis!
        if (introPanel == null)
        {
            Debug.LogWarning("[ChapterManager] introPanel terputus! Memulai pemulihan otomatis dari ScreenUiCanvas...");
            GameObject canvasObj = GameObject.Find("ScreenUiCanvas");
            if (canvasObj != null)
            {
                Transform introTransform = canvasObj.transform.Find("IntroPanel");
                if (introTransform != null)
                {
                    introPanel = introTransform.gameObject;
                    introCanvasGroup = introPanel.GetComponent<CanvasGroup>();

                    // Cari text dan gambar berdasarkan nama
                    TextMeshProUGUI[] texts = introPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var txt in texts) 
                    {
                        if (txt.name.ToLower().Contains("chapter") && chapterNameText == null) chapterNameText = txt;
                        if (txt.name.ToLower().Contains("lore") && loreTextUI == null) 
                        {
                            loreTextUI = txt;
                            loreBackgroundUI = txt.transform.parent.gameObject;
                        }
                    }

                    Image[] images = introPanel.GetComponentsInChildren<Image>(true);
                    foreach (var img in images) 
                    {
                        if ((img.name.ToLower().Contains("siluet") || img.name.ToLower().Contains("silhouette")) && silhouetteImage == null) 
                        {
                            silhouetteImage = img;
                        }
                    }
                    Debug.Log("[ChapterManager] Pemulihan SUKSES! IntroPanel kembali terhubung.");
                }
                else
                {
                    Debug.LogError("[ChapterManager] Pemulihan GAGAL: IntroPanel tidak ditemukan di dalam ScreenUiCanvas.");
                    yield break;
                }
            }
            else
            {
                Debug.LogError("[ChapterManager] Pemulihan GAGAL: ScreenUiCanvas tidak ditemukan di Scene.");
                yield break;
            }
        }

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
            silhouetteImage.gameObject.SetActive(false); // Sembunyikan dulu
            silhouetteImage.transform.localScale = Vector3.one; // Reset scale
            if (silhouetteSprite != null) silhouetteImage.sprite = silhouetteSprite;
        }

        // 2. Beri waktu agar Nama Chapter tampil sendirian (sekitar 1 detik)
        yield return new WaitForSeconds(1.0f);

        // 3. Tampilkan Siluet SEBELUM Lore Text
        if (silhouetteImage != null && silhouetteSprite != null)
        {
            silhouetteImage.gameObject.SetActive(true);
            // Jeda sebentar sebelum lore text muncul (bisa diatur di Inspector)
            yield return new WaitForSeconds(waitBeforeSilhouette);
        }

        // 4. Tampilkan teks lore (jika ada) SAAT siluet sudah tampil di layar
        if (loreTextUI != null && !string.IsNullOrEmpty(loreTextContent))
        {
            yield return new WaitForSeconds(waitBeforeLore);
            
            if (loreBackgroundUI != null) loreBackgroundUI.SetActive(true);
            loreTextUI.text = "";
            loreTextUI.gameObject.SetActive(true);

            // Efek Typewriter (mengetik satu per satu)
            foreach (char c in loreTextContent)
            {
                loreTextUI.text += c;
                yield return new WaitForSeconds(0.04f); // Kecepatan ketik (bisa diubah)
            }

            // Tunggu sebentar agar pemain selesai membaca
            yield return new WaitForSeconds(loreReadingDuration);

            // Matikan teks lore sebelum animasi zoom & memudar (Fade Out)
            loreTextUI.gameObject.SetActive(false);
            if (loreBackgroundUI != null) loreBackgroundUI.SetActive(false);
        }

        // 4. Mulai Dialog Visual Novel BERSAMAAN dengan transisi Fade Out
        // Ini menciptakan efek transisi menyilang (crossfade) yang mulus tanpa jeda!
        if (dialogueManager != null && dataToPlay != null)
        {
            dialogueManager.PlayDialogue(dataToPlay);
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
