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

    [Header("Gate Settings")]
    [SerializeField] private DoorController targetGate; // Pintu/Gate yang terbuka setelah puzzle selesai
    [SerializeField] private BarangTrigger barangTrigger; // Objek trigger sisik di scene

    [Header("Events On Success")]
    [SerializeField] private UnityEvent OnPuzzleSolved; // Aksi setelah puzzle berhasil dipecahkan

    [Header("Dialogue (Optional)")]
    [SerializeField] private Dialogue failDialogue; // Dialog saat susunan masih salah

    public void OpenPuzzle(int collectedCount)
    {
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
            HandleSuccess();
        }
        else
        {
            Debug.Log("Susunan sisik masih salah atau belum lengkap!");
            HandleFailure();
        }
    }

    private void HandleSuccess()
    {
        // Tutup panel puzzle
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        // Tampilkan panel dapat kunci
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

        // Jika tidak ada panel reward, langsung kembalikan input player
        if (rewardPanel == null)
        {
            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null) player.ToggleInput(true);
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
        bool isAnyPanelActive = (puzzlePanel != null && puzzlePanel.activeSelf) || (rewardPanel != null && rewardPanel.activeSelf);
        if (isAnyPanelActive)
        {
            // Cek tombol ESC menggunakan New Input System
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ClosePuzzlePanel();
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

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }

        // Nyalakan kembali input player
        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null) player.ToggleInput(true);
    }
}
