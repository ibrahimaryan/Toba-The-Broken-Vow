using UnityEngine;

public class Chapter5PipeTrigger : MonoBehaviour
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

        bool solved = GameManager.Instance.IsFlagSet("bamboo_pipe_puzzle_solved");
        if (solved)
        {
            gameObject.SetActive(false); // Nonaktifkan trigger jika puzzle sudah selesai
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            ShowPromptIfNeeded();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPlayerInRange)
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

        bool solved = GameManager.Instance.IsFlagSet("bamboo_pipe_puzzle_solved");
        if (solved) return;

        bool arrivedDialogueDone = GameManager.Instance.IsFlagSet("chapter5_arrived_dialogue_done");

        if (!arrivedDialogueDone)
        {
            // Belum pernah berdialog saat sampai: Langsung picu sekuens dialog kedatangan
            if (Chapter5StoryManager.Instance != null)
            {
                Chapter5StoryManager.Instance.OnPlayerArrivedAtPipe();
            }
        }
        else
        {
            // Sudah berdialog tetapi puzzle ditutup (ESC): Tampilkan prompt tekan E
            InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk merapikan pipa");
        }
    }

    private void HandleInteraction()
    {
        if (!isPlayerInRange) return;
        if (GameManager.Instance == null) return;

        bool solved = GameManager.Instance.IsFlagSet("bamboo_pipe_puzzle_solved");
        if (!solved)
        {
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.HidePrompt();
            }

            if (Chapter5StoryManager.Instance != null)
            {
                Chapter5StoryManager.Instance.OpenPuzzlePanel();
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // Matikan trigger secara berkala jika puzzle terdeteksi selesai
        bool solved = GameManager.Instance.IsFlagSet("bamboo_pipe_puzzle_solved");
        if (solved)
        {
            gameObject.SetActive(false);
            return;
        }

        // Tampilkan/perbarui prompt interaksi secara dinamis ketika pemain berada di dalam range
        if (isPlayerInRange)
        {
            bool arrivedDialogueDone = GameManager.Instance.IsFlagSet("chapter5_arrived_dialogue_done");
            bool isPuzzleActive = Chapter5StoryManager.Instance != null && Chapter5StoryManager.Instance.IsPuzzleActive();

            if (arrivedDialogueDone && !isPuzzleActive)
            {
                if (InteractionPromptUI.Instance != null)
                {
                    InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk merapikan pipa");
                }
            }
            else
            {
                // Sembunyikan prompt jika panel puzzle sedang aktif atau dialog sedang berjalan
                if (InteractionPromptUI.Instance != null)
                {
                    InteractionPromptUI.Instance.HidePrompt();
                }
            }
        }
    }
}
