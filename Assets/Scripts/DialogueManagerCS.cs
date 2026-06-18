using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManagerCS : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;
    
    [Header("Portraits")]
    public PortraitSlot leftSlot;
    public PortraitSlot centerSlot;
    public PortraitSlot rightSlot;
    
    [Header("Background")]
    public BackgroundFader backgroundFader;

    private VNDialogueData currentDialogue;
    private int currentLineIndex = 0;
    private bool isPlaying = false;
    private bool isTyping = false;
    private string currentFullText = "";
    private Sprite currentBgSprite;

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
        if (isTyping)
        {
            // Skip typing effect and show full text
            StopAllCoroutines();
            dialogueText.text = currentFullText;
            isTyping = false;
            return; 
        }

        if (currentLineIndex < currentDialogue.lines.Count)
        {
            VNDialogueLine line = currentDialogue.lines[currentLineIndex];
            
            // Tampilkan panel HANYA jika teks tidak kosong atau ada karakter yang bicara
            bool showPanel = !string.IsNullOrWhiteSpace(line.text) || line.speaker != null;
            if (dialoguePanel != null) dialoguePanel.SetActive(showPanel);

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
        dialogueText.text = "";
        
        if (!string.IsNullOrEmpty(line.text))
        {
            foreach (char c in line.text.ToCharArray())
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(0.02f); // Typing speed
            }
        }
        isTyping = false;
    }

    private void EndDialogue()
    {
        isPlaying = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        
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
