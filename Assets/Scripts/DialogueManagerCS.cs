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
    }

    public void PlayDialogue(VNDialogueData dialogueData)
    {
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
                if (prologueText != null) prologueText.text = currentFullText;
                
                // Jika teks prolog di-skip (muncul instan), mulai hitung mundur auto-play
                VNDialogueLine skippedLine = currentDialogue.lines[currentLineIndex - 1];
                autoAdvanceCoroutine = StartCoroutine(AutoAdvanceDelay(skippedLine));
            }
            else
            {
                if (dialogueText != null) dialogueText.text = currentFullText;
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
                if (prologuePanel != null) prologuePanel.SetActive(true);
                
                // Sembunyikan semua potret
                if (leftSlot != null) leftSlot.Clear();
                if (centerSlot != null) centerSlot.Clear();
                if (rightSlot != null) rightSlot.Clear();
            }
            else
            {
                if (prologuePanel != null) prologuePanel.SetActive(false);
                // Tampilkan panel HANYA jika teks tidak kosong atau ada karakter yang bicara
                bool showPanel = !string.IsNullOrWhiteSpace(line.text) || line.speaker != null;
                if (dialoguePanel != null) dialoguePanel.SetActive(showPanel);
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
        if (targetTextUI != null) targetTextUI.text = "";
        
        if (!string.IsNullOrEmpty(line.text) && targetTextUI != null)
        {
            foreach (char c in line.text.ToCharArray())
            {
                targetTextUI.text += c;
                yield return new WaitForSeconds(0.02f); // Typing speed
            }
        }
        isTyping = false;

        // Jika ini adalah teks Prologue, jalankan hitung mundur otomatis untuk baris berikutnya
        if (line.isPrologueCenterText)
        {
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceDelay(line));
        }
    }

    private IEnumerator AutoAdvanceDelay(VNDialogueLine line)
    {
        yield return new WaitForSeconds(prologueAutoPlayDelay);

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
        if (!isPlaying) return;
        
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
