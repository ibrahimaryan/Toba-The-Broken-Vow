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
        Debug.Log($"[ChapterManager] START PlayIntroSequence! waitBeforeSilhouette={waitBeforeSilhouette}, chapterName={chapterName}");

        // Pengecekan krusial: Apakah introPanel adalah Prefab dari folder?
        if (introPanel != null && !introPanel.scene.IsValid())
        {
            Debug.LogError("[ChapterManager] ERROR FATAL: Intro Panel yang dimasukkan di Inspector adalah PREFAB dari folder Project! Tolong seret IntroPanel dari Hierarchy (ScreenUiCanvas) ke kotak Intro Panel.");
            yield break; // Hentikan animasi agar tidak error
        }

        // AUTO-RECOVERY
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
                    if (introCanvasGroup == null) 
                    {
                        introCanvasGroup = introPanel.AddComponent<CanvasGroup>();
                    }

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
                    
                    if (chapterNameText == null && texts.Length > 0) chapterNameText = texts[0];

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
            }
        }

        if (waitBeforeSilhouette <= 0f) waitBeforeSilhouette = 1.5f;
        if (loreReadingDuration <= 0f) loreReadingDuration = 4.0f;
        if (zoomAndFadeDuration <= 0f) zoomAndFadeDuration = 2.0f;

        if (dialogueManager == null || !dialogueManager.gameObject.activeInHierarchy || dialogueManager.dialoguePanel == null)
        {
            DialogueManagerCS[] allManagers = FindObjectsByType<DialogueManagerCS>(FindObjectsSortMode.None);
            foreach (var mgr in allManagers)
            {
                if (mgr.gameObject.activeInHierarchy && mgr.dialoguePanel != null)
                {
                    dialogueManager = mgr;
                    Debug.Log("[ChapterManager] Auto-Recover DialogueManagerCS yang ASLI (Punya Panel)!");
                    break;
                }
            }
        }

        Debug.Log($"[ChapterManager] LANGKAH 1: Set layar hitam. introPanel={(introPanel!=null?"Ada":"NULL")}, introCanvasGroup={(introCanvasGroup!=null?"Ada":"NULL")}");
        // 1. Setup awal: Layar Hitam
        if (introPanel != null) 
        {
            introPanel.SetActive(true);
            introPanel.transform.SetAsLastSibling(); // Paksa ke depan
        }
        if (introCanvasGroup != null) introCanvasGroup.alpha = 1f;
        
        if (loreTextUI != null) loreTextUI.gameObject.SetActive(false);
        if (loreBackgroundUI != null) loreBackgroundUI.SetActive(false);
        
        if (silhouetteImage != null)
        {
            silhouetteImage.gameObject.SetActive(false); // Sembunyikan dulu
            silhouetteImage.transform.localScale = Vector3.one; // Reset scale
            if (silhouetteSprite != null) silhouetteImage.sprite = silhouetteSprite;
        }

        Debug.Log($"[ChapterManager] LANGKAH 2: Munculkan Judul Chapter. chapterNameText={(chapterNameText!=null?"Ada":"NULL")}");
        // 2. Munculkan Teks Judul Chapter
        if (chapterNameText != null) 
        {
            chapterNameText.text = chapterName;
            chapterNameText.gameObject.SetActive(true);
            Debug.Log($"[ChapterManager] Menunggu {waitBeforeSilhouette} detik untuk judul chapter...");
            yield return new WaitForSeconds(waitBeforeSilhouette); 
            chapterNameText.gameObject.SetActive(false); 
        }

        Debug.Log($"[ChapterManager] LANGKAH 3: Munculkan Siluet & Lore. silhouetteImage={(silhouetteImage!=null?"Ada":"NULL")}, loreTextUI={(loreTextUI!=null?"Ada":"NULL")}");
        // 3. Tampilkan Siluet & Lore Text BERSAMAAN
        if (silhouetteImage != null && silhouetteSprite != null)
        {
            silhouetteImage.gameObject.SetActive(true);
        }

        if (loreTextUI != null && !string.IsNullOrEmpty(loreTextContent))
        {
            if (loreBackgroundUI != null) loreBackgroundUI.SetActive(true);
            loreTextUI.text = loreTextContent;
            loreTextUI.gameObject.SetActive(true);

            Debug.Log($"[ChapterManager] Menunggu {loreReadingDuration} detik untuk membaca lore...");
            yield return new WaitForSeconds(loreReadingDuration);

            loreTextUI.gameObject.SetActive(false);
            if (loreBackgroundUI != null) loreBackgroundUI.SetActive(false);
        }

        Debug.Log($"[ChapterManager] LANGKAH 4: Memulai VN Dialog. dialogueManager={(dialogueManager!=null?"Ada":"NULL")}");
        // 4. Mulai Dialog Visual Novel
        if (dialogueManager != null && dataToPlay != null)
        {
            dialogueManager.PlayDialogue(dataToPlay);
        }
        else
        {
            Debug.LogError($"[ChapterManager] GAGAL MULAI VN! dialogueManager={(dialogueManager!=null?"Ada":"NULL")}, dataToPlay={(dataToPlay!=null?"Ada":"NULL")}");
        }

        Debug.Log("[ChapterManager] LANGKAH 5: Animasi Zoom & Fade Out.");
        // 5. Proses Animasi Zoom In dan Menghilang (Fade Out)
        float t = 0;
        while (t < zoomAndFadeDuration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / zoomAndFadeDuration; // 0 hingga 1

            if (silhouetteImage != null)
            {
                float scale = Mathf.Lerp(1f, 1f + zoomSpeed, normalizedTime);
                silhouetteImage.transform.localScale = new Vector3(scale, scale, 1f);
            }

            if (introCanvasGroup != null)
            {
                introCanvasGroup.alpha = Mathf.Lerp(1f, 0f, normalizedTime);
            }

            yield return null;
        }

        Debug.Log("[ChapterManager] SELESAI.");
        if (introPanel != null) introPanel.SetActive(false);
        if (silhouetteImage != null) silhouetteImage.gameObject.SetActive(false);
    }
}
