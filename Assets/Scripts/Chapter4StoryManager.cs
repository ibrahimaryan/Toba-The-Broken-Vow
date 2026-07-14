using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chapter4StoryManager : MonoBehaviour
{
    public static Chapter4StoryManager Instance { get; private set; }

    [Header("NPC Configuration")]
    [SerializeField] public GameObject npcGameObject; // NPC diubah ke public agar bisa dibaca oleh DialogueManager
    [SerializeField] private Transform npcPostAxePoint; // Titik NPC muncul setelah Kapak diambil
    [SerializeField] private Transform npcHutPoint;     // Titik NPC kembali di Gubug

    [Header("Dialogue Configurations")]
    [SerializeField] private Dialogue postAxeDialogue;        // Dialog awal Chapter 4 saat ketemu NPC
    [SerializeField] private Dialogue crossDialogue;          // Dialog saat mencapai tanda silang
    [SerializeField] private Dialogue pickaxeApproachDialogue; // Dialog saat mendekati pickaxe (equipment 2)
    [SerializeField] private Dialogue postDigDialogue;         // Dialog setelah menggali harta karun
    [SerializeField] private Dialogue missingItemsDialogue;   // Dialog jika barang belum lengkap
    [SerializeField] private Dialogue handoverDialogue;       // Dialog penyerahan kayu & batu
    [SerializeField] private Dialogue cangkulReceivedDialogue; // Dialog setelah menerima cangkul

    public Dialogue PickaxeApproachDialogue => pickaxeApproachDialogue;

    [Header("Quest Requirements")]
    [SerializeField] private int woodRequired = 5;
    [SerializeField] private int stoneRequired = 5;

    [Header("Equipment & Spawns")]
    [SerializeField] private GameObject crossGameObject;      // GameObject tanda silang (cross) di scene
    [SerializeField] private GameObject pickaxePickupGameObject; // Objek alat pemecah batu di ujung map
    [SerializeField] private float interactionDistance = 2f;

    [Header("UI PopUp Settings")]
    [SerializeField] private GameObject getCangkulPanel;     // Panel popup "Mendapatkan Cangkul!"
    [SerializeField] private GameObject getTreasurePanel;    // Panel popup "Dapat Harta Karun!"
    [SerializeField] private GameObject getBambooKeyPanel;   // Panel popup "Dapat Kunci Bambu!"

    [Header("Dig Settings")]
    [SerializeField] private string digAnimationTrigger = "dig"; // Parameter trigger di Animator Player untuk menggali
    [SerializeField] private float delayBetweenDigHits = 0.6f;
    [SerializeField] private int totalDigHits = 7;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip cangkulReceiveSound; // Suara saat mendapatkan cangkul
    [SerializeField] private AudioClip digSound;            // Suara saat cangkul menghantam tanah
    [SerializeField] private AudioClip treasureGetSound;     // Suara saat mendapatkan peti/kunci
    private AudioSource audioSource;

    private bool isPlayerInNPCRange = false;
    private bool isRunningSequence = false;
    private Transform currentTarget = null;
    private int currentDigHits = 0;

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
        // PENGAMAN: Inisialisasi status setelah scene diload
        if (GameManager.Instance != null)
        {
            bool axeCollected = GameManager.Instance.IsFlagSet("chapter3_axe_collected");
            bool talkedPostAxe = GameManager.Instance.IsFlagSet("chapter4_npc_talked_post_axe");
            bool crossReached = GameManager.Instance.IsFlagSet("chapter4_cross_reached");
            bool pickaxeCollected = GameManager.Instance.IsFlagSet("chapter4_pickaxe_collected");
            bool cangkulReceived = GameManager.Instance.IsFlagSet("chapter4_cangkul_received");
            bool dugTreasure = GameManager.Instance.IsFlagSet("chapter4_dug_treasure");

            if (dugTreasure)
            {
                // Jika chapter 4 selesai (harta digali), jangan sembunyikan NPC di sini karena objeknya sama dengan Chapter 5 (Chapter 5 yang akan mengontrol keaktifannya)
                if (pickaxePickupGameObject != null) pickaxePickupGameObject.SetActive(false);
                if (crossGameObject != null) crossGameObject.SetActive(false);

                // Panggil SetupChapter4Quests HANYA jika Chapter 5 belum mengambil alih
                bool chapter5Active = GameManager.Instance.IsFlagSet("chapter5_intro_done");
                if (!chapter5Active)
                {
                    SetupChapter4Quests();
                }
            }
            else if (cangkulReceived)
            {
                // Jika baru cangkul yang diterima, NPC masih berada di gubug
                if (npcGameObject != null)
                {
                    npcGameObject.SetActive(true);
                    if (npcHutPoint != null) npcGameObject.transform.position = npcHutPoint.position;
                }
                if (pickaxePickupGameObject != null) pickaxePickupGameObject.SetActive(false);
                if (crossGameObject != null) crossGameObject.SetActive(true);

                // Panggil SetupChapter4Quests agar misi terisi di UI setelah load scene
                SetupChapter4Quests();

                // Pantau jarak player: jika menjauh > 15 unit, sembunyikan NPC Chapter 4 dari gubug
                StartCoroutine(HideNPCOncePlayerLeaves());
            }
            else if (talkedPostAxe)
            {
                // Misi mencari kayu/batu sedang berjalan
                if (npcGameObject != null)
                {
                    npcGameObject.SetActive(true);
                    // Pindahkan NPC ke gubug menunggu player kembali
                    if (npcHutPoint != null) npcGameObject.transform.position = npcHutPoint.position;
                }
                
                // Panggil SetupChapter4Quests agar misi terisi di UI setelah load scene
                SetupChapter4Quests();
                
                // Atur keaktifan tanda silang & pickaxe berdasarkan state eksplorasi
                if (crossGameObject != null)
                {
                    crossGameObject.SetActive(!crossReached);
                }
                if (pickaxePickupGameObject != null)
                {
                    pickaxePickupGameObject.SetActive(crossReached && !pickaxeCollected);
                }
            }
            else if (axeCollected)
            {
                // Player baru saja mengambil kapak, posisikan NPC di tempat berbeda untuk interaksi baru
                SpawnNPCPostAxe();
                if (pickaxePickupGameObject != null) pickaxePickupGameObject.SetActive(false);
                if (crossGameObject != null) crossGameObject.SetActive(false);
            }
            else
            {
                // Chapter 3 belum selesai, sembunyikan NPC, pickaxe, dan cross
                if (npcGameObject != null) npcGameObject.SetActive(false);
                if (pickaxePickupGameObject != null) pickaxePickupGameObject.SetActive(false);
                if (crossGameObject != null) crossGameObject.SetActive(false);
            }

            // Atur panah petunjuk arah di awal berdasarkan status saat ini
            UpdateObjectivePointer();
        }
    }

    private void Update()
    {
        if (npcGameObject != null && npcGameObject.activeSelf)
        {
            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null)
            {
                float dist = Vector2.Distance(player.transform.position, npcGameObject.transform.position);
                bool inRange = dist <= interactionDistance;

                if (inRange != isPlayerInNPCRange)
                {
                    isPlayerInNPCRange = inRange;
                    if (isPlayerInNPCRange)
                    {
                        if (InteractionPromptUI.Instance != null && !IsDialogueActive() && !isRunningSequence)
                        {
                            InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk bicara");
                        }
                    }
                    else
                    {
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
            if (isPlayerInNPCRange)
            {
                isPlayerInNPCRange = false;
                if (InteractionPromptUI.Instance != null)
                {
                    InteractionPromptUI.Instance.HidePrompt();
                }
            }
        }

        // Selalu update UI quest jika Chapter 4 sedang aktif
        if (GameManager.Instance != null && GameManager.Instance.IsFlagSet("chapter4_quest_active") && !GameManager.Instance.IsFlagSet("chapter4_cangkul_received"))
        {
            UpdateQuestStatus();
        }
    }

    // Dipanggil oleh AxePickupTrigger.OnAxeCollected
    public void OnAxeCollectedTriggered()
    {
        SpawnNPCPostAxe();
        UpdateObjectivePointer();
    }

    private void SpawnNPCPostAxe()
    {
        if (npcGameObject != null)
        {
            npcGameObject.SetActive(true);
            if (npcPostAxePoint != null)
            {
                npcGameObject.transform.position = npcPostAxePoint.position;
            }
            // Pastikan alpha NPC terlihat
            SpriteRenderer sr = npcGameObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                sr.color = new Color(c.r, c.g, c.b, 1f);
            }
            Debug.Log("[Chapter4StoryManager] NPC berpindah ke posisi post-axe.");
        }
    }

    private void HandleInteraction()
    {
        if (!isPlayerInNPCRange || isRunningSequence || IsDialogueActive()) return;

        if (GameManager.Instance == null) return;

        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt();
        }

        bool talkedPostAxe = GameManager.Instance.IsFlagSet("chapter4_npc_talked_post_axe");
        bool cangkulReceived = GameManager.Instance.IsFlagSet("chapter4_cangkul_received");

        if (cangkulReceived)
        {
            // NPC di gubug hanya dialog santai setelah quest selesai
            return;
        }

        if (!talkedPostAxe)
        {
            // Interaksi pertama setelah ambil kapak (Dialog 2 arah & aktivasi Chapter 4)
            StartCoroutine(PlayPostAxeSequence());
        }
        else
        {
            // Interaksi saat quest mencari kayu dan batu berjalan
            int currentWood = 0;
            int currentStone = 0;
            if (InventoryManager.Instance != null)
            {
                currentWood = InventoryManager.Instance.GetItemCount("kayu");
                currentStone = InventoryManager.Instance.GetItemCount("batu");
            }

            if (currentWood >= woodRequired && currentStone >= stoneRequired)
            {
                // Barang cukup, serahkan dan beri Cangkul
                StartCoroutine(PlayHandoverSequence());
            }
            else
            {
                // Barang kurang, beri dialog pengingat
                StartCoroutine(PlayDialogueOnly(missingItemsDialogue));
            }
        }
    }

    private IEnumerator PlayPostAxeSequence()
    {
        isRunningSequence = true;

        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null)
        {
            player.StopMovement();
            player.ToggleInput(false);
        }

        // 1. Dialog 2 Arah
        if (postAxeDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(postAxeDialogue);
            yield return new WaitForSeconds(0.1f);
            while (IsDialogueActive()) yield return null;
        }

        // 2. Tandai status bertemu
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag("chapter4_npc_talked_post_axe", true);
            GameManager.Instance.SetFlag("chapter4_quest_active", true);
        }

        // 3. Setup daftar misi di ToDoManager
        SetupChapter4Quests();

        // 4. Mulai memantau jarak untuk memindahkan NPC setelah player menjauh
        StartCoroutine(MoveNPCOncePlayerLeaves());

        // 5. Aktifkan tanda silang di map (bukan pickaxe secara langsung)
        if (crossGameObject != null)
        {
            crossGameObject.SetActive(true);
        }
        if (pickaxePickupGameObject != null)
        {
            pickaxePickupGameObject.SetActive(false);
        }

        // 6. Update pointer untuk mengarah ke tanda silang
        UpdateObjectivePointer();

        if (player != null) player.ToggleInput(true);
        isRunningSequence = false;
    }

    private IEnumerator MoveNPCOncePlayerLeaves()
    {
        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player == null) yield break;

        // Tunggu sampai jarak player dengan NPC cukup jauh (di luar layar / viewport, misal > 12 unit)
        float leaveDistance = 15f;
        while (npcGameObject != null && npcGameObject.activeSelf)
        {
            float dist = Vector2.Distance(player.transform.position, npcGameObject.transform.position);
            if (dist > leaveDistance)
            {
                // Pindahkan NPC ke gubug
                if (npcHutPoint != null)
                {
                    npcGameObject.transform.position = npcHutPoint.position;
                    Debug.Log("[Chapter4StoryManager] Player sudah menjauh. NPC dipindahkan ke gubug.");
                }
                yield break;
            }
            yield return new WaitForSeconds(0.5f); // Periksa setiap 0.5 detik untuk menghemat CPU
        }
    }

    private IEnumerator HideNPCOncePlayerLeaves()
    {
        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player == null) yield break;

        float leaveDistance = 15f;
        while (npcGameObject != null && npcGameObject.activeSelf)
        {
            // PENGAMAN: Jika sudah masuk ke Chapter 5 (harta digali), serahkan kontrol NPC ke Chapter 5
            if (GameManager.Instance != null && GameManager.Instance.IsFlagSet("chapter4_dug_treasure"))
            {
                yield break;
            }

            float dist = Vector2.Distance(player.transform.position, npcGameObject.transform.position);
            if (dist > leaveDistance)
            {
                npcGameObject.SetActive(false);
                Debug.Log("[Chapter4StoryManager] Player sudah menjauh dari gubug. NPC Opung Chapter 4 disembunyikan.");
                yield break;
            }
            yield return new WaitForSeconds(0.5f); // Periksa setiap 0.5 detik
        }
    }

    private IEnumerator PlayHandoverSequence()
    {
        isRunningSequence = true;

        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null)
        {
            player.StopMovement();
            player.ToggleInput(false);
        }

        // 1. Dialog penyerahan
        if (handoverDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(handoverDialogue);
            yield return new WaitForSeconds(0.1f);
            while (IsDialogueActive()) yield return null;
        }

        // 2. Ambil barang dari inventory & beri Cangkul ke Slot Equipment
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem("kayu", woodRequired);
            InventoryManager.Instance.RemoveItem("batu", stoneRequired);
            
            // Mengisi cangkul ke slot equipment ke-3
            InventoryManager.Instance.EquipCangkul();
        }

        // Putar efek suara reward Cangkul
        if (audioSource != null && cangkulReceiveSound != null)
        {
            audioSource.PlayOneShot(cangkulReceiveSound);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag("chapter4_cangkul_received", true);
        }

        // 3. Tampilkan panel "Mendapatkan Cangkul!" terlebih dahulu (jika ada)
        if (getCangkulPanel != null)
        {
            getCangkulPanel.SetActive(true);

            // Beri jeda sangat kecil agar tombol "E" penutup dialog tidak langsung menutup panel
            yield return new WaitForSeconds(0.2f);

            bool panelClosed = false;
            while (!panelClosed)
            {
                if (UnityEngine.InputSystem.Keyboard.current != null &&
                    (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame ||
                     UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame ||
                     UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame))
                {
                    panelClosed = true;
                }
                yield return null;
            }

            getCangkulPanel.SetActive(false);
        }

        // 4. Baru putar dialog setelah menerima cangkul (dialog penutup)
        if (cangkulReceivedDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(cangkulReceivedDialogue);
            yield return new WaitForSeconds(0.1f);
            while (IsDialogueActive()) yield return null;
        }

        // Aktifkan kembali tanda silang untuk menggali
        if (crossGameObject != null)
        {
            crossGameObject.SetActive(true);
        }

        // Update Quest UI agar selesai / diperbarui
        UpdateQuestStatus();

        if (player != null) player.ToggleInput(true);
        isRunningSequence = false;

        // Mulai memantau jarak untuk menyembunyikan NPC setelah player menjauh dari gubug
        StartCoroutine(HideNPCOncePlayerLeaves());
    }

    private IEnumerator PlayDialogueOnly(Dialogue dialogue)
    {
        isRunningSequence = true;
        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null)
        {
            player.StopMovement();
            player.ToggleInput(false);
        }

        if (dialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(dialogue);
            yield return new WaitForSeconds(0.1f);
            while (IsDialogueActive()) yield return null;
        }

        if (player != null) player.ToggleInput(true);
        isRunningSequence = false;
    }

    private void SetupChapter4Quests()
    {
        if (ToDoManager.Instance == null) return;

        // Jangan timpa misi jika chapter yang aktif sudah lebih maju dari Chapter_4
        if (!ToDoManager.Instance.IsChapterAtLeast("Chapter_4")) return;
        // Juga jangan timpa jika sudah di Chapter_5
        if (ToDoManager.Instance.currentChapterID == "Chapter_5") return;

        ToDoManager.Instance.SetCurrentChapterID("Chapter_4");
        ToDoManager.Instance.daftarMisi = new List<Quest>
        {
            new Quest { namaMisi = "Periksa area misterius (tanda silang)", sudahSelesai = false },
            new Quest { namaMisi = "Cari alat pemecah batu di ujung map", sudahSelesai = false },
            new Quest { namaMisi = $"Tebang pohon untuk mengumpulkan Kayu (0/{woodRequired})", sudahSelesai = false },
            new Quest { namaMisi = $"Pecahkan batu untuk mengumpulkan Batu (0/{stoneRequired})", sudahSelesai = false },
            new Quest { namaMisi = "Serahkan Kayu dan Batu ke Opung di Gubug", sudahSelesai = false },
            new Quest { namaMisi = "Gali tanah di tanda silang menggunakan Cangkul", sudahSelesai = false }
        };
        ToDoManager.Instance.UpdateTampilanUI();
    }

    public void UpdateQuestStatus()
    {
        if (ToDoManager.Instance != null && ToDoManager.Instance.currentChapterID == "Chapter_4")
        {
            int currentWood = 0;
            int currentStone = 0;
            bool hasPickaxe = false;
            bool crossReached = false;
            bool dugTreasure = false;

            if (GameManager.Instance != null)
            {
                crossReached = GameManager.Instance.IsFlagSet("chapter4_cross_reached");
                hasPickaxe = GameManager.Instance.IsFlagSet("chapter4_pickaxe_collected");
                dugTreasure = GameManager.Instance.IsFlagSet("chapter4_dug_treasure");
            }

            if (InventoryManager.Instance != null)
            {
                currentWood = InventoryManager.Instance.GetItemCount("kayu");
                currentStone = InventoryManager.Instance.GetItemCount("batu");
                if (InventoryManager.Instance.hasEquipment2) hasPickaxe = true;
            }

            ToDoManager.Instance.daftarMisi[0].namaMisi = "Periksa area misterius (tanda silang)";
            ToDoManager.Instance.daftarMisi[0].sudahSelesai = crossReached;

            ToDoManager.Instance.daftarMisi[1].namaMisi = "Cari alat pemecah batu di ujung map";
            ToDoManager.Instance.daftarMisi[1].sudahSelesai = hasPickaxe;

            bool isDone = GameManager.Instance.IsFlagSet("chapter4_cangkul_received");

            ToDoManager.Instance.daftarMisi[2].namaMisi = $"Tebang pohon untuk mengumpulkan Kayu ({(isDone ? woodRequired : currentWood)}/{woodRequired})";
            ToDoManager.Instance.daftarMisi[2].sudahSelesai = (currentWood >= woodRequired) || isDone;

            ToDoManager.Instance.daftarMisi[3].namaMisi = $"Pecahkan batu untuk mengumpulkan Batu ({(isDone ? stoneRequired : currentStone)}/{stoneRequired})";
            ToDoManager.Instance.daftarMisi[3].sudahSelesai = (currentStone >= stoneRequired) || isDone;

            ToDoManager.Instance.daftarMisi[4].namaMisi = "Serahkan Kayu dan Batu ke Opung di Gubug";
            ToDoManager.Instance.daftarMisi[4].sudahSelesai = isDone;

            ToDoManager.Instance.daftarMisi[5].namaMisi = "Gali tanah di tanda silang menggunakan Cangkul";
            ToDoManager.Instance.daftarMisi[5].sudahSelesai = dugTreasure;

            ToDoManager.Instance.UpdateTampilanUI();
        }

        // Perbarui target penunjuk arah jika status misi berubah
        UpdateObjectivePointer();
    }

    public void OnCrossReached()
    {
        StartCoroutine(PlayCrossReachedSequence());
    }

    private IEnumerator PlayCrossReachedSequence()
    {
        isRunningSequence = true;

        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null)
        {
            player.StopMovement();
            player.ToggleInput(false);
        }

        // 1. Putar dialog tanda silang
        if (crossDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(crossDialogue);
            yield return new WaitForSeconds(0.1f);
            while (IsDialogueActive()) yield return null;
        }

        // 2. Set flag di GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag("chapter4_cross_reached", true);
        }

        // 3. Tanda silang dibiarkan tetap aktif agar player ingat lokasinya


        // 4. Aktifkan alat pemecah batu
        if (pickaxePickupGameObject != null)
        {
            pickaxePickupGameObject.SetActive(true);
        }

        // 5. Update status misi & pointer
        UpdateQuestStatus();

        if (player != null) player.ToggleInput(true);
        isRunningSequence = false;
    }

    public void StartDigging()
    {
        if (isRunningSequence) return;
        StartCoroutine(PlaySingleDigHitCoroutine());
    }

    private IEnumerator PlaySingleDigHitCoroutine()
    {
        isRunningSequence = true;
        currentDigHits++;

        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null)
        {
            player.StopMovement();
            player.ToggleInput(false);

            // Kunci arah hadap player agar menghadap ke arah tanda silang saat menggali
            if (crossGameObject != null)
            {
                bool crossIsOnLeft = crossGameObject.transform.position.x < player.transform.position.x;
                player.LockFacingDirection(crossIsOnLeft, delayBetweenDigHits);
            }
        }

        // Putar animasi menggali sekali
        Animator animator = player != null ? player.GetComponent<Animator>() : null;
        if (animator != null && !string.IsNullOrEmpty(digAnimationTrigger))
        {
            animator.ResetTrigger(digAnimationTrigger);
            animator.SetTrigger(digAnimationTrigger);
        }

        // Putar audio galian
        if (audioSource != null && digSound != null)
        {
            audioSource.PlayOneShot(digSound);
        }

        // Tunggu durasi animasi galian selesai sebelum mengizinkan input lagi
        yield return new WaitForSeconds(delayBetweenDigHits);

        if (currentDigHits >= totalDigHits)
        {
            // Pemicu reward setelah 7 kali gali selesai
            yield return StartCoroutine(PlayDigRewardSequence());
        }
        else
        {
            // Jika belum 7 kali, kembalikan input player agar bisa menekan E lagi
            if (player != null) player.ToggleInput(true);
            isRunningSequence = false;
        }
    }

    private IEnumerator PlayDigRewardSequence()
    {
        var player = FindAnyObjectByType<PlayerControllerScript>();

        // 2. Set flag dug_treasure & kunci_bambu
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag("chapter4_dug_treasure", true);
            GameManager.Instance.SetFlag("chapter4_bamboo_key_received", true);
        }

        // Putar audio ketika berhasil mendapatkan peti/kunci
        if (audioSource != null && treasureGetSound != null)
        {
            audioSource.PlayOneShot(treasureGetSound);
        }

        // 3. Masukkan kunci bambu ke Inventory
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem("kunci_bambu", 1);
        }

        // 4. Sembunyikan tanda silang secara permanen karena berpindah ke Chapter 5
        if (crossGameObject != null)
        {
            crossGameObject.SetActive(false);
        }
        // JANGAN nonaktifkan npcGameObject (Opung) di sini karena objeknya sama dengan Chapter 5

        // 5. Tampilkan panel "Dapat Harta Karun!"
        if (getTreasurePanel != null)
        {
            getTreasurePanel.SetActive(true);
            yield return new WaitForSeconds(0.2f); // Beri jeda kecil agar input tidak tertumpuk

            bool panelClosed = false;
            while (!panelClosed)
            {
                if (UnityEngine.InputSystem.Keyboard.current != null &&
                    (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame ||
                     UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame ||
                     UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame))
                {
                    panelClosed = true;
                }
                yield return null;
            }
            getTreasurePanel.SetActive(false);
        }

        // 6. Tampilkan panel "Dapat Kunci Bambu!"
        if (getBambooKeyPanel != null)
        {
            getBambooKeyPanel.SetActive(true);
            yield return new WaitForSeconds(0.2f); // Beri jeda kecil

            bool panelClosed = false;
            while (!panelClosed)
            {
                if (UnityEngine.InputSystem.Keyboard.current != null &&
                    (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame ||
                     UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame ||
                     UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame))
                {
                    panelClosed = true;
                }
                yield return null;
            }
            getBambooKeyPanel.SetActive(false);
        }

        // 7. Putar dialog penutup
        if (postDigDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(postDigDialogue);
            yield return new WaitForSeconds(0.1f);
            while (IsDialogueActive()) yield return null;
        }

        // 8. Update status misi & pointer
        UpdateQuestStatus();

        // 9. Pemicu transisi ke Chapter 5 secara real-time jika ada di scene
        if (Chapter5StoryManager.Instance != null)
        {
            Chapter5StoryManager.Instance.StartChapter5();
        }

        if (MemoryShardManager.Instance != null)
        {
            MemoryShardManager.Instance.UnlockShard("Chapter4");
        }

        if (player != null) player.ToggleInput(true);
        isRunningSequence = false;
    }

    public void UpdateObjectivePointer()
    {
        if (ObjectivePointer.Instance == null || GameManager.Instance == null) return;

        bool talkedPostAxe = GameManager.Instance.IsFlagSet("chapter4_npc_talked_post_axe");
        bool crossReached = GameManager.Instance.IsFlagSet("chapter4_cross_reached");
        bool pickaxeCollected = GameManager.Instance.IsFlagSet("chapter4_pickaxe_collected");
        bool cangkulReceived = GameManager.Instance.IsFlagSet("chapter4_cangkul_received");
        bool dugTreasure = GameManager.Instance.IsFlagSet("chapter4_dug_treasure");

        Transform newTarget = null;

        if (dugTreasure)
        {
            newTarget = null;
        }
        else if (cangkulReceived)
        {
            // Arahkan kembali ke tanda silang untuk menggali
            if (crossGameObject != null) newTarget = crossGameObject.transform;
        }
        else if (pickaxeCollected)
        {
            if (npcGameObject != null) newTarget = npcGameObject.transform;
        }
        else if (crossReached)
        {
            if (pickaxePickupGameObject != null) newTarget = pickaxePickupGameObject.transform;
        }
        else if (talkedPostAxe)
        {
            if (crossGameObject != null) newTarget = crossGameObject.transform;
        }
        else
        {
            if (npcGameObject != null && npcGameObject.activeSelf) newTarget = npcGameObject.transform;
        }

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

    public bool IsAnyPanelActive()
    {
        return (getCangkulPanel != null && getCangkulPanel.activeSelf) ||
               (getTreasurePanel != null && getTreasurePanel.activeSelf) ||
               (getBambooKeyPanel != null && getBambooKeyPanel.activeSelf);
    }
}

