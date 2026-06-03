using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [System.Serializable]
    public struct ItemData
    {
        public string itemID;
        public Sprite itemIcon;
    }

    [Header("UI Panels")]
    [SerializeField] private GameObject inventoryPanel; 
    
    [Header("Legacy Item Slots (Optional Backward Compatibility)")]
    [SerializeField] private Image fishingRodSlotImage; 

    [Header("Dynamic Inventory Settings")]
    [SerializeField] private GameObject slotPrefab;      // Prefab untuk slot item (berisi Image untuk icon dan Text untuk jumlah)
    [SerializeField] private Transform slotParent;        // Container (misalnya Grid Layout Group) tempat menampung slot item
    [SerializeField] private ItemData[] itemDatabase;     // Daftar asset gambar item berdasarkan ID-nya

    public bool hasFishingRod { get; private set; } = false;

    // Database penyimpanan item dan jumlahnya di runtime
    private Dictionary<string, int> items = new Dictionary<string, int>();

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
        PlayerControllerScript.OnInventoryPressed -= TransitionToggle; 
    }

    private void TransitionToggle()
    {
        PlayerControllerScript.OnInventoryPressed -= ToggleInventory;
    }

    // --- LEGACY METHODS (Kail Pancing) ---
    public void GetFishingRod()
    {
        hasFishingRod = true;
        AddItem("fishing_rod", 1);
        UpdateInventoryUI(); 
        Debug.Log("Kail Pancing masuk ke inventory!");
    }

    public void UseFishingRod()
    {
        hasFishingRod = false;
        RemoveItem("fishing_rod", 1);
        UpdateInventoryUI(); 
        Debug.Log("Kail Pancing telah digunakan!");
    }

    // --- DYNAMIC INVENTORY METHODS ---
    
    public void AddItem(string itemID, int count = 1)
    {
        if (items.ContainsKey(itemID))
        {
            items[itemID] += count;
        }
        else
        {
            items[itemID] = count;
        }
        Debug.Log($"Inventory: Menambah {count}x {itemID}. Total sekarang: {items[itemID]}");
        UpdateInventoryUI();
    }

    public void RemoveItem(string itemID, int count = 1)
    {
        if (items.ContainsKey(itemID))
        {
            items[itemID] -= count;
            if (items[itemID] <= 0)
            {
                items.Remove(itemID);
            }
            UpdateInventoryUI();
        }
    }

    public int GetItemCount(string itemID)
    {
        if (items.ContainsKey(itemID))
        {
            return items[itemID];
        }
        return 0;
    }

    private void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            if (inventoryPanel.activeSelf)
            {
                UpdateInventoryUI();
            }
        }
    }

    public void UpdateInventoryUI()
    {
        // 1. Update legacy kail pancing
        if (fishingRodSlotImage != null)
        {
            fishingRodSlotImage.enabled = hasFishingRod;
        }

        // 2. Update dynamic slots
        if (slotParent != null && slotPrefab != null)
        {
            // Hapus slot lama terlebih dahulu
            foreach (Transform child in slotParent)
            {
                Destroy(child.gameObject);
            }

            // Instansiasi slot baru untuk setiap item yang dimiliki
            foreach (KeyValuePair<string, int> pair in items)
            {
                // Item legasi pancingan tidak perlu di-render ulang secara dinamis jika slot fisiknya sudah ada di luar
                if (pair.Key == "fishing_rod" && fishingRodSlotImage != null) continue;

                GameObject newSlot = Instantiate(slotPrefab, slotParent);
                
                // Cari komponen Image untuk Icon
                Image iconImage = newSlot.transform.Find("Icon")?.GetComponent<Image>();
                if (iconImage != null)
                {
                    Sprite icon = GetItemSprite(pair.Key);
                    if (icon != null)
                    {
                        iconImage.sprite = icon;
                        iconImage.enabled = true;
                    }
                    else
                    {
                        iconImage.enabled = false;
                    }
                }

                // Cari komponen Text/TextMeshPro untuk Jumlah
                TMPro.TMP_Text tmpText = newSlot.transform.Find("Count")?.GetComponent<TMPro.TMP_Text>();
                if (tmpText != null)
                {
                    tmpText.text = pair.Value > 1 ? pair.Value.ToString() : "";
                }
                else
                {
                    Text legacyText = newSlot.transform.Find("Count")?.GetComponent<Text>();
                    if (legacyText != null)
                    {
                        legacyText.text = pair.Value > 1 ? pair.Value.ToString() : "";
                    }
                }
            }
        }
    }

    private Sprite GetItemSprite(string itemID)
    {
        if (itemDatabase == null) return null;
        foreach (ItemData data in itemDatabase)
        {
            if (data.itemID == itemID)
            {
                return data.itemIcon;
            }
        }
        return null;
    }
}