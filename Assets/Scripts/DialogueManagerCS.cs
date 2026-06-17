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

    public void PlayDialogue(VNDialogueData dialogueData)
    {
        currentDialogue = dialogueData;
        currentLineIndex = 0;
        isPlaying = true;
        dialoguePanel.SetActive(true);
        
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

        if (line.backgroundOverride != null && backgroundFader != null)
        {
            backgroundFader.SetBackground(line.backgroundOverride);
        }
    }

    private IEnumerator TypeLine(VNDialogueLine line)
    {
        isTyping = true;
        currentFullText = line.text;
        dialogueText.text = "";
        
        foreach (char c in line.text.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.02f); // Typing speed
        }
        isTyping = false;
    }

    private void EndDialogue()
    {
        isPlaying = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }
    
    private void Update()
    {
        if (!isPlaying) return;
        
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            DisplayNextLine();
        }
    }
}
