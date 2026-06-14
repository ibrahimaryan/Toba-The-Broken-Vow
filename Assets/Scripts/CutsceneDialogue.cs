using UnityEngine;

[System.Serializable]
public class CutsceneDialogueLine
{
    [Header("Dialog")]
    public string characterName;

    [TextArea(3, 10)]
    public string sentence;

    [Header("Portrait")]
    public Sprite portraitSprite;

    [Tooltip("Left / Center / Right")]
    public string portraitSlot = "Left";

    public bool showPortrait = true;

    public string[] hideCharacters;

    [Header("Background")]
    public Sprite backgroundSprite;

    [Header("Settings")]
    public bool isSpeechBubble = false;

    [Tooltip("Jika true harus ditekan tombol lanjut")]
    public bool waitForPlayerInput = true;

    public float autoNextDelay = 2f;
}

[System.Serializable]
public class CutsceneDialogue
{
    public CutsceneDialogueLine[] lines;
}