using System.Collections;
using UnityEngine;

public class PendingReturnDialoguePlayer : MonoBehaviour
{
    [System.Serializable]
    public struct ReturnDialogueSetup
    {
        public string dialogueID;
        public Dialogue dialogue;
        public bool playOnlyOnce;
    }

    [Header("Dialog Setelah Kembali Dari Cutscene")]
    [SerializeField] private ReturnDialogueSetup[] returnDialogues;
    [SerializeField] private float delayBeforePlay = 0.2f;

    private IEnumerator Start()
    {
        string pendingID = PlayerPrefs.GetString("PendingReturnDialogueID", "");
        if (string.IsNullOrEmpty(pendingID)) yield break;

        PlayerPrefs.DeleteKey("PendingReturnDialogueID");

        yield return new WaitForSeconds(delayBeforePlay);

        foreach (ReturnDialogueSetup setup in returnDialogues)
        {
            if (setup.dialogueID != pendingID) continue;
            if (setup.dialogue == null) yield break;

            string flagID = $"return_dialogue_{pendingID}";
            if (setup.playOnlyOnce && GameManager.Instance != null && GameManager.Instance.IsFlagSet(flagID))
            {
                yield break;
            }

            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.StartDialogue(setup.dialogue);
            }

            if (setup.playOnlyOnce && GameManager.Instance != null)
            {
                GameManager.Instance.SetFlag(flagID, true);
            }

            yield break;
        }

        Debug.LogWarning($"PendingReturnDialoguePlayer: Dialogue ID '{pendingID}' tidak ditemukan di scene ini.");
    }
}
