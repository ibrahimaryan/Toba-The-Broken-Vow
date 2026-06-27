using UnityEngine;

public class Chapter4CrossTrigger : MonoBehaviour
{
    private bool isPlayerInRange = false;

    private void Start()
    {
        UpdateState();
    }

    private void OnEnable()
    {
        PlayerControllerScript.OnInteractPressed += HandleInteraction;
    }

    private void OnDisable()
    {
        PlayerControllerScript.OnInteractPressed -= HandleInteraction;
        if (isPlayerInRange && InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt();
        }
    }

    public void UpdateState()
    {
        if (GameManager.Instance == null) return;

        bool crossReached = GameManager.Instance.IsFlagSet("chapter4_cross_reached");
        bool cangkulReceived = GameManager.Instance.IsFlagSet("chapter4_cangkul_received");
        bool dugTreasure = GameManager.Instance.IsFlagSet("chapter4_dug_treasure");

        if (dugTreasure)
        {
            gameObject.SetActive(false);
            return;
        }

        // Tanda silang aktif jika:
        // 1. Belum pernah dicapai (tahap 1)
        // 2. ATAU sudah dapat cangkul tapi belum digali (tahap 2)
        bool shouldBeActive = !crossReached || (cangkulReceived && !dugTreasure);
        gameObject.SetActive(shouldBeActive);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            ShowPromptIfNeeded();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.HidePrompt();
            }
        }
    }

    private void ShowPromptIfNeeded()
    {
        if (GameManager.Instance == null || InteractionPromptUI.Instance == null) return;

        bool crossReached = GameManager.Instance.IsFlagSet("chapter4_cross_reached");
        bool cangkulReceived = GameManager.Instance.IsFlagSet("chapter4_cangkul_received");

        if (!crossReached)
        {
            // Tahap 1: Langsung dipicu ketika dilewati
            Chapter4StoryManager.Instance.OnCrossReached();
        }
        else if (cangkulReceived)
        {
            // Tahap 2: Harus menggali menggunakan Cangkul
            string activeTool = InventoryManager.Instance != null ? InventoryManager.Instance.currentEquippedItem : "";
            if (activeTool == "cangkul")
            {
                InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk menggali");
            }
            else
            {
                InteractionPromptUI.Instance.ShowPrompt("Butuh Cangkul untuk menggali");
            }
        }
    }

    private void HandleInteraction()
    {
        if (!isPlayerInRange) return;
        if (GameManager.Instance == null) return;

        bool crossReached = GameManager.Instance.IsFlagSet("chapter4_cross_reached");
        bool cangkulReceived = GameManager.Instance.IsFlagSet("chapter4_cangkul_received");
        bool dugTreasure = GameManager.Instance.IsFlagSet("chapter4_dug_treasure");

        if (crossReached && cangkulReceived && !dugTreasure)
        {
            string activeTool = InventoryManager.Instance != null ? InventoryManager.Instance.currentEquippedItem : "";
            if (activeTool == "cangkul")
            {
                if (InteractionPromptUI.Instance != null)
                {
                    InteractionPromptUI.Instance.HidePrompt();
                }
                if (Chapter4StoryManager.Instance != null)
                {
                    Chapter4StoryManager.Instance.StartDigging();
                }
            }
        }
    }

    private void Update()
    {
        // Jika player di dalam area trigger dan cangkul sudah diterima, perbarui prompt
        // secara berkala/saat player menukar active equipment
        if (isPlayerInRange && GameManager.Instance != null && GameManager.Instance.IsFlagSet("chapter4_cangkul_received"))
        {
            ShowPromptIfNeeded();
        }
    }
}
