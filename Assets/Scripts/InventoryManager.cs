using UnityEngine;
using UnityEngine.UI; // Wajib ada untuk memanipulasi komponen Image

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject inventoryPanel; // Papan besar inventory (diaktifkan pakai tombol I)
    
    [Header("Item Slots")]
    [SerializeField] private GameObject fishingRodSlotImage; // Gambar/Icon Kail Pancing di dalam UI Inventory

    // Status item 
    public bool hasFishingRod { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        PlayerControllerScript.OnInventoryPressed += ToggleInventory;
    }

    private void OnDisable()
    {
        PlayerControllerScript.OnInventoryPressed -= ToggleInventory;
    }

    // Fungsi mengambil kail pancing
    public void GetFishingRod()
    {
        hasFishingRod = true;
        UpdateInventoryUI(); // Update tampilan UI biar kailnya muncul
        Debug.Log("Kail Pancing masuk ke inventory!");
    }

    // Fungsi menggunakan kail pancing
    public void UseFishingRod()
    {
        hasFishingRod = false;
        UpdateInventoryUI(); // Update tampilan UI biar kailnya hilang
        Debug.Log("Kail Pancing telah digunakan!");
    }

    private void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            // Buka atau tutup panel utama saat tombol I ditekan
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);

            // Jika panelnya sedang dibuka, sekalian update isi slotnya
            if (inventoryPanel.activeSelf)
            {
                UpdateInventoryUI();
            }
        }
    }

    // --- FUNGSI BARU: Mengatur muncul/hilangnya item di UI ---
    private void UpdateInventoryUI()
    {
        if (fishingRodSlotImage != null)
        {
            // Jika player punya kail pancing, gambar icon kail AKTIF. Jika tidak, gambar kail MATI.
            fishingRodSlotImage.SetActive(hasFishingRod);
        }
    }
}