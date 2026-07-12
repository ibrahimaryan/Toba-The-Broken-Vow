using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ObjectiveTarget : MonoBehaviour
{
    [Header("Dialogue & Flag Settings")]
    [SerializeField] private Dialogue targetDialogue;
    [SerializeField] private string targetFlag = "chapter3_objective_reached";
    [SerializeField] private bool triggerOnlyOnce = true;

    [Header("Events (Optional)")]
    [SerializeField] private UnityEvent onReached;

    [Header("Pointer Settings")]
    [SerializeField] private bool registerOnStart = false;

    private bool hasTriggered = false;

    private void Start()
    {
        // Jika target sudah pernah dicapai, matikan gameobject ini
        if (GameManager.Instance != null && GameManager.Instance.IsFlagSet(targetFlag))
        {
            gameObject.SetActive(false);
            return;
        }

        // Jika diset untuk langsung aktif menjadi target di awal scene
        if (registerOnStart && ObjectivePointer.Instance != null)
        {
            ObjectivePointer.Instance.SetTarget(transform);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (triggerOnlyOnce)
        {
            if (hasTriggered) return;
            if (GameManager.Instance != null && GameManager.Instance.IsFlagSet(targetFlag)) return;
        }

        TriggerTargetReached();
    }

    private void TriggerTargetReached()
    {
        hasTriggered = true;

        // 1. Simpan status target selesai ke GameManager
        if (GameManager.Instance != null && !string.IsNullOrEmpty(targetFlag))
        {
            GameManager.Instance.SetFlag(targetFlag, true);
        }

        // 2. Beritahukan ke Chapter3StoryManager bahwa map telah dieksplorasi
        if (Chapter3StoryManager.Instance != null)
        {
            Chapter3StoryManager.Instance.TriggerExploration();
        }
        else
        {
            // 3. Hilangkan panah penunjuk arah jika tidak ada Story Manager yang mengaturnya
            if (ObjectivePointer.Instance != null)
            {
                ObjectivePointer.Instance.ClearTarget();
            }
        }

        // 4. Mainkan dialog dan matikan input player sejenak
        StartCoroutine(PlayDialogueSequence());
    }

    private IEnumerator PlayDialogueSequence()
    {
        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null) player.ToggleInput(false);

        if (targetDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(targetDialogue);

            yield return new WaitForSeconds(0.1f);
            while (IsDialogueActive())
            {
                yield return null;
            }
        }

        // Jalankan event opsional jika dipasang
        onReached?.Invoke();

        if (player != null) player.ToggleInput(true);

        // Nonaktifkan area tujuan karena sudah selesai
        gameObject.SetActive(false);
    }

    private bool IsDialogueActive()
    {
        if (DialogueManager.instance == null) return false;

        bool screenActive = DialogueManager.instance.screenBoxPanel != null && DialogueManager.instance.screenBoxPanel.activeSelf;
        bool bubbleActive = DialogueManager.instance.bubblePanel != null && DialogueManager.instance.bubblePanel.activeSelf;
        bool cutsceneActive = DialogueManager.instance.cutsceneBoxPanel != null && DialogueManager.instance.cutsceneBoxPanel.activeSelf;

        return screenActive || bubbleActive || cutsceneActive;
    }
}
