using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManagerCS : MonoBehaviour
{
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;

    [Header("Prologue UI (Center Text)")]
    public GameObject prologuePanel;
    public TextMeshProUGUI prologueText;
    [Tooltip("Waktu tunggu sebelum teks prolog otomatis pindah ke baris selanjutnya")]
    public float prologueAutoPlayDelay = 2.5f;
    
    [Header("Portraits")]
    public PortraitSlot leftSlot;
    public PortraitSlot centerSlot;
    public PortraitSlot rightSlot;
    
    [Header("Background")]
    public BackgroundFader backgroundFader;

    [Header("Audio")]
    public AudioSource sfxSource;

    private VNDialogueData currentDialogue;
    private int currentLineIndex = 0;
    private bool isPlaying = false;
    public bool IsPlaying => isPlaying; // Tambahan agar bisa dicek dari luar
    private bool isTyping = false;
    private string currentFullText = "";
    private Sprite currentBgSprite;
    private Coroutine autoAdvanceCoroutine; // Coroutine untuk auto-play prolog

    // Anti-Spam / Anti Double-Click Cooldown
    private float lastClickTime = 0f;
    private float clickCooldown = 0.1f;

    private void Start()
    {
        // Pastikan semua UI mati saat game baru mulai
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        // prologuePanel.SetActive(false) dihapus agar tidak mematikan UI dari PrologueManager

        // Fallback: Cari referensi prologueText jika hilang di Inspector
        if (prologueText == null && prologuePanel != null)
        {
            prologueText = prologuePanel.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    private void EnsureUIReferences()
    {
        // Jika UI Panel hilang (karena pindah scene dan tertimpa DontDestroyOnLoad), cari ulang!
        if (dialoguePanel == null)
        {
            Debug.LogWarning("[DialogueManager] Referensi UI hilang! Memulai Auto-Recovery...");
            GameObject canvas = GameObject.Find("ScreenUiCanvas");
            if (canvas != null)
            {
                Transform panel = canvas.transform.Find("DialoguePanel");
                if (panel != null)
                {
                    dialoguePanel = panel.gameObject;
                    
                    // Cari ulang komponen teks di dalamnya
                    TextMeshProUGUI[] texts = dialoguePanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach(var t in texts)
                    {
                        if (t.name.Contains("Speaker")) speakerNameText = t;
                        if (t.name.Contains("Dialogue")) dialogueText = t;
                    }
                    Debug.Log("[DialogueManager] Auto-Recovery UI SUKSES!");
                }
            }
        }

        // Auto-Recovery untuk Prologue Panel
        if (prologuePanel == null || prologuePanel.scene.rootCount == 0) // rootCount == 0 berarti dia adalah asset Prefab, bukan instance di Scene!
        {
            GameObject canvas = GameObject.Find("ScreenUiCanvas");
            if (canvas != null)
            {
                Transform pPanel = canvas.transform.Find("ProloguePanel");
                if (pPanel != null)
                {
                    prologuePanel = pPanel.gameObject;
                    prologueText = prologuePanel.GetComponentInChildren<TextMeshProUGUI>(true);
                    Debug.Log("[DialogueManager] Auto-Recovery ProloguePanel SUKSES!");
                }
            }
        }

        // Cari ulang Background Fader jika hilang
        if (backgroundFader == null)
        {
            GameObject canvas = GameObject.Find("ScreenUiCanvas");
            if (canvas != null)
            {
                Transform bg = canvas.transform.Find("Background");
                if (bg != null) backgroundFader = bg.GetComponent<BackgroundFader>();

                // Cari ulang Portrait Slots
                if (leftSlot == null)
                {
                    Transform left = canvas.transform.Find("LeftPortraitRoot");
                    if (left == null) left = canvas.transform.Find("LeftPotraitRoot"); // Fallback typo
                    if (left != null) leftSlot = left.GetComponent<PortraitSlot>();
                }
                if (centerSlot == null)
                {
                    Transform center = canvas.transform.Find("CenterPortraitRoot");
                    if (center == null) center = canvas.transform.Find("CenterPotraitRoot"); // Fallback typo
                    if (center != null) centerSlot = center.GetComponent<PortraitSlot>();
                }
                if (rightSlot == null)
                {
                    Transform right = canvas.transform.Find("RightPortraitRoot");
                    if (right == null) right = canvas.transform.Find("RightPotraitRoot"); // Fallback typo
                    if (right != null) rightSlot = right.GetComponent<PortraitSlot>();
                }
            }
        }
    }

    public void PlayDialogue(VNDialogueData dialogueData)
    {
        if (dialogueData == null) 
        {
            Debug.LogError("[DialogueManager] Gagal memutar dialog: Data KOSONG!");
            return;
        }

        EnsureUIReferences(); // PASTIKAN UI ADA SEBELUM MULAI!

        currentDialogue = dialogueData;
        currentLineIndex = 0;
        isPlaying = true;
        currentBgSprite = null;
        
        // Clear portraits initially
        if (leftSlot != null) leftSlot.Clear();
        if (centerSlot != null) centerSlot.Clear();
        if (rightSlot != null) rightSlot.Clear();

        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        // Cegah klik ganda / double trigger dalam waktu yang sangat singkat
        if (Time.time - lastClickTime < clickCooldown) return;
        lastClickTime = Time.time;

        // Batalkan auto-play jika pemain memutuskan untuk klik secara manual
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }

        if (isTyping)
        {
            // Skip typing effect and show full text
            StopAllCoroutines();
            if (currentDialogue.lines[currentLineIndex - 1].isPrologueCenterText)
            {
                if (prologueText != null) 
                {
                    prologueText.text = currentFullText;
                    prologueText.maxVisibleCharacters = 99999;
                }
                
                // Jika teks prolog di-skip (muncul instan), mulai hitung mundur auto-play
                VNDialogueLine skippedLine = currentDialogue.lines[currentLineIndex - 1];
                autoAdvanceCoroutine = StartCoroutine(AutoAdvanceDelay(skippedLine));
            }
            else
            {
                if (dialogueText != null) 
                {
                    dialogueText.text = currentFullText;
                    dialogueText.maxVisibleCharacters = 99999;
                }
            }
            isTyping = false;
            return; 
        }

        if (currentLineIndex < currentDialogue.lines.Count)
        {
            VNDialogueLine line = currentDialogue.lines[currentLineIndex];
            
            if (line.sfxClip != null && sfxSource != null)
            {
                if (line.isPrologueCenterText)
                {
                    // Gunakan clip.Play agar bisa dideteksi isPlaying dan tidak tumpang tindih
                    sfxSource.clip = line.sfxClip;
                    sfxSource.Play();
                }
                else
                {
                    sfxSource.PlayOneShot(line.sfxClip);
                }
            }
            
            // Atur Panel mana yang muncul
            if (line.isPrologueCenterText)
            {
                if (dialoguePanel != null) dialoguePanel.SetActive(false);
                if (prologuePanel != null) 
                {
                    prologuePanel.SetActive(true);
                    prologuePanel.transform.SetAsLastSibling(); // Pastikan panel ini merender paling depan (di atas FadeOverlay)

                    // Fallback pencarian saat runtime jika null (gunakan GetComponentInChildren untuk mengabaikan salah ketik nama)
                    if (prologueText == null)
                    {
                        prologueText = prologuePanel.GetComponentInChildren<TextMeshProUGUI>(true);
                    }

                    if (prologueText != null) 
                    {
                        prologueText.gameObject.SetActive(true);
                        prologueText.color = Color.white; // Paksa warna teks putih agar tidak transparan
                    }    
                }
                
                // PAKSA MUNCUL: Memastikan prologuePanel tidak tersembunyi karena Alpha atau Scale 0
                if (prologuePanel != null)
                {
                    CanvasGroup cg = prologuePanel.GetComponent<CanvasGroup>();
                    if (cg != null) cg.alpha = 1f;
                    prologuePanel.transform.localScale = Vector3.one;
                }
                
                // Sembunyikan semua potret
                if (leftSlot != null) leftSlot.Clear();
                if (centerSlot != null) centerSlot.Clear();
                if (rightSlot != null) rightSlot.Clear();
            }
            else
            {
                if (prologuePanel != null) prologuePanel.SetActive(false);
                // Tampilkan panel HANYA jika teks tidak kosong atau ada karakter yang bicara DAN BUKAN mode isAutoPlay
                bool showPanel = (!string.IsNullOrWhiteSpace(line.text) || line.speaker != null) && !line.isAutoPlay;
                if (dialoguePanel != null) 
                {
                    dialoguePanel.SetActive(showPanel);
                    
                    // PAKSA MUNCUL: Memastikan panel tidak tersembunyi karena Alpha atau Scale 0
                    if (showPanel)
                    {
                        CanvasGroup cg = dialoguePanel.GetComponent<CanvasGroup>();
                        if (cg != null) cg.alpha = 1f;
                        dialoguePanel.transform.localScale = Vector3.one;
                        dialoguePanel.transform.SetAsLastSibling(); // Paksa pindah ke urutan paling bawah (paling depan di layar)
                        Debug.Log("[DialogueManagerCS] Panel Dialog BERHASIL dinyalakan dan dipaksa tampil di depan!");
                    }
                }
            }

            UpdateVisuals(line);
            StartCoroutine(TypeLine(line));
            currentLineIndex++;
        }
        else
        {
            EndDialogue();
        }
    }

    private void UpdateVisuals(VNDialogueLine line)
    {
        // 1. Cek apakah background berubah ke gambar yang BARU
        if (line.backgroundOverride != null && line.backgroundOverride != currentBgSprite)
        {
            // Jika berubah, bersihkan semua potret karakter agar layar segar kembali
            if (leftSlot != null) leftSlot.Clear();
            if (centerSlot != null) centerSlot.Clear();
            if (rightSlot != null) rightSlot.Clear();
            
            if (backgroundFader != null)
            {
                backgroundFader.SetBackground(line.backgroundOverride);
            }
            currentBgSprite = line.backgroundOverride;
        }

        // 2. Tampilkan Karakter
        if (line.speaker != null)
        {
            if (speakerNameText != null) speakerNameText.text = line.speaker.characterName;
            Sprite portrait = line.speaker.GetPortrait(line.emotion);
            
            // Assign to the correct slot
            switch (line.position)
            {
                case VNPortraitPosition.Left:
                    if (leftSlot != null) leftSlot.SetPortrait(portrait);
                    break;
                case VNPortraitPosition.Center:
                    if (centerSlot != null) centerSlot.SetPortrait(portrait);
                    break;
                case VNPortraitPosition.Right:
                    if (rightSlot != null) rightSlot.SetPortrait(portrait);
                    break;
            }

            // Highlight speaker, dim others
            if (leftSlot != null) leftSlot.SetDimmed(line.position != VNPortraitPosition.Left);
            if (centerSlot != null) centerSlot.SetDimmed(line.position != VNPortraitPosition.Center);
            if (rightSlot != null) rightSlot.SetDimmed(line.position != VNPortraitPosition.Right);
        }
        else
        {
            if (speakerNameText != null) speakerNameText.text = ""; // Narrator or unknown
            if (leftSlot != null) leftSlot.SetDimmed(true);
            if (centerSlot != null) centerSlot.SetDimmed(true);
            if (rightSlot != null) rightSlot.SetDimmed(true);
        }
    }

    private IEnumerator TypeLine(VNDialogueLine line)
    {
        isTyping = true;
        currentFullText = line.text;
        
        TextMeshProUGUI targetTextUI = line.isPrologueCenterText ? prologueText : dialogueText;
        if (targetTextUI != null) 
        {
            targetTextUI.text = "";
            targetTextUI.maxVisibleCharacters = 99999; // Lepas limit
            targetTextUI.ForceMeshUpdate(true);
        }

        if (!string.IsNullOrEmpty(line.text) && targetTextUI != null)
        {
            int totalChars = line.text.Length;
            for (int i = 0; i <= totalChars; i++)
            {
                // Gunakan substring untuk kompatibilitas mutlak (mengakali bug maxVisibleCharacters)
                targetTextUI.text = currentFullText.Substring(0, i);
                targetTextUI.ForceMeshUpdate(true); // PAKSA REBUILD SETIAP HURUF
                yield return new WaitForSeconds(0.02f); // Typing speed
            }
        }
        isTyping = false;

        // Jika ini adalah teks Prologue ATAU AutoPlay, jalankan hitung mundur otomatis untuk baris berikutnya
        if (line.isPrologueCenterText || line.isAutoPlay)
        {
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceDelay(line));
        }
    }

    private IEnumerator AutoAdvanceDelay(VNDialogueLine line)
    {
        float delay = line.isAutoPlay ? line.autoPlayDelay : prologueAutoPlayDelay;
        yield return new WaitForSeconds(delay);

        // Jika baris ini minta ditunggu sampai audionya selesai
        if (line.waitForAudio && sfxSource != null)
        {
            while (sfxSource.isPlaying)
            {
                yield return null;
            }
        }

        DisplayNextLine();
    }

    private void EndDialogue()
    {
        isPlaying = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (prologuePanel != null) prologuePanel.SetActive(false);
        
        // Bersihkan layar dari semua potret
        if (leftSlot != null) leftSlot.Clear();
        if (centerSlot != null) centerSlot.Clear();
        if (rightSlot != null) rightSlot.Clear();
        
        // Kembalikan background ke kondisi normal (tembus pandang ke gameplay)
        if (backgroundFader != null) backgroundFader.SetBackground(null);
        currentBgSprite = null;
    }
    
    private void Update()
    {
        // Paksa PrologueText selalu aktif jika sedang di elemen prolog
        if (isPlaying && currentDialogue != null && currentLineIndex > 0 && currentLineIndex <= currentDialogue.lines.Count)
        {
            VNDialogueLine currentLine = currentDialogue.lines[currentLineIndex - 1];
            if (currentLine.isPrologueCenterText)
            {
                if (prologueText != null && !prologueText.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning("[DialogueManagerCS] Auto-Fix: Memaksa PrologueText aktif karena terdeteksi mati!");
                    prologueText.gameObject.SetActive(true);
                    prologueText.color = Color.white;
                }

                if (prologuePanel != null)
                {
                    Transform bg = prologuePanel.transform.Find("PrologueBakcground"); // Sesuai ejaan di screenshot user
                    if (bg != null && !bg.gameObject.activeSelf) bg.gameObject.SetActive(true);
                    
                    Transform bg2 = prologuePanel.transform.Find("PrologueBackground"); // Jaga-jaga ejaan benar
                    if (bg2 != null && !bg2.gameObject.activeSelf) bg2.gameObject.SetActive(true);
                }
            }
        }

        if (isPlaying)
        {
            bool nextClicked = false;

#if ENABLE_INPUT_SYSTEM
            // Mendukung sistem Input System baru (Unity 6+)
            if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame) nextClicked = true;
            if (UnityEngine.InputSystem.Keyboard.current != null && (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)) nextClicked = true;
#else
        // Mendukung sistem Input Manager lama
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) nextClicked = true;
#endif

        if (nextClicked)
        {
            DisplayNextLine();
        }
    }
}
}
