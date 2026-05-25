using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance; // Singleton agar mudah dipanggil dari script lain

    [Header("Screen Box UI")]
    public GameObject screenBoxPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;

    [Header("Speech Bubble UI (Dari Samosir)")]
    public GameObject bubblePanel;
    public TextMeshProUGUI bubbleText;

    // Queue digunakan untuk mengantre kalimat dialog seperti antrean kasir
    private Queue<DialogueLine> sentences; 
    private bool isTyping = false; // Mencegah teks bertumpuk saat diketik

    void Awake()
    {
        // Setup Singleton
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        sentences = new Queue<DialogueLine>();
        screenBoxPanel.SetActive(false);
        bubblePanel.SetActive(false);
    }

    public void StartDialogue(Dialogue dialogue)
    {
        sentences.Clear(); // Bersihkan antrean lama

        // Masukkan semua kalimat baru ke dalam antrean
        foreach (DialogueLine line in dialogue.lines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (isTyping) return; // Abaikan klik jika teks masih diketik

        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = sentences.Dequeue(); // Keluarkan kalimat urutan pertama dari antrean
        
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentLine));
    }

    private IEnumerator TypeSentence(DialogueLine line)
    {
        isTyping = true;
        screenBoxPanel.SetActive(false);
        bubblePanel.SetActive(false);

        TextMeshProUGUI activeTextDisplay;

        // Cek apakah kalimat ini disetting sebagai Bubble atau Screen Box
        if (line.isSpeechBubble)
        {
            bubblePanel.SetActive(true);
            activeTextDisplay = bubbleText;
        }
        else
        {
            screenBoxPanel.SetActive(true);
            nameText.text = line.characterName; // Tampilkan nama karakter di Box
            activeTextDisplay = dialogText;
        }

        activeTextDisplay.text = ""; // Kosongkan teks sebelum mulai ngetik

        // Efek Typewriter (Ketik per huruf)
        foreach (char letter in line.sentence.ToCharArray())
        {
            activeTextDisplay.text += letter;
            yield return new WaitForSeconds(0.02f); // Kecepatan ngetik, bisa diubah
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        screenBoxPanel.SetActive(false);
        bubblePanel.SetActive(false);
        Debug.Log("Percakapan Selesai.");
    }
}