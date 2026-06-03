using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// --- SLOT PUZZLE COMPONENT ---
public class PuzzleSlot : MonoBehaviour, IDropHandler
{
    [Tooltip("ID item yang benar untuk slot ini (misal: sisik_part_0, sisik_part_1, dll.)")]
    public string correctItemID;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        DraggableItem draggable = dropped.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            // Jika slot ini masih kosong, pindahkan item ke dalam slot
            if (transform.childCount == 0)
            {
                draggable.parentAfterDrag = transform;
                dropped.transform.SetParent(transform);
                
                // Posisikan tepat di tengah slot
                RectTransform droppedRect = dropped.GetComponent<RectTransform>();
                if (droppedRect != null)
                {
                    droppedRect.anchoredPosition = Vector2.zero;
                }
                
                Debug.Log($"Item '{draggable.itemID}' diletakkan di Slot dengan target '{correctItemID}'");
            }
        }
    }
}

// --- MAIN PUZZLE MANAGER ---
public class SisikPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Slots")]
    [SerializeField] private PuzzleSlot[] slots; // List 7 slot penyusunan

    [Header("Draggable Scales")]
    [SerializeField] private DraggableItem[] draggableItems; // List 7 item sisik yang dapat diseret

    [Header("UI Panels")]
    [SerializeField] private GameObject puzzlePanel;

    [Header("Events On Success")]
    [SerializeField] private UnityEvent OnPuzzleSolved; // Aksi setelah puzzle berhasil dipecahkan

    [Header("Dialogue (Optional)")]
    [SerializeField] private Dialogue failDialogue; // Dialog saat susunan masih salah

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

        // Nyalakan kembali input player
        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null) player.ToggleInput(true);

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

    public void ResetAllDraggables()
    {
        foreach (DraggableItem item in draggableItems)
        {
            if (item != null)
            {
                // Kembalikan ke parent aslinya (kembali keluar dari slot)
                item.transform.SetParent(this.transform); // Set parent ke panel asalnya
                item.ResetToStart();
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

        // Nyalakan kembali input player
        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null) player.ToggleInput(true);
    }
}
