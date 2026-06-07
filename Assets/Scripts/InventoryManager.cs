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

    [Header("Static Grid Inventory Settings")]
    [SerializeField] private GameObject[] staticSlots;    // Tarik 20 GameObject slot Anda ke sini di Inspector
    [SerializeField] private ItemData[] itemDatabase;     // Daftar asset gambar item berdasarkan ID-nya

    public bool hasFishingRod { get; private set; } = false;

    // Database penyimpanan item dan jumlahnya di runtime
    private Dictionary<string, int> items = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateInventoryUI();
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
        Debug.Log($"[Inventory] UpdateInventoryUI: hasFishingRod={hasFishingRod}, total items in dict={items.Count}, assigned staticSlots={staticSlots?.Length ?? 0}");

        // 1. Update legacy kail pancing
        if (fishingRodSlotImage != null)
        {
            fishingRodSlotImage.enabled = hasFishingRod;
        }

        // 2. Update static slots grid jika dipasang di inspector
        if (staticSlots != null && staticSlots.Length > 0)
        {
            // Ambil semua item dalam list untuk dipetakan ke slot
            List<KeyValuePair<string, int>> currentItems = new List<KeyValuePair<string, int>>();
            foreach (KeyValuePair<string, int> pair in items)
            {
                // Item legasi pancingan tidak perlu digambar ulang di slot dinamis jika sudah menggunakan slot fisik tersendiri
                if (pair.Key == "fishing_rod" && fishingRodSlotImage != null) continue;
                currentItems.Add(pair);
            }

            // Update setiap slot UI berdasarkan data item
            for (int i = 0; i < staticSlots.Length; i++)
            {
                GameObject slotObj = staticSlots[i];
                if (slotObj == null) continue;

                if (i < currentItems.Count)
                {
                    // Masukkan item ke slot
                    UpdateSlotUI(slotObj, currentItems[i].Key, currentItems[i].Value);
                }
                else
                {
                    // Kosongkan slot
                    ClearSlotUI(slotObj);
                }
            }
        }
    }

    private void UpdateSlotUI(GameObject slot, string itemID, int count)
    {
        // Cari Image Component untuk Icon (mencari di child bernama "icon", "Icon" atau "Image" atau "ItemImage")
        Image iconImage = null;
        Transform iconTransform = slot.transform.Find("icon") ?? slot.transform.Find("Icon") ?? slot.transform.Find("Image") ?? slot.transform.Find("ItemImage");
        
        if (iconTransform != null)
        {
            iconImage = iconTransform.GetComponent<Image>();
        }
        else
        {
            // Fallback: Cari Image pertama di anak-anaknya yang bukan background slot itu sendiri
            foreach (Transform child in slot.transform)
            {
                Image img = child.GetComponent<Image>();
                if (img != null)
                {
                    iconImage = img;
                    break;
                }
            }
        }

        if (iconImage != null)
        {
            Sprite icon = GetItemSprite(itemID);
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = true;
                Debug.Log($"[Inventory] Slot {slot.name}: Menampilkan item '{itemID}'");
            }
            else
            {
                iconImage.enabled = false;
                Debug.LogWarning($"[Inventory] Slot {slot.name}: Sprite untuk '{itemID}' tidak ditemukan di database!");
            }
        }
        else
        {
            Debug.LogError($"[Inventory] Slot {slot.name}: Tidak ditemukan komponen Image (Icon) di child!");
        }

        // Cari Text Component untuk jumlah (mencari di child bernama "Count", "CountText", "Amount", "text", atau "Text")
        Transform textTransform = slot.transform.Find("Count") ?? slot.transform.Find("CountText") ?? slot.transform.Find("Amount") ?? slot.transform.Find("text") ?? slot.transform.Find("Text");
        
        TMPro.TMP_Text tmpText = null;
        Text legacyText = null;

        if (textTransform != null)
        {
            tmpText = textTransform.GetComponent<TMPro.TMP_Text>();
            legacyText = textTransform.GetComponent<Text>();
        }
        else
        {
            // Fallback: Cari Text/TMP Component di seluruh child
            tmpText = slot.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmpText == null)
            {
                legacyText = slot.GetComponentInChildren<Text>();
            }
        }

        if (tmpText != null)
        {
            tmpText.text = count > 1 ? count.ToString() : "";
        }
        else if (legacyText != null)
        {
            legacyText.text = count > 1 ? count.ToString() : "";
        }
        else
        {
            Debug.LogWarning($"[Inventory] Slot {slot.name}: Tidak ditemukan komponen Text untuk jumlah!");
        }
    }

    private void ClearSlotUI(GameObject slot)
    {
        // Matikan gambar ikon
        Image iconImage = null;
        Transform iconTransform = slot.transform.Find("icon") ?? slot.transform.Find("Icon") ?? slot.transform.Find("Image") ?? slot.transform.Find("ItemImage");
        
        if (iconTransform != null)
        {
            iconImage = iconTransform.GetComponent<Image>();
        }
        else
        {
            foreach (Transform child in slot.transform)
            {
                Image img = child.GetComponent<Image>();
                if (img != null)
                {
                    iconImage = img;
                    break;
                }
            }
        }

        if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        // Kosongkan teks jumlah
        Transform textTransform = slot.transform.Find("Count") ?? slot.transform.Find("CountText") ?? slot.transform.Find("Amount") ?? slot.transform.Find("text") ?? slot.transform.Find("Text");
        
        TMPro.TMP_Text tmpText = null;
        Text legacyText = null;

        if (textTransform != null)
        {
            tmpText = textTransform.GetComponent<TMPro.TMP_Text>();
            legacyText = textTransform.GetComponent<Text>();
        }
        else
        {
            tmpText = slot.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmpText == null)
            {
                legacyText = slot.GetComponentInChildren<Text>();
            }
        }

        if (tmpText != null)
        {
            tmpText.text = "";
        }
        else if (legacyText != null)
        {
            legacyText.text = "";
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