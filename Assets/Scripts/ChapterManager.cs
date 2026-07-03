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

    public void TriggerChapterIntro(MemoryShardData shardData = null)
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError("[ChapterManager] ERROR FATAL: ChapterManager berada di GameObject yang sedang MATI!");
            return;
        }

        // Terapkan data intro dari Shard (jika ada) sehingga 1 scene bisa memutar banyak chapter!
        if (shardData != null)
        {
            if (!string.IsNullOrEmpty(shardData.chapterName)) chapterName = shardData.chapterName;
            if (shardData.silhouetteSprite != null) silhouetteSprite = shardData.silhouetteSprite;
            if (!string.IsNullOrEmpty(shardData.loreTextContent)) loreTextContent = shardData.loreTextContent;
        }

        VNDialogueData dataToPlay = shardData != null ? shardData.dialogueData : chapterIntroData;
        
        if (dataToPlay != null && dialogueManager != null)
        {
            StartCoroutine(PlayIntroSequence(dataToPlay));
        }
        else
        {
            if (dataToPlay == null) Debug.LogError("[ChapterManager] GAGAL: Data Dialog KOSONG!");
            if (dialogueManager == null) Debug.LogError("[ChapterManager] GAGAL: Kolom 'Dialogue Manager' di Inspector ChapterManager KOSONG!");
        }
    }

    private IEnumerator PlayIntroSequence(VNDialogueData dataToPlay)
    {
        Debug.Log($"[ChapterManager] Memeriksa isi Inspector... introPanel = {introPanel != null}, introCanvasGroup = {introCanvasGroup != null}, dialogueManager = {dialogueManager != null}");

        // Pengecekan krusial dibuang. Kita akan selalu MENGAMBIL ALIH paksa dari Scene!
        Debug.LogWarning("[ChapterManager] Membuang referensi Inspector dan memaksa pencarian IntroPanel dari ScreenUiCanvas yang aktif...");
        
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
                chapterNameText = null;
                loreTextUI = null;
                foreach (var txt in texts) 
                {
                    if (txt.name.ToLower().Contains("chapter") && chapterNameText == null) chapterNameText = txt;
                    if (txt.name.ToLower().Contains("lore") && loreTextUI == null) 
                    {
                        loreTextUI = txt;
                    }
                }

                // Cari LoreBackground secara spesifik dari semua child IntroPanel (walaupun dia sibling dari LoreText)
                Transform[] allChildren = introPanel.GetComponentsInChildren<Transform>(true);
                foreach (var child in allChildren)
                {
                    if (child.name.ToLower().Contains("lorebackground") && loreBackgroundUI == null)
                    {
                        loreBackgroundUI = child.gameObject;
                    }
                }

                // Fallback pencarian Chapter Text jika namanya tidak mengandung "chapter"
                if (chapterNameText == null && texts.Length > 0)
                {
                    foreach (var txt in texts)
                    {
                        if (txt != loreTextUI)
                        {
                            chapterNameText = txt;
                            break;
                        }
                    }
                }

                Image[] images = introPanel.GetComponentsInChildren<Image>(true);
                silhouetteImage = null;
                foreach (var img in images) 
                {
                    if ((img.name.ToLower().Contains("siluet") || img.name.ToLower().Contains("silhouette")) && silhouetteImage == null) 
                    {
                        silhouetteImage = img;
                    }
                }

                Debug.Log($"[ChapterManager] Pengambilalihan SUKSES! ChapterText={(chapterNameText != null)}, LoreText={(loreTextUI != null)}, Siluet={(silhouetteImage != null)}");
            }
            else
            {
                Debug.LogError("[ChapterManager] GAGAL: IntroPanel tidak ditemukan di dalam ScreenUiCanvas.");
                yield break;
            }
        }
        else
        {
            Debug.LogError("[ChapterManager] GAGAL: ScreenUiCanvas tidak ditemukan di Scene.");
            yield break;
        }

        // AUTO-RECOVERY untuk DialogueManager
        if (dialogueManager == null)
        {
            dialogueManager = Object.FindAnyObjectByType<DialogueManagerCS>();
            if (dialogueManager != null) Debug.Log("[ChapterManager] DialogueManager berhasil dipulihkan secara otomatis!");
            else Debug.LogError("[ChapterManager] FATAL: DialogueManagerCS tidak ditemukan di Scene!");
        }

        // 1. Setup awal: Layar Hitam + Nama Chapter Aktif
        if (introPanel != null) 
        {
            // PENGAMANAN EKSTRA: Paksa IntroPanel menjadi Hitam Pekat dan memiliki CanvasGroup
            Image bgImage = introPanel.GetComponent<Image>();
            if (bgImage == null) bgImage = introPanel.AddComponent<Image>();
            bgImage.sprite = null;
            bgImage.color = Color.black;
            
            if (introCanvasGroup == null) introCanvasGroup = introPanel.GetComponent<CanvasGroup>();
            if (introCanvasGroup == null) introCanvasGroup = introPanel.AddComponent<CanvasGroup>();
            
            introPanel.SetActive(true);
            
            // Lacak asal-usul IntroPanel ini dan PAKSA SEMUA PARENT HIDUP!
            string path = introPanel.name;
            Transform parentTransform = introPanel.transform.parent;
            while (parentTransform != null) 
            { 
                path = parentTransform.name + "/" + path; 
                
                // Jika parent mati, hidupkan paksa!
                if (!parentTransform.gameObject.activeSelf)
                {
                    Debug.Log($"[ChapterManager] MENGHIDUPKAN PAKSA PARENT YANG MATI: {parentTransform.name}");
                    parentTransform.gameObject.SetActive(true);
                }
                
                parentTransform = parentTransform.parent; 
            }
            Debug.Log($"[ChapterManager] IntroPanel diaktifkan dan dipaksa menjadi Hitam Pekat! Path objek: {path}");
        }
        if (introCanvasGroup != null) introCanvasGroup.alpha = 1f;
        
        Debug.Log($"[ChapterManager] Menyiapkan Teks Chapter. string chapterName = '{chapterName}'");
        if (chapterNameText != null) 
        {
            chapterNameText.text = chapterName;
            chapterNameText.gameObject.SetActive(true);
            Debug.Log("[ChapterManager] Teks Chapter Dinyalakan!");
        }

        if (loreTextUI != null) loreTextUI.gameObject.SetActive(false);
        if (loreBackgroundUI != null) loreBackgroundUI.SetActive(false);
        if (silhouetteImage != null)
        {
            silhouetteImage.gameObject.SetActive(false); // Sembunyikan dulu
            silhouetteImage.transform.localScale = Vector3.one; // Reset scale
            if (silhouetteSprite != null) silhouetteImage.sprite = silhouetteSprite;
        }

        // CEK STATUS SEBELUM WAIT: Apakah masih aktif?!
        Debug.Log($"[ChapterManager] SEBELUM JEDA 3 DETIK: introPanel.activeInHierarchy = {introPanel.activeInHierarchy}, CanvasGroup.alpha = {introCanvasGroup.alpha}");

        // 2. Beri waktu agar Nama Chapter tampil sendirian
        yield return new WaitForSeconds(waitBeforeSilhouette);
        
        // CEK STATUS SETELAH WAIT: Apakah berubah?!
        Debug.Log($"[ChapterManager] SETELAH JEDA 3 DETIK: introPanel.activeInHierarchy = {introPanel.activeInHierarchy}, CanvasGroup.alpha = {introCanvasGroup.alpha}");
        Debug.Log("[ChapterManager] Jeda selesai, memunculkan siluet!");

        // 3. Tampilkan Siluet, Background Dongeng, dan Teks Dongeng BERSAMAAN
        if (chapterNameText != null) 
        {
            chapterNameText.gameObject.SetActive(false); // Sembunyikan teks chapter saat siluet muncul
        }

        if (silhouetteImage != null && silhouetteSprite != null)
        {
            silhouetteImage.gameObject.SetActive(true);
        }

        if (loreTextUI != null && !string.IsNullOrEmpty(loreTextContent))
        {
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
