using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// --- MAIN PUZZLE MANAGER ---
public class SisikPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Slots")]
    [SerializeField] private PuzzleSlot[] slots; // List 7 slot penyusunan

    [Header("Draggable Scales")]
    [SerializeField] private DraggableItem[] draggableItems; // List 7 item sisik yang dapat diseret

    [Header("UI Panels")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject submitButton; // Tombol periksa / submit
    [SerializeField] private GameObject rewardPanel; // Panel "Dapat Kunci"
    [SerializeField] private GameObject reopenPanel; // Panel re open

    [Header("Gate Settings")]
    [SerializeField] private DoorController targetGate; // Pintu/Gate yang terbuka setelah puzzle selesai
    [SerializeField] private BarangTrigger barangTrigger; // Objek trigger sisik di scene

    [Header("Events On Success")]
    [SerializeField] private UnityEvent OnPuzzleSolved; // Aksi setelah puzzle berhasil dipecahkan

    [Header("Dialogue (Optional)")]
    [SerializeField] private Dialogue failDialogue; // Dialog saat susunan masih salah
    [SerializeField] private Dialogue successDialogue; // Dialog setelah puzzle selesai & reward panel ditutup
    [SerializeField] private VNDialogueData successVNDialogue; // VN Dialog setelah puzzle selesai (prioritas di atas Dialogue lama)

    [Header("Memory Shard (Optional)")]
    [SerializeField] private string rewardMemoryShardID; // Jika diisi, memory shard ini akan terbuka setelah puzzle/dialog selesai

    [Header("Audio Settings")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip wrongPasswordSound;
    [SerializeField] private AudioClip rewardSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    private bool isReopeningSolved = false;

    public void OpenPuzzle(int collectedCount)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsFlagSet("sisik_puzzle_solved"))
        {
            isReopeningSolved = true;
            if (reopenPanel != null)
            {
                reopenPanel.SetActive(true);
                // Matikan pergerakan player
                var player = FindAnyObjectByType<PlayerControllerScript>();
                if (player != null) player.ToggleInput(false);
            }
            return;
        }

        isReopeningSolved = false;

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
            
            // Matikan pergerakan player
            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null) player.ToggleInput(false);

            // Aktifkan draggable item sesuai jumlah yang dikoleksi
            for (int i = 0; i < draggableItems.Length; i++)
            {
                if (draggableItems[i] != null)
                {
                    draggableItems[i].gameObject.SetActive(i < collectedCount);
                }
            }

            // Tampilkan tombol submit hanya jika sisik lengkap (>= 7)
            if (submitButton != null)
            {
                submitButton.SetActive(collectedCount >= 7);
            }
        }
    }

    // Panggil fungsi ini lewat button "Periksa" atau "Selesai" di UI
    public void CheckSolution()
    {
        bool allCorrect = true;

        foreach (PuzzleSlot slot in slots)
        {
            // Jika slot kosong
            if (slot.transform.childCount == 0)
            {
                allCorrect = false;
                break;
            }

            // Ambil script DraggableItem dari anak slot
            DraggableItem placedItem = slot.GetComponentInChildren<DraggableItem>();
            if (placedItem == null || placedItem.itemID != slot.correctItemID)
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            Debug.Log("Puzzle Sukses! Sisik tersusun dengan benar.");
            if (audioSource != null && buttonClickSound != null)
            {
                audioSource.PlayOneShot(buttonClickSound);
            }
            HandleSuccess();
            if (ToDoManager.Instance != null)
                {
                    // Angka 1 berarti mencoret misi urutan KEDUA di daftar misi Chapter tersebut
                    ToDoManager.Instance.SelesaikanMisi(2); 
                }
        }
        else
        {
            Debug.Log("Susunan sisik masih salah atau belum lengkap!");
            if (audioSource != null && wrongPasswordSound != null)
            {
                audioSource.PlayOneShot(wrongPasswordSound);
            }
            HandleFailure();
        }
    }

    private void HandleSuccess()
    {
        isPuzzleSolved = true; // Tandai bahwa puzzle sudah selesai

        // Tutup panel puzzle
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        // Tampilkan panel dapat kunci
        if (audioSource != null && rewardSound != null)
        {
            audioSource.PlayOneShot(rewardSound);
        }
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
        }

        // Buka gate/pintu
        if (targetGate != null)
        {
            targetGate.OpenDoor();
        }

        // Matikan efek berkedip pada barang trigger
        if (barangTrigger != null)
        {
            barangTrigger.StopBlink();
        }

        // Simpan status puzzle selesai ke GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag("sisik_puzzle_solved", true);
            // Konsumsi/hapus sisik dari inventory
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RemoveItem("sisik", 7);
            }
        }

        // Jika tidak ada panel reward, langsung kembalikan input player & jalankan dialog
        if (rewardPanel == null)
        {
            var player = FindAnyObjectByType<PlayerControllerScript>();
            
            if (successVNDialogue != null)
            {
                var vnManager = FindAnyObjectByType<DialogueManagerCS>();
                if (vnManager != null)
                {
                    if (player != null) player.ToggleInput(false);
                    vnManager.PlayDialogue(successVNDialogue);
                    StartCoroutine(WaitVNDialogueAndUnlockShard(vnManager, player));
                }
            }
            else if (successDialogue != null && DialogueManager.instance != null)
            {
                if (player != null) player.ToggleInput(false);
                DialogueManager.instance.StartDialogue(successDialogue);
            }
            else
            {
                if (player != null) player.ToggleInput(true);
                UnlockMemoryShardIfAny();
            }
        }

        // Jalankan event sukses
        OnPuzzleSolved?.Invoke();
    }

    private void HandleFailure()
    {
        // Tampilkan dialog gagal jika dipasang
        if (failDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(failDialogue);
        }

        // Kembalikan semua kepingan sisik ke posisi awal penyusunan
        ResetAllDraggables();
    }

    private void Update()
    {
        bool isAnyPanelActive = (puzzlePanel != null && puzzlePanel.activeSelf) || (rewardPanel != null && rewardPanel.activeSelf) || (reopenPanel != null && reopenPanel.activeSelf);
        if (isAnyPanelActive)
        {
            // Cek tombol ESC menggunakan New Input System
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ClosePuzzlePanel();
                return; // Langsung keluar dari fungsi agar tidak tereksekusi ganda
            }

            // JIKA Panel Kunci aktif, izinkan menutup dengan klik mouse, Enter, atau Spasi 
            // (Solusi untuk masalah bentrok ESC)
            if (rewardPanel != null && rewardPanel.activeSelf)
            {
                bool mouseClick = UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;
                bool enterOrSpace = UnityEngine.InputSystem.Keyboard.current != null && 
                                   (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame || 
                                    UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame);

                if (mouseClick || enterOrSpace)
                {
                    ClosePuzzlePanel();
                }
            }

            if (reopenPanel != null && reopenPanel.activeSelf)
            {
                bool mouseClick = UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;
                bool enterOrSpace = UnityEngine.InputSystem.Keyboard.current != null && 
                                   (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame || 
                                    UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame);

                if (mouseClick || enterOrSpace)
                {
                    ClosePuzzlePanel();
                }
            }
        }
        
    }

    public void ResetAllDraggables()
    {
        foreach (DraggableItem item in draggableItems)
        {
            if (item != null)
            {
                // Kembalikan ke parent aslinya dan posisi aslinya
                item.ResetToOriginalState();
            }
        }
    }

    // Panggil untuk keluar dari Puzzle secara manual
    public void ClosePuzzlePanel()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        bool wasRewardActive = rewardPanel != null && rewardPanel.activeSelf;

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }

        if (reopenPanel != null)
        {
            reopenPanel.SetActive(false);
        }

        // TAMBAHKAN INI: Tutup paksa seluruh panel dialog agar tidak menggantung di layar
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.TutupPaksaSeluruhPanel();
        }

        // Nyalakan kembali input player
        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null) player.ToggleInput(true);

        if (wasRewardActive && !isReopeningSolved)
        {
            if (successVNDialogue != null)
            {
                var vnManager = FindAnyObjectByType<DialogueManagerCS>();
                if (vnManager != null)
                {
                    if (player != null) player.ToggleInput(false); // Tahan input saat VN
                    vnManager.PlayDialogue(successVNDialogue);
                    StartCoroutine(WaitVNDialogueAndUnlockShard(vnManager, player));
                }
            }
            else if (successDialogue != null && DialogueManager.instance != null)
            {
                if (player != null) player.ToggleInput(false); // Tahan input saat dialog biasa
                DialogueManager.instance.StartDialogue(successDialogue);
                StartCoroutine(WaitNormalDialogueAndUnlockShard(player));
            }
            else
            {
                UnlockMemoryShardIfAny();
            }
        }
        else
        {
            UnlockMemoryShardIfAny();
        }
    }

    private System.Collections.IEnumerator WaitNormalDialogueAndUnlockShard(PlayerControllerScript player)
    {
        yield return null; // Tunggu satu frame agar dialog sempat aktif

        while (DialogueManager.instance != null && 
               ((DialogueManager.instance.screenBoxPanel != null && DialogueManager.instance.screenBoxPanel.activeInHierarchy) ||
                (DialogueManager.instance.bubblePanel != null && DialogueManager.instance.bubblePanel.activeInHierarchy) ||
                (DialogueManager.instance.cutsceneBoxPanel != null && DialogueManager.instance.cutsceneBoxPanel.activeInHierarchy)))
        {
            yield return null;
        }

        UnlockMemoryShardIfAny();

        if (player != null && (MemoryShardManager.Instance == null || string.IsNullOrEmpty(rewardMemoryShardID)))
        {
            player.ToggleInput(true);
        }
    }

    private System.Collections.IEnumerator WaitVNDialogueAndUnlockShard(DialogueManagerCS vnManager, PlayerControllerScript player)
    {
        // Tunggu 1 frame agar status IsPlaying sempat diperbarui oleh vnManager
        yield return null; 

        // Tunggu sampai dialog benar-benar selesai
        while (vnManager != null && vnManager.IsPlaying)
        {
            yield return null;
        }

        // Dialog selesai, unlock memori shard jika ada
        UnlockMemoryShardIfAny();

        // Kembalikan input player jika tidak tertahan oleh popup memory shard
        if (player != null && (MemoryShardManager.Instance == null || string.IsNullOrEmpty(rewardMemoryShardID)))
        {
            player.ToggleInput(true);
        }
    }

    private bool isPuzzleSolved = false;

    private void UnlockMemoryShardIfAny()
    {
        // Hanya buka memory shard jika puzzle benar-benar SUDAH diselesaikan 
        // (mencegah terbuka tidak sengaja saat panel puzzle biasa ditutup)
        if (isPuzzleSolved && !string.IsNullOrEmpty(rewardMemoryShardID) && MemoryShardManager.Instance != null)
        {
            MemoryShardManager.Instance.UnlockShard(rewardMemoryShardID);
            rewardMemoryShardID = ""; // Kosongkan agar tidak terpicu berkali-kali jika tombol tutup ditekan lagi
        }
    }
}
