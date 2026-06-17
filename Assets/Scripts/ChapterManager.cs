using UnityEngine;

public class ChapterManager : MonoBehaviour
{
    [Header("Dialogue Configuration")]
    public VNDialogueData chapterIntroData;
    public DialogueManagerCS dialogueManager;

    private void Start()
    {
        // Play Intro Dialogue automatically when scene loads
        if (chapterIntroData != null && dialogueManager != null)
        {
            dialogueManager.PlayDialogue(chapterIntroData);
        }
    }
}
