using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; 

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance; 

    [Header("Screen Box UI (Dialog Biasa)")]
    public GameObject screenBoxPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;

    [Header("Cutscene Box UI (Otomatis)")]
    public GameObject cutsceneBoxPanel;
    public TextMeshProUGUI cutsceneNameText;
    public TextMeshProUGUI cutsceneDialogText;

    [Header("Speech Bubble UI (Otomatis)")]
    public GameObject bubblePanel;
    public TextMeshProUGUI bubbleText;

    [Header("Pengaturan Waktu (Untuk Cutscene/Bubble)")]
    [Tooltip("Berapa detik jeda sebelum dialog otomatis memuat kalimat berikutnya?")]
    public float durasiOtomatis = 3f; 

    private Queue<DialogueLine> sentences; 
    private bool isTyping = false; 

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        sentences = new Queue<DialogueLine>();
        screenBoxPanel.SetActive(false);
        bubblePanel.SetActive(false);
        
        if (cutsceneBoxPanel != null) cutsceneBoxPanel.SetActive(false);
    }

    void Update()
    {
        // PENTING: Tombol 'E' SEKARANG HANYA BERLAKU UNTUK DIALOG KECIL/BIASA.
        // Dialog cutscene atau bubble (otomatis) tidak bisa di-skip pakai E.
        if (!isTyping && screenBoxPanel.activeInHierarchy && Keyboard.current.eKey.wasPressedThisFrame)
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        // Paksa berhenti seluruh ketikan lama yang menggantung!
        StopAllCoroutines(); 
        isTyping = false; 

        sentences.Clear(); 
        foreach (DialogueLine line in dialogue.lines)
        {
            sentences.Enqueue(line);
        }
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (isTyping) return; 

        if (sentences.Count == 0)
        {
            TutupPaksaSeluruhPanel(); // Ganti rujukan tutup
            return;
        }

        DialogueLine currentLine = sentences.Dequeue(); 
        StopAllCoroutines(); // Paksa matikan TypeSentence lama lagi
        StartCoroutine(TypeSentence(currentLine));
    }

    private IEnumerator TypeSentence(DialogueLine line)
    {
        isTyping = true;
        
        // Sembunyikan semuanya sebelum menentukan yang mana yg aktif
        screenBoxPanel.SetActive(false);
        bubblePanel.SetActive(false);
        if (cutsceneBoxPanel != null) cutsceneBoxPanel.SetActive(false);

        TextMeshProUGUI activeTextDisplay;

        if (line.isSpeechBubble)
        {
            bubblePanel.SetActive(true);
            activeTextDisplay = bubbleText;
        }
        else if (line.isCutsceneStyle && cutsceneBoxPanel != null) 
        {
            cutsceneBoxPanel.SetActive(true);
            if (cutsceneNameText != null) cutsceneNameText.text = line.characterName;
            activeTextDisplay = cutsceneDialogText;
        }
        else 
        {
            screenBoxPanel.SetActive(true);
            nameText.text = line.characterName; 
            activeTextDisplay = dialogText;
        }

        activeTextDisplay.text = ""; 

        // Efek mesin tik (Typewriter) mengeja satu persatu
        foreach (char letter in line.sentence.ToCharArray())
        {
            activeTextDisplay.text += letter;
            yield return new WaitForSeconds(0.02f); 
        }

        isTyping = false;

        // FITUR OTOMATIS LANJUT SEPERTI FILM (Kini khusus untuk Bubble Saja!)
        if (line.isSpeechBubble)
        {
            // Tahan layar sejenak
            yield return new WaitForSeconds(durasiOtomatis);
            DisplayNextSentence(); 
        }
        else if (line.isCutsceneStyle)
        {
            // [DIKOSONGKAN] Tipe Cutscene akan diam menunggu ditutup oleh panjang balok Timeline!
        }
    }

    private void EndDialogue()
    {
        screenBoxPanel.SetActive(false);
        bubblePanel.SetActive(false);
        if (cutsceneBoxPanel != null) cutsceneBoxPanel.SetActive(false);
    }

    public void TutupPaksaSeluruhPanel()
    {
        StopAllCoroutines(); 
        isTyping = false;     // Matikan sisa ketikan yang nanggung
        sentences.Clear(); 
        
        // Jangan panggil EndDialogue(), kita matikan manual saja di sini:
        if(screenBoxPanel != null) screenBoxPanel.SetActive(false);
        if(bubblePanel != null) bubblePanel.SetActive(false);
        if(cutsceneBoxPanel != null) cutsceneBoxPanel.SetActive(false);
    }
}