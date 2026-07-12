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
    [SerializeField] private float npcInteractionDistance = 2.0f; // Jarak interaksi dekat dengan NPC

    [Header("Puzzle Settings")]
    [SerializeField] private Chapter3PuzzleTrigger chapter3PuzzleTrigger;

    [Header("NPC Fade Settings")]
    [SerializeField] private float npcFadeDuration = 1.5f;

    private bool isPlayerInHutZone = false;
    private bool isRunningSequence = false;
    private bool isNearNPC = false;
    private Transform currentTarget = null;

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
        // PENGAMAN: Jika Chapter 3 sudah selesai (kapak diambil), jangan sentuh NPC ini karena objeknya sama dengan Chapter 4 & 5
        bool isChapter3Finished = GameManager.Instance != null && GameManager.Instance.IsFlagSet("chapter3_axe_collected");

        // 1. Setup awal status NPC
        if (npcGameObject != null && !isChapter3Finished)
        {
            // Sembunyikan NPC jika sequence belum berjalan (hanya muncul saat terpicu)
            // Atau nonaktifkan jika sekuens sudah pernah selesai sepenuhnya
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
                else
                {
                    UpdateObjectivePointer();
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

        UpdateObjectivePointer();

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

            UpdateObjectivePointer();
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

    private void Update()
    {
        // Cek jarak pemain ke NPC untuk menampilkan prompt interaksi secara dinamis
        if (npcGameObject != null && npcGameObject.activeSelf && 
            GameManager.Instance != null && !GameManager.Instance.IsFlagSet(npcSequenceFlag) && 
            !isRunningSequence)
        {
            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null)
            {
                float distance = Vector2.Distance(player.transform.position, npcGameObject.transform.position);
                if (distance <= npcInteractionDistance)
                {
                    if (!isNearNPC)
                    {
                        isNearNPC = true;
                        if (InteractionPromptUI.Instance != null)
                        {
                            InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk bicara");
                        }
                    }
                }
                else
                {
                    if (isNearNPC)
                    {
                        isNearNPC = false;
                        if (InteractionPromptUI.Instance != null)
                        {
                            InteractionPromptUI.Instance.HidePrompt();
                        }
                    }
                }
            }
        }
        else
        {
            if (isNearNPC)
            {
                isNearNPC = false;
                if (InteractionPromptUI.Instance != null)
                {
                    InteractionPromptUI.Instance.HidePrompt();
                }
            }
        }
    }

    private void HandleInteraction()
    {
        if (isNearNPC && 
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
        Debug.Log("[Chapter3StoryManager] PlayNPCSequenceCoroutine - Memulai sekuens NPC...");
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
            Debug.Log("[Chapter3StoryManager] PlayNPCSequenceCoroutine - Menjalankan dialog NPC...");
            DialogueManager.instance.StartDialogue(npcDialogue);

            yield return new WaitForSeconds(0.1f);
            while (IsDialogueActive())
            {
                yield return null;
            }
            Debug.Log("[Chapter3StoryManager] PlayNPCSequenceCoroutine - Dialog NPC selesai.");
        }
        else
        {
            Debug.LogWarning($"[Chapter3StoryManager] PlayNPCSequenceCoroutine - npcDialogue ada? {npcDialogue != null}, DialogueManager ada? {DialogueManager.instance != null}");
        }

        // 4. NPC perlahan menghilang
        Debug.Log("[Chapter3StoryManager] PlayNPCSequenceCoroutine - Menghilangkan NPC...");
        yield return StartCoroutine(FadeOutNPCCoroutine());
        Debug.Log("[Chapter3StoryManager] PlayNPCSequenceCoroutine - NPC berhasil dihilangkan.");

        // 5. Puzzle mulai berkedip (blinking)
        if (chapter3PuzzleTrigger != null)
        {
            chapter3PuzzleTrigger.StartBlink();
        }

        // 6. Simpan status sequence selesai
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag(npcSequenceFlag, true);
            Debug.Log($"[Chapter3StoryManager] PlayNPCSequenceCoroutine - Flag {npcSequenceFlag} diset ke true.");
        }

        Debug.Log("[Chapter3StoryManager] PlayNPCSequenceCoroutine - Memperbarui pointer objektif...");
        UpdateObjectivePointer();

        // 7. Kembalikan input player
        if (player != null) player.ToggleInput(true);

        isRunningSequence = false;
        Debug.Log("[Chapter3StoryManager] PlayNPCSequenceCoroutine - Sekuens NPC SELESAI.");
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

    public void UpdateObjectivePointer()
    {
        if (ObjectivePointer.Instance == null || GameManager.Instance == null) return;

        bool exitPlayed = GameManager.Instance.IsFlagSet(exitDialogueFlag);
        bool explored = GameManager.Instance.IsFlagSet(explorationFlag);
        bool npcSequencePlayed = GameManager.Instance.IsFlagSet(npcSequenceFlag);
        bool puzzleSolved = GameManager.Instance.IsFlagSet("chapter3_puzzle_solved");
        bool axeCollected = GameManager.Instance.IsFlagSet("chapter3_axe_collected");

        Transform newTarget = null;

        Debug.Log($"[Chapter3StoryManager] UpdateObjectivePointer - exitPlayed={exitPlayed}, explored={explored}, npcSequencePlayed={npcSequencePlayed}, puzzleSolved={puzzleSolved}, axeCollected={axeCollected}");

        if (axeCollected)
        {
            newTarget = null;
        }
        else if (puzzleSolved)
        {
            var axeTrigger = FindAnyObjectByType<AxePickupTrigger>();
            if (axeTrigger != null) newTarget = axeTrigger.transform;
        }
        else if (npcSequencePlayed)
        {
            if (chapter3PuzzleTrigger != null) 
            {
                newTarget = chapter3PuzzleTrigger.transform;
            }
            else
            {
                var trigger = FindAnyObjectByType<Chapter3PuzzleTrigger>();
                if (trigger != null) newTarget = trigger.transform;
            }
        }
        else if (explored)
        {
            // Arahkan ke NPC (di Gubug) meskipun NPC belum aktif/muncul di scene
            if (npcGameObject != null) 
            {
                newTarget = npcGameObject.transform;
            }
        }
        else if (exitPlayed)
        {
            newTarget = objectiveTargetTransform;
        }

        Debug.Log($"[Chapter3StoryManager] UpdateObjectivePointer - final newTarget: {(newTarget != null ? newTarget.name : "null")}, currentTarget: {(currentTarget != null ? currentTarget.name : "null")}");

        if (newTarget != currentTarget)
        {
            currentTarget = newTarget;
            ObjectivePointer.Instance.SetTarget(newTarget);
        }
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
