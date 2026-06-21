using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chapter4StoryManager : MonoBehaviour
{
    public static Chapter4StoryManager Instance { get; private set; }

    [Header("NPC Configuration")]
    [SerializeField] private GameObject npcGameObject;
    [SerializeField] private Transform npcPostAxePoint; // Titik NPC muncul setelah Kapak diambil
    [SerializeField] private Transform npcHutPoint;     // Titik NPC kembali di Gubug

    [Header("Dialogue Configurations")]
    [SerializeField] private Dialogue postAxeDialogue;        // Dialog awal Chapter 4 saat ketemu NPC
    [SerializeField] private Dialogue missingItemsDialogue;   // Dialog jika barang belum lengkap
    [SerializeField] private Dialogue handoverDialogue;       // Dialog penyerahan kayu & batu
    [SerializeField] private Dialogue cangkulReceivedDialogue; // Dialog setelah menerima cangkul

    [Header("Quest Requirements")]
    [SerializeField] private int woodRequired = 5;
    [SerializeField] private int stoneRequired = 5;

    [Header("Equipment & Spawns")]
    [SerializeField] private GameObject pickaxePickupGameObject; // Objek alat pemecah batu di ujung map
    [SerializeField] private float interactionDistance = 2f;

    [Header("UI PopUp Settings")]
    [SerializeField] private GameObject getCangkulPanel; // Panel popup "Mendapatkan Cangkul!"

    [Header("Audio Settings")]
    [SerializeField] private AudioClip cangkulReceiveSound; // Suara saat mendapatkan cangkul
    private AudioSource audioSource;

    private bool isPlayerInNPCRange = false;
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
            bool cangkulReceived = GameManager.Instance.IsFlagSet("chapter4_cangkul_received");

            if (cangkulReceived)
            {
                // Jika chapter 4 selesai, sembunyikan NPC atau letakkan di gubug sesuai status
                if (npcGameObject != null)
                {
                    npcGameObject.SetActive(true);
                    if (npcHutPoint != null) npcGameObject.transform.position = npcHutPoint.position;
                }
                if (pickaxePickupGameObject != null) pickaxePickupGameObject.SetActive(false);
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
                if (pickaxePickupGameObject != null && !GameManager.Instance.IsFlagSet("chapter4_pickaxe_collected"))
                {
                    pickaxePickupGameObject.SetActive(true);
                }
            }
            else if (axeCollected)
            {
                // Player baru saja mengambil kapak, posisikan NPC di tempat berbeda untuk interaksi baru
                SpawnNPCPostAxe();
            }
            else
            {
                // Chapter 3 belum selesai, sembunyikan NPC dan pickaxe
                if (npcGameObject != null) npcGameObject.SetActive(false);
                if (pickaxePickupGameObject != null) pickaxePickupGameObject.SetActive(false);
            }
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

        // 5. Aktifkan alat pemecah batu di ujung map
        if (pickaxePickupGameObject != null)
        {
            pickaxePickupGameObject.SetActive(true);
        }

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

        // Update Quest UI agar selesai
        UpdateQuestStatus();

        if (player != null) player.ToggleInput(true);
        isRunningSequence = false;
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
        if (ToDoManager.Instance != null)
        {
            ToDoManager.Instance.currentChapterID = "Chapter_4";
            ToDoManager.Instance.daftarMisi = new List<Quest>
            {
                new Quest { namaMisi = $"Tebang pohon untuk mengumpulkan Kayu (0/{woodRequired})", sudahSelesai = false },
                new Quest { namaMisi = "Cari alat pemecah batu di ujung map", sudahSelesai = false },
                new Quest { namaMisi = $"Pecahkan batu untuk mengumpulkan Batu (0/{stoneRequired})", sudahSelesai = false },
                new Quest { namaMisi = "Serahkan Kayu dan Batu ke NPC di Gubug", sudahSelesai = false }
            };
            ToDoManager.Instance.UpdateTampilanUI();
        }
    }

    public void UpdateQuestStatus()
    {
        if (ToDoManager.Instance != null && ToDoManager.Instance.currentChapterID == "Chapter_4")
        {
            int currentWood = 0;
            int currentStone = 0;
            bool hasPickaxe = false;

            if (InventoryManager.Instance != null)
            {
                currentWood = InventoryManager.Instance.GetItemCount("kayu");
                currentStone = InventoryManager.Instance.GetItemCount("batu");
                hasPickaxe = InventoryManager.Instance.hasEquipment2;
            }

            ToDoManager.Instance.daftarMisi[0].namaMisi = $"Tebang pohon untuk mengumpulkan Kayu ({currentWood}/{woodRequired})";
            ToDoManager.Instance.daftarMisi[0].sudahSelesai = (currentWood >= woodRequired);

            ToDoManager.Instance.daftarMisi[1].namaMisi = "Cari alat pemecah batu di ujung map";
            ToDoManager.Instance.daftarMisi[1].sudahSelesai = hasPickaxe || GameManager.Instance.IsFlagSet("chapter4_pickaxe_collected");

            ToDoManager.Instance.daftarMisi[2].namaMisi = $"Pecahkan batu untuk mengumpulkan Batu ({currentStone}/{stoneRequired})";
            ToDoManager.Instance.daftarMisi[2].sudahSelesai = (currentStone >= stoneRequired);

            bool isDone = GameManager.Instance.IsFlagSet("chapter4_cangkul_received");
            ToDoManager.Instance.daftarMisi[3].namaMisi = "Serahkan Kayu dan Batu ke NPC di Gubug";
            ToDoManager.Instance.daftarMisi[3].sudahSelesai = isDone;

            ToDoManager.Instance.UpdateTampilanUI();
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
