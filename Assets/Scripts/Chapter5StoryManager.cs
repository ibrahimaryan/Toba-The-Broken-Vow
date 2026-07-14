using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Chapter5StoryManager : MonoBehaviour
{
    public static Chapter5StoryManager Instance { get; private set; }

    [Header("Puzzle Configuration")]
    [SerializeField] private BambooPuzzleManager puzzleManager;

    [Header("GameObjects Configurations")]
    [SerializeField] private GameObject lockedPipeGameObject; // GameObject pipa terkunci di scene
    [SerializeField] private GameObject openPipeGameObject;   // GameObject pipa terbuka setelah solved
    [SerializeField] private GameObject opungNPCGameObject;    // NPC Opung
    [SerializeField] private Transform opungSpawnPoint;        // Lokasi muncul Opung

    [Header("Dialogue Configurations")]
    [SerializeField] private Dialogue openingDialogue;  // Dialog pembuka Chapter 5
    [SerializeField] private Dialogue arrivalDialogue;  // Dialog player saat sampai di pipa terkunci
    [SerializeField] private Dialogue successDialogue;  // Dialog sendiri setelah puzzle selesai
    [SerializeField] private Dialogue opungDialogue;    // Dialog dengan Opung setelah muncul

    [Header("Audio Settings")]
    [SerializeField] private AudioClip opungSpawnSound;  // Suara kemunculan Opung (opsional)
    private AudioSource audioSource;

    [Header("NPC Fade Settings")]
    [SerializeField] private float opungFadeDuration = 1.5f; // Durasi efek fade in Opung

    private bool isRunningSequence = false;
    private bool isPlayerInOpungZone = false;

    private void OnEnable()
    {
        PlayerControllerScript.OnInteractPressed += HandleOpungInteraction;
    }

    private void OnDisable()
    {
        PlayerControllerScript.OnInteractPressed -= HandleOpungInteraction;
        if (isPlayerInOpungZone && InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt();
        }
    }

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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        // PENGAMAN: Inisialisasi keadaan objek sesuai flag GameManager
        if (GameManager.Instance != null)
        {
            // Cek apakah Chapter 5 sudah aktif (Chapter 4 sudah selesai dengan menggali harta karun)
            bool isChapter5Active = GameManager.Instance.IsFlagSet("chapter4_dug_treasure");

            if (!isChapter5Active)
            {
                // Sembunyikan elemen Chapter 5 jika chapter ini belum aktif (tapi biarkan lockedPipeGameObject tetap aktif/terlihat)
                if (openPipeGameObject != null) openPipeGameObject.SetActive(false);
                if (opungNPCGameObject != null) opungNPCGameObject.SetActive(false);
                return; // Keluar, jangan jalankan sekuens Chapter 5
            }

            InitializeChapterState();
        }
        else
        {
            // Jika GameManager null (misal testing scene langsung), sembunyikan Opung secara default agar tidak bisa diinteraksi sebelum puzzle selesai
            if (opungNPCGameObject != null) opungNPCGameObject.SetActive(false);
        }
    }

    // Dipanggil secara real-time saat transisi dari Chapter 4 selesai
    public void StartChapter5()
    {
        InitializeChapterState();
    }

    private void InitializeChapterState()
    {
        if (GameManager.Instance == null) return;

        // Daftarkan listener sukses puzzle secara dinamis untuk menjamin pemanggilan
        if (puzzleManager != null)
        {
            puzzleManager.OnPuzzleSolved.RemoveListener(OnPuzzleCleared);
            puzzleManager.OnPuzzleSolved.AddListener(OnPuzzleCleared);
            Debug.Log("[Chapter5StoryManager] Berhasil mendaftarkan listener OnPuzzleSolved ke BambooPuzzleManager.");
        }

        bool introDone = GameManager.Instance.IsFlagSet("chapter5_intro_done");
        bool solved = GameManager.Instance.IsFlagSet("bamboo_pipe_puzzle_solved");
        bool opungTalked = GameManager.Instance.IsFlagSet("chapter5_opung_talked");

        // Atur status pipa
        if (lockedPipeGameObject != null) lockedPipeGameObject.SetActive(!solved);
        if (openPipeGameObject != null) openPipeGameObject.SetActive(solved);

        // Atur status Opung
        if (opungNPCGameObject != null)
        {
            opungNPCGameObject.SetActive(solved && !opungTalked);
            if (solved && !opungTalked)
            {
                if (opungSpawnPoint != null)
                {
                    opungNPCGameObject.transform.position = opungSpawnPoint.position;
                }

                // Pastikan alpha-nya 1 penuh jika diaktifkan sejak Start
                SpriteRenderer sr = opungNPCGameObject.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    sr.color = new Color(c.r, c.g, c.b, 1f);
                }
            }
        }

        // Jalankan intro jika belum pernah
        if (!introDone)
        {
            StartCoroutine(PlayOpeningSequence());
        }
        else
        {
            // Isi kembali daftarMisi Chapter 5 agar tidak kosong saat scene di-reload
            SetupChapter5Quests();

            // Restore status misi berdasarkan flags yang sudah tersimpan
            bool arrivedAtPipe = GameManager.Instance.IsFlagSet("chapter5_arrived_dialogue_done");
            if (arrivedAtPipe && ToDoManager.Instance != null && ToDoManager.Instance.daftarMisi.Count > 0)
                ToDoManager.Instance.daftarMisi[0].sudahSelesai = true;

            if (solved && ToDoManager.Instance != null && ToDoManager.Instance.daftarMisi.Count > 1)
                ToDoManager.Instance.daftarMisi[1].sudahSelesai = true;

            if (opungTalked && ToDoManager.Instance != null && ToDoManager.Instance.daftarMisi.Count > 2)
                ToDoManager.Instance.daftarMisi[2].sudahSelesai = true;

            if (ToDoManager.Instance != null)
                ToDoManager.Instance.UpdateTampilanUI();

            UpdateObjectivePointer();
        }
    }

    // Coroutine untuk Dialog Pembuka Chapter 5
    private IEnumerator PlayOpeningSequence()
    {
        isRunningSequence = true;

        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null)
        {
            player.StopMovement();
            player.ToggleInput(false);
        }

        // Tunggu sampai popup memory shard dan dialog VN-nya selesai sepenuhnya
        yield return null; // Tunggu satu frame agar setup awal selesai
        while (MemoryShardManager.Instance != null && 
               ((MemoryShardManager.Instance.popupPanel != null && MemoryShardManager.Instance.popupPanel.activeInHierarchy) ||
                (MemoryShardManager.Instance.dialogueManager != null && MemoryShardManager.Instance.dialogueManager.IsPlaying)))
        {
            yield return null;
        }

        // Jalankan dialog pembuka
        if (openingDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(openingDialogue);
            yield return new WaitForSeconds(0.1f);
            while (IsDialogueActive()) yield return null;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag("chapter5_intro_done", true);
        }

        // Daftarkan Misi di ToDoManager
        SetupChapter5Quests();

        // Arahkan pointer ke pipa terkunci
        UpdateObjectivePointer();

        if (player != null) player.ToggleInput(true);
        isRunningSequence = false;
    }

    // Dipanggil oleh Chapter5PipeTrigger ketika Player masuk pertama kali
    public void OnPlayerArrivedAtPipe()
    {
        if (isRunningSequence) return;

        if (GameManager.Instance != null && !GameManager.Instance.IsFlagSet("chapter5_arrived_dialogue_done"))
        {
            StartCoroutine(PlayArrivalSequence());
        }
        else
        {
            OpenPuzzlePanel();
        }
    }

    private IEnumerator PlayArrivalSequence()
    {
        isRunningSequence = true;

        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null)
        {
            player.StopMovement();
            player.ToggleInput(false);
        }

        // Sembunyikan prompt interaksi jika ada
        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt();
        }

        // Dialog setibanya di pipa terkunci
        if (arrivalDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(arrivalDialogue);
            yield return new WaitForSeconds(0.1f);
            while (IsDialogueActive()) yield return null;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag("chapter5_arrived_dialogue_done", true);
        }

        // Selesaikan misi pertama (periksa pipa terkunci)
        if (ToDoManager.Instance != null)
        {
            ToDoManager.Instance.SelesaikanMisi(0); 
        }

        isRunningSequence = false;
        
        // Langsung buka panel puzzle setelah dialog selesai
        OpenPuzzlePanel();
    }

    public void OpenPuzzlePanel()
    {
        if (puzzleManager != null)
        {
            puzzleManager.OpenPuzzle();
        }
    }

    public bool IsPuzzleActive()
    {
        return puzzleManager != null && puzzleManager.IsPuzzlePanelActive;
    }

    // Dipanggil melalui UnityEvent dari BambooPuzzleManager ketika puzzle selesai terpecahkan
    public void OnPuzzleCleared()
    {
        StartCoroutine(PlaySuccessSequence());
    }

    private IEnumerator PlaySuccessSequence()
    {
        Debug.Log("[Chapter5StoryManager] PlaySuccessSequence Started!");
        isRunningSequence = true;

        // Tutup panel puzzle terlebih dahulu
        if (puzzleManager != null)
        {
            puzzleManager.ClosePuzzlePanel();
        }

        // Sembunyikan/hilangkan pointer pipa karena puzzle telah selesai dipecahkan
        UpdateObjectivePointer();

        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null)
        {
            player.StopMovement();
            player.ToggleInput(false);
        }

        // Hapus Kunci Bambu dari Inventory karena telah digunakan untuk membuka pipa
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem("kunci_bambu", 1);
            Debug.Log("[Chapter5StoryManager] Kunci Bambu dihapus dari Inventory.");
        }

        // 1. Ganti GameObject Pipa Terkunci ke Pipa Terbuka
        if (lockedPipeGameObject != null) lockedPipeGameObject.SetActive(false);
        if (openPipeGameObject != null) openPipeGameObject.SetActive(true);

        // 2. Dialog sendiri (self-dialogue) setelah sukses merapikan pipa
        if (successDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(successDialogue);
            yield return new WaitForSeconds(0.1f);
            while (IsDialogueActive()) yield return null;
        }

        // Selesaikan misi kedua (selesaikan puzzle)
        if (ToDoManager.Instance != null)
        {
            ToDoManager.Instance.SelesaikanMisi(1); 
        }

        // 3. Spawning NPC Opung dengan efek fade in
        if (opungNPCGameObject != null)
        {
            if (opungSpawnPoint != null)
            {
                opungNPCGameObject.transform.position = opungSpawnPoint.position;
            }

            // Putar suara teleport/kemunculan jika ada
            if (audioSource != null && opungSpawnSound != null)
            {
                audioSource.PlayOneShot(opungSpawnSound);
            }

            // Jalankan efek fade in
            yield return StartCoroutine(FadeInOpungCoroutine());
        }

        // Pulihkan input player agar pemain bisa jalan mendekati Opung untuk berinteraksi
        if (player != null) player.ToggleInput(true);
        isRunningSequence = false;

        // Arahkan pointer ke Opung setelah muncul
        UpdateObjectivePointer();
    }

    // Dipanggil oleh Chapter5OpungTrigger ketika player masuk/keluar area Opung
    public void SetPlayerInOpungZone(bool inZone)
    {
        isPlayerInOpungZone = inZone;
        Debug.Log($"[Chapter5StoryManager] Player in Opung Zone: {inZone}");

        if (isPlayerInOpungZone)
        {
            // Pastikan puzzle sudah selesai sebelum mengizinkan interaksi dengan Opung
            bool puzzleSolved = GameManager.Instance != null && GameManager.Instance.IsFlagSet("bamboo_pipe_puzzle_solved");

            // Tampilkan prompt interaksi jika Opung aktif dan dialog belum selesai
            if (puzzleSolved && opungNPCGameObject != null && opungNPCGameObject.activeSelf && 
                GameManager.Instance != null && !GameManager.Instance.IsFlagSet("chapter5_opung_talked") && 
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
            // Sembunyikan prompt saat keluar area
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.HidePrompt();
            }
        }
    }

    private void HandleOpungInteraction()
    {
        bool puzzleSolved = GameManager.Instance != null && GameManager.Instance.IsFlagSet("bamboo_pipe_puzzle_solved");
        if (isPlayerInOpungZone && puzzleSolved &&
            opungNPCGameObject != null && opungNPCGameObject.activeSelf &&
            GameManager.Instance != null &&
            !GameManager.Instance.IsFlagSet("chapter5_opung_talked") &&
            !isRunningSequence)
        {
            Debug.Log("[Chapter5StoryManager] Player berinteraksi dengan Opung.");
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.HidePrompt();
            }
            StartCoroutine(PlayOpungSequenceCoroutine());
        }
    }

    private IEnumerator PlayOpungSequenceCoroutine()
    {
        isRunningSequence = true;

        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null)
        {
            player.StopMovement();
            player.ToggleInput(false);
        }

        // Mainkan dialog Opung
        if (opungDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(opungDialogue);
            yield return new WaitForSeconds(0.1f);
            while (IsDialogueActive()) yield return null;
        }

        // Efek Opung Menghilang (Fade Out)
        yield return StartCoroutine(FadeOutOpungCoroutine());

        // Set status selesai ke GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag("chapter5_opung_talked", true);
            GameManager.Instance.SetFlag("chapter5_completed", true);
        }

        // Selesaikan misi ketiga (bicara dengan Opung)
        if (ToDoManager.Instance != null)
        {
            ToDoManager.Instance.SelesaikanMisi(2); 
        }

        // Matikan petunjuk pointer
        UpdateObjectivePointer();

        if (MemoryShardManager.Instance != null)
        {
            MemoryShardManager.Instance.UnlockShard("Chapter5");
        }

        if (player != null) player.ToggleInput(true);
        isRunningSequence = false;
    }

    private void SetupChapter5Quests()
    {
        if (ToDoManager.Instance != null)
        {
            ToDoManager.Instance.SetCurrentChapterID("Chapter_5");
            ToDoManager.Instance.daftarMisi = new List<Quest>
            {
                new Quest { namaMisi = "Periksa saluran pipa bambu yang tersumbat", sudahSelesai = false },
                new Quest { namaMisi = "Rapikan susunan pipa bambu agar air mengalir", sudahSelesai = false },
                new Quest { namaMisi = "Bicara dengan Opung yang tiba-tiba muncul", sudahSelesai = false }
            };
            ToDoManager.Instance.UpdateTampilanUI();
        }
    }

    public void UpdateObjectivePointer()
    {
        if (ObjectivePointer.Instance == null || GameManager.Instance == null) return;

        bool solved = GameManager.Instance.IsFlagSet("bamboo_pipe_puzzle_solved");
        bool opungTalked = GameManager.Instance.IsFlagSet("chapter5_opung_talked");

        Transform newTarget = null;

        if (opungTalked)
        {
            newTarget = null;
        }
        else if (solved)
        {
            if (opungNPCGameObject != null && opungNPCGameObject.activeSelf)
            {
                newTarget = opungNPCGameObject.transform;
            }
        }
        else
        {
            if (lockedPipeGameObject != null)
            {
                newTarget = lockedPipeGameObject.transform;
            }
        }

        ObjectivePointer.Instance.SetTarget(newTarget);
    }

    private bool IsDialogueActive()
    {
        if (DialogueManager.instance == null) return false;

        bool screenActive = DialogueManager.instance.screenBoxPanel != null && DialogueManager.instance.screenBoxPanel.activeSelf;
        bool bubbleActive = DialogueManager.instance.bubblePanel != null && DialogueManager.instance.bubblePanel.activeSelf;
        bool cutsceneActive = DialogueManager.instance.cutsceneBoxPanel != null && DialogueManager.instance.cutsceneBoxPanel.activeSelf;

        return screenActive || bubbleActive || cutsceneActive;
    }

    private IEnumerator FadeInOpungCoroutine()
    {
        Debug.Log($"[Chapter5StoryManager] FadeInOpungCoroutine Started! opungNPCGameObject: {opungNPCGameObject != null}");
        if (opungNPCGameObject == null) yield break;

        SpriteRenderer sr = opungNPCGameObject.GetComponent<SpriteRenderer>();
        Debug.Log($"[Chapter5StoryManager] SpriteRenderer found on parent: {sr != null}");
        if (sr == null)
        {
            sr = opungNPCGameObject.GetComponentInChildren<SpriteRenderer>();
            Debug.Log($"[Chapter5StoryManager] SpriteRenderer found in children: {sr != null}");
        }
        if (sr != null)
        {
            // Set alpha ke 0 terlebih dahulu
            Color originalColor = sr.color;
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            
            opungNPCGameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < opungFadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / opungFadeDuration);
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }

            // Pastikan alpha kembali ke 1 penuh setelah selesai
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }
        else
        {
            opungNPCGameObject.SetActive(true);
        }
    }

    private IEnumerator FadeOutOpungCoroutine()
    {
        if (opungNPCGameObject == null) yield break;

        SpriteRenderer sr = opungNPCGameObject.GetComponent<SpriteRenderer>();
        if (sr == null) sr = opungNPCGameObject.GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
        {
            float elapsed = 0f;
            Color originalColor = sr.color;

            while (elapsed < opungFadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / opungFadeDuration);
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
            // Kembalikan ke 1f sebelum dimatikan agar jika di-load nanti tidak transparan
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }

        opungNPCGameObject.SetActive(false);
        Debug.Log("[Chapter5StoryManager] NPC Opung telah menghilang (Fade Out).");
    }
}
