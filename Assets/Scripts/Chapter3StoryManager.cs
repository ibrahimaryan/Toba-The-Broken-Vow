using UnityEngine;
using System.Collections;

public class Chapter3StoryManager : MonoBehaviour
{
    public static Chapter3StoryManager Instance { get; private set; }

    [Header("Immediate Dialogue Settings")]
    [SerializeField] private Dialogue exitDialogue;
    [SerializeField] private string exitDialogueFlag = "chapter3_exit_dialogue_played";
    [SerializeField] private Transform objectiveTargetTransform; // Tarik Empty GameObject target ke sini

    [Header("NPC Event Settings")]
    [SerializeField] public GameObject npcGameObject; // NPC yang akan tiba-tiba muncul
    [SerializeField] private Dialogue npcDialogue;
    [SerializeField] private string explorationFlag = "chapter3_player_explored_map";
    [SerializeField] private string npcSequenceFlag = "chapter3_npc_sequence_played";

    [Header("Puzzle Settings")]
    [SerializeField] private Chapter3PuzzleTrigger chapter3PuzzleTrigger;

    [Header("NPC Fade Settings")]
    [SerializeField] private float npcFadeDuration = 1.5f;

    private bool isPlayerInHutZone = false;
    private bool isRunningSequence = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        PlayerControllerScript.OnInteractPressed += HandleInteraction;
    }

    private void OnDisable()
    {
        PlayerControllerScript.OnInteractPressed -= HandleInteraction;
    }

    private void Start()
    {
        // 1. Setup awal status NPC
        if (npcGameObject != null)
        {
            // Sembunyikan NPC jika sequence belum berjalan (hanya muncul saat terpicu)
            // Atau nonaktifkan jika sequence sudah pernah selesai sepenuhnya
            if (GameManager.Instance != null && GameManager.Instance.IsFlagSet(npcSequenceFlag))
            {
                npcGameObject.SetActive(false);
            }
            else if (GameManager.Instance != null && GameManager.Instance.IsFlagSet("sisik_puzzle_solved") && GameManager.Instance.IsFlagSet(explorationFlag))
            {
                // Jika sudah dieksplorasi tapi dialog belum dimainkan, aktifkan NPC sejak Start
                npcGameObject.SetActive(true);
            }
            else
            {
                npcGameObject.SetActive(false); 
            }
        }

        // 2. Jalankan dialog langsung setelah keluar ruangan jika puzzle sisik sudah selesai dan dialog belum diputar
        if (GameManager.Instance != null)
        {
            bool sisikSolved = GameManager.Instance.IsFlagSet("sisik_puzzle_solved");
            bool exitPlayed = GameManager.Instance.IsFlagSet(exitDialogueFlag);
            bool explored = GameManager.Instance.IsFlagSet(explorationFlag);

            Debug.Log($"[Chapter3StoryManager] Start - Flags: sisik_puzzle_solved={sisikSolved}, exitDialogueFlag={exitPlayed}, explorationFlag={explored}");

            if (sisikSolved)
            {
                if (!exitPlayed)
                {
                    StartCoroutine(PlayExitDialogueCoroutine());
                }
                else if (!explored && objectiveTargetTransform != null && ObjectivePointer.Instance != null)
                {
                    // Jika dialog keluar gubug sudah diputar tapi eksplorasi target belum diselesaikan, aktifkan panah kembali
                    ObjectivePointer.Instance.SetTarget(objectiveTargetTransform);
                }
            }
            else
            {
                Debug.LogWarning("[Chapter3StoryManager] 'sisik_puzzle_solved' belum bernilai True. Sekuens Chapter 3 tidak akan berjalan. (Saran: Selesaikan puzzle sisik terlebih dahulu, atau jika sedang testing scene luar secara langsung, pastikan flag 'sisik_puzzle_solved' diset ke True di GameManager Anda!)");
            }
        }
    }

    private IEnumerator PlayExitDialogueCoroutine()
    {
        yield return new WaitForSeconds(0.5f); // Beri sedikit jeda setelah scene loading selesai

        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null) player.ToggleInput(false);

        if (exitDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(exitDialogue);

            yield return new WaitForSeconds(0.1f);
            while (IsDialogueActive())
            {
                yield return null;
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag(exitDialogueFlag, true);
        }

        // Aktifkan panah penunjuk jalan ke arah target setelah dialog keluar ruangan selesai
        if (objectiveTargetTransform != null && ObjectivePointer.Instance != null)
        {
            ObjectivePointer.Instance.SetTarget(objectiveTargetTransform);
        }

        if (player != null) player.ToggleInput(true);
    }

    // Helper untuk mengambil target objek tujuan
    public Transform GetObjectiveTarget()
    {
        return objectiveTargetTransform;
    }

    // Dipanggil oleh trigger area ketika player berjalan-jalan di map
    public void TriggerExploration()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsFlagSet(explorationFlag))
        {
            GameManager.Instance.SetFlag(explorationFlag, true);
            Debug.Log("[Chapter3StoryManager] Eksplorasi map berhasil diselesaikan oleh Player.");
            
            // Coba munculkan NPC jika player kebetulan sudah berada di gubug saat flag ini aktif
            if (isPlayerInHutZone)
            {
                SetPlayerInHutZone(true);
            }
        }
    }

    // Dipanggil oleh trigger di dekat gubug (OnTriggerEnter2D)
    public void SetPlayerInHutZone(bool inZone)
    {
        isPlayerInHutZone = inZone;
        Debug.Log($"[Chapter3StoryManager] Player in Hut Zone: {inZone}");

        if (isPlayerInHutZone)
        {
            // Coba munculkan NPC
            TrySpawnNPC();

            // Tampilkan prompt interaksi jika NPC aktif dan dialog belum selesai
            if (npcGameObject != null && npcGameObject.activeSelf && 
                GameManager.Instance != null && !GameManager.Instance.IsFlagSet(npcSequenceFlag) && 
                !isRunningSequence)
            {
                if (InteractionPromptUI.Instance != null)
                {
                    InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk bicara");
                }
            }
        }
        else
        {
            // Sembunyikan prompt saat keluar area gubug
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.HidePrompt();
            }
        }
    }

    private void TrySpawnNPC()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsFlagSet("sisik_puzzle_solved") &&
            GameManager.Instance.IsFlagSet(explorationFlag) &&
            !GameManager.Instance.IsFlagSet(npcSequenceFlag))
        {
            if (npcGameObject != null && !npcGameObject.activeSelf)
            {
                npcGameObject.SetActive(true);
                Debug.Log("[Chapter3StoryManager] NPC Baru dimunculkan di dekat gubug.");

                SpriteRenderer sr = npcGameObject.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    sr.color = new Color(c.r, c.g, c.b, 1f);
                }
            }
        }
    }

    private void HandleInteraction()
    {
        if (isPlayerInHutZone && 
            npcGameObject != null && npcGameObject.activeSelf &&
            GameManager.Instance != null &&
            !GameManager.Instance.IsFlagSet(npcSequenceFlag) &&
            !isRunningSequence)
        {
            Debug.Log("[Chapter3StoryManager] Player berinteraksi dengan NPC.");
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.HidePrompt();
            }
            StartCoroutine(PlayNPCSequenceCoroutine());
        }
    }

    private IEnumerator PlayNPCSequenceCoroutine()
    {
        isRunningSequence = true;

        // 1. Matikan input player selama dialog
        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null)
        {
            player.StopMovement();
            player.ToggleInput(false);
        }

        // 2. Mainkan dialog NPC
        if (npcDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(npcDialogue);

            yield return new WaitForSeconds(0.1f);
            while (IsDialogueActive())
            {
                yield return null;
            }
        }

        // 4. NPC perlahan menghilang
        yield return StartCoroutine(FadeOutNPCCoroutine());

        // 5. Puzzle mulai berkedip (blinking)
        if (chapter3PuzzleTrigger != null)
        {
            chapter3PuzzleTrigger.StartBlink();
        }

        // 6. Simpan status sequence selesai
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag(npcSequenceFlag, true);
        }

        // 7. Kembalikan input player
        if (player != null) player.ToggleInput(true);

        isRunningSequence = false;
    }

    private IEnumerator FadeOutNPCCoroutine()
    {
        if (npcGameObject == null) yield break;

        SpriteRenderer sr = npcGameObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float elapsed = 0f;
            Color originalColor = sr.color;

            while (elapsed < npcFadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / npcFadeDuration);
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        npcGameObject.SetActive(false);
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
