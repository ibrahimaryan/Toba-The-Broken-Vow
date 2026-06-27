using UnityEngine;
using UnityEngine.UI;

public class ActiveEquipmentUI : MonoBehaviour
{
    [System.Serializable]
    public struct EquipmentVisual
    {
        public string itemID;             // ID equipment, misal "kapak", "equipment2", "cangkul"
        public Sprite backgroundSprite;   // Sprite background khusus untuk equipment ini
        public Sprite overrideIconSprite; // Sprite icon custom (opsional, jika kosong akan mengambil dari InventoryManager)
    }

    [Header("UI Component References")]
    [SerializeField] private Image backgroundImage;      // Image untuk background panel indikator
    [SerializeField] private Image iconImage;            // Image untuk icon equipment yang sedang dipegang

    [Header("Default Visual Settings")]
    [SerializeField] private Sprite emptyBgSprite;       // Background saat tidak memegang apa-apa (None)
    [SerializeField] private Sprite defaultActiveBgSprite; // Background default saat memegang equipment (jika tidak ada bg khusus)
    [SerializeField] private bool hideIconWhenEmpty = true; // Sembunyikan ikon jika tidak memegang equipment

    [Header("Custom Visual Overrides")]
    [SerializeField] private EquipmentVisual[] customVisuals; // Pengaturan visual khusus per itemID

    private string lastEquippedItem = null;

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        if (InventoryManager.Instance != null)
        {
            string current = InventoryManager.Instance.currentEquippedItem;
            if (current != lastEquippedItem)
            {
                lastEquippedItem = current;
                UpdateUI();
            }
        }
    }

    public void UpdateUI()
    {
        if (InventoryManager.Instance == null) return;

        string currentEquipped = InventoryManager.Instance.currentEquippedItem;

        if (string.IsNullOrEmpty(currentEquipped))
        {
            // --- KONDISI KOSONG (TIDAK MEMEGANG EQUIPMENT) ---
            if (backgroundImage != null)
            {
                backgroundImage.sprite = emptyBgSprite;
            }

            if (iconImage != null)
            {
                if (hideIconWhenEmpty)
                {
                    iconImage.enabled = false;
                }
                else
                {
                    iconImage.sprite = null;
                    iconImage.enabled = false;
                }
            }
        }
        else
        {
            // --- KONDISI MEMEGANG EQUIPMENT ---
            Sprite bgSpriteToUse = defaultActiveBgSprite;
            Sprite iconSpriteToUse = null;

            // Cari apakah ada override visual khusus untuk itemID ini
            if (customVisuals != null)
            {
                foreach (var visual in customVisuals)
                {
                    if (visual.itemID == currentEquipped)
                    {
                        if (visual.backgroundSprite != null)
                        {
                            bgSpriteToUse = visual.backgroundSprite;
                        }
                        if (visual.overrideIconSprite != null)
                        {
                            iconSpriteToUse = visual.overrideIconSprite;
                        }
                        break;
                    }
                }
            }

            // Jika tidak ada override icon, ambil dari database InventoryManager
            if (iconSpriteToUse == null)
            {
                iconSpriteToUse = InventoryManager.Instance.GetItemSprite(currentEquipped);
            }

            // Terapkan ke UI
            if (backgroundImage != null && bgSpriteToUse != null)
            {
                backgroundImage.sprite = bgSpriteToUse;
            }

            if (iconImage != null)
            {
                if (iconSpriteToUse != null)
                {
                    iconImage.sprite = iconSpriteToUse;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }
        }
    }
}
