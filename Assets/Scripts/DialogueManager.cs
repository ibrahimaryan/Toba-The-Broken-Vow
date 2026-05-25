using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // Tambahkan ini untuk membaca tombol keyboard

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance; 

    [Header("Screen Box UI")]
    public GameObject screenBoxPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;

    [Header("Speech Bubble UI")]
    public GameObject bubblePanel;
    public TextMeshProUGUI bubbleText;

    [Header("Pengaturan Waktu")]
    [Tooltip("Berapa lama bubble bertahan setelah teks selesai diketik?")]
    public float durasiBubble = 3f; // Kamu bisa ubah nilainya di Inspector!

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
    }

    void Update()
    {
        // Fitur menekan tombol 'E' untuk melanjutkan dialog di Kotak Bawah (Screen Box)
        // Dialog hanya bisa dilanjut jika teks sudah selesai diketik dan kotak sedang aktif
        if (!isTyping && screenBoxPanel.activeInHierarchy && Keyboard.current.eKey.wasPressedThisFrame)
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
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
            EndDialogue();
            return;
        }

        DialogueLine currentLine = sentences.Dequeue(); 
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentLine));
    }

    private IEnumerator TypeSentence(DialogueLine line)
    {
        isTyping = true;
        screenBoxPanel.SetActive(false);
        bubblePanel.SetActive(false);

        TextMeshProUGUI activeTextDisplay;

        if (line.isSpeechBubble)
        {
            bubblePanel.SetActive(true);
            activeTextDisplay = bubbleText;
        }
        else
        {
            screenBoxPanel.SetActive(true);
            nameText.text = line.characterName; 
            activeTextDisplay = dialogText;
        }

        activeTextDisplay.text = ""; 

        // Efek mesin tik
        foreach (char letter in line.sentence.ToCharArray())
        {
            activeTextDisplay.text += letter;
            yield return new WaitForSeconds(0.02f); 
        }

        isTyping = false;

        // FITUR BARU: Auto-Hilang / Auto-Lanjut khusus untuk Speech Bubble
        if (line.isSpeechBubble)
        {
            // Tunggu beberapa detik sesuai pengaturan, lalu otomatis lanjut/tutup
            yield return new WaitForSeconds(durasiBubble);
            DisplayNextSentence(); 
        }
    }

    private void EndDialogue()
    {
        screenBoxPanel.SetActive(false);
        bubblePanel.SetActive(false);
    }
}