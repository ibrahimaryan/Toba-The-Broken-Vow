using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CutsceneDialogueManager : MonoBehaviour
{
    public static CutsceneDialogueManager Instance;

    [Header("UI")]
    public GameObject dialoguePanel;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Portrait")]
    public CutscenePortraitManager portraitManager;

    [Header("Background")]
    public CutsceneBackgroundManager backgroundManager;

    [Header("Typing")]
    public float typingSpeed = 0.02f;

    Queue<CutsceneDialogueLine> lines =
        new Queue<CutsceneDialogueLine>();

    bool isTyping;
    bool waitingInput;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void StartDialogue(
        CutsceneDialogue dialogue)
    {
        StopAllCoroutines();

        lines.Clear();

        foreach (var line in dialogue.lines)
        {
            lines.Enqueue(line);
        }

        ShowNextLine();
    }

    void Update()
    {
        if (!waitingInput)
            return;

        bool nextPressed =
            Keyboard.current.eKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame ||
            Mouse.current.leftButton.wasPressedThisFrame;

        if (nextPressed)
        {
            waitingInput = false;
            ShowNextLine();
        }
    }

    void ShowNextLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        StopAllCoroutines();

        StartCoroutine(
            ProcessLine(lines.Dequeue()));
    }

    IEnumerator ProcessLine(
        CutsceneDialogueLine line)
    {
        if (line.backgroundSprite != null)
        {
            yield return StartCoroutine(
                backgroundManager.ChangeBackground(
                    line.backgroundSprite));
        }

        if (line.hideCharacters != null)
        {
            foreach (string c in line.hideCharacters)
            {
                portraitManager.HideCharacter(c);
            }
        }

        if (line.showPortrait &&
            line.portraitSprite != null)
        {
            portraitManager.ShowPortrait(
                line.portraitSlot,
                line.characterName,
                line.portraitSprite);
        }

        portraitManager.SetSpeaker(
            line.characterName);

        dialoguePanel.SetActive(true);

        nameText.text =
            line.characterName;

        dialogueText.text = "";

        isTyping = true;

        foreach (char letter in line.sentence)
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(
                typingSpeed);
        }

        isTyping = false;

        if (line.waitForPlayerInput)
        {
            waitingInput = true;
        }
        else
        {
            yield return new WaitForSeconds(
                line.autoNextDelay);

            ShowNextLine();
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}