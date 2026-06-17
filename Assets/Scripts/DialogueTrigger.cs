using UnityEngine;
using UnityEngine.Events;
public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    [Tooltip("Centang jika dialog ini hanya boleh muncul satu kali saja saat dilewati.")]
    public bool triggerOnlyOnce = true;
    public string triggerID;

    private bool hasTriggered = false;
    public UnityEvent saatDialogBerjalan;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (triggerOnlyOnce)
        {
            if (hasTriggered) return;
            if (GameManager.Instance != null && GameManager.Instance.IsFlagSet(triggerID)) return;
        }

        DialogueManager.instance.StartDialogue(dialogue);
        saatDialogBerjalan?.Invoke();
        hasTriggered = true;

        if (triggerOnlyOnce && GameManager.Instance != null && !string.IsNullOrEmpty(triggerID))
        {
            GameManager.Instance.SetFlag(triggerID, true);
        }
    }
}