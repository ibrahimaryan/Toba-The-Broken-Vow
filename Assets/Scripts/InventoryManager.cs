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

    [Header("Equipment Settings")]
    [SerializeField] private int equipmentStartSlotIndex = 14; // Slot 15 di UI (0-indexed)

    [Header("Equipment Slot Backgrounds")]
    [SerializeField] private Sprite equipmentEmptyBgSprite;  // Sprite bg saat slot kosong (misal kotak putih bawaan)
    [SerializeField] private Sprite equipmentNormalBgSprite; // Sprite bg saat equipment dimiliki tapi tidak dipakai (hilangkan bg putih -> transparan)
    [SerializeField] private Sprite equipmentActiveBgSprite; // Sprite bg saat equipment aktif dipakai (asset gambar kuning/custom dari user)

    public bool hasFishingRod { get; private set; } = false;
    public bool hasAxe { get; private set; } = false;
    public bool hasEquipment2 { get; private set; } = false;
    public string currentEquippedItem { get; private set; } = ""; // ID item yang sedang dipasang ("kapak", "equipment2", atau "")

    // Database penyimpanan item dan jumlahnya di runtime
    private Dictionary<string, int> items = new Dictionary<string, int>();

    // Menyimpan sprite asli dari slot untuk dikembalikan saat kosong
    private Dictionary<GameObject, Sprite> originalSlotSprites = new Dictionary<GameObject, Sprite>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Catat sprite bawaan asli dari masing-masing slot agar bisa direstore saat kosong
        if (staticSlots != null)
        {
            foreach (GameObject slot in staticSlots)
            {
                if (slot != null)
                {
                    Image img = slot.GetComponent<Image>();
                    if (img != null)
                    {
                        originalSlotSprites[slot] = img.sprite;
                    }
                }
            }
        }
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

    // --- EQUIPMENT METHODS ---
    public void EquipAxe()
    {
        hasAxe = true;
        UpdateInventoryUI();
        Debug.Log("Kapak masuk ke equipment slot!");
    }

    public void EquipEquipment2()
    {
        hasEquipment2 = true;
        UpdateInventoryUI();
        Debug.Log("Equipment 2 masuk ke equipment slot!");
    }

    public void CycleEquipment()
    {
        List<string> ownedEquipment = new List<string>();
        if (hasAxe) ownedEquipment.Add("kapak");
        if (hasEquipment2) ownedEquipment.Add("equipment2");

        if (ownedEquipment.Count == 0)
        {
            currentEquippedItem = "";
            UpdateInventoryUI();
            return;
        }

        int currentIndex = ownedEquipment.IndexOf(currentEquippedItem);
        int nextIndex = currentIndex + 1;

        if (nextIndex >= ownedEquipment.Count)
        {
            currentEquippedItem = ""; // Kembali ke tanpa equipment (None)
        }
        else
        {
            currentEquippedItem = ownedEquipment[nextIndex];
        }

        UpdateInventoryUI();
        Debug.Log($"Equipment aktif diganti ke: {(string.IsNullOrEmpty(currentEquippedItem) ? "None" : currentEquippedItem)}");
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
            // Ambil semua item dalam list untuk dipetakan ke slot (hanya item biasa)
            List<KeyValuePair<string, int>> normalItems = new List<KeyValuePair<string, int>>();
            foreach (KeyValuePair<string, int> pair in items)
            {
                // Item legasi pancingan tidak perlu digambar ulang di slot dinamis jika sudah menggunakan slot fisik tersendiri
                if (pair.Key == "fishing_rod" && fishingRodSlotImage != null) continue;
                normalItems.Add(pair);
            }

            // Batas maksimal slot item biasa adalah sebelum slot equipment (misal index 14 untuk slot 15)
            int maxNormalSlots = Mathf.Min(equipmentStartSlotIndex, staticSlots.Length);

            // 2a. Gambar normal items di slot sebelum slot equipment
            for (int i = 0; i < maxNormalSlots; i++)
            {
                GameObject slotObj = staticSlots[i];
                if (slotObj == null) continue;

                if (i < normalItems.Count)
                {
                    UpdateSlotUI(slotObj, normalItems[i].Key, normalItems[i].Value);
                }
                else
                {
                    ClearSlotUI(slotObj);
                }
            }

            // 2b. Gambar equipment items mulai dari slot index equipmentStartSlotIndex (Slot 15+)
            List<string> ownedEquipment = new List<string>();
            if (hasAxe) ownedEquipment.Add("kapak");
            if (hasEquipment2) ownedEquipment.Add("equipment2");

            for (int i = equipmentStartSlotIndex; i < staticSlots.Length; i++)
            {
                GameObject slotObj = staticSlots[i];
                if (slotObj == null) continue;

                int equipListIdx = i - equipmentStartSlotIndex;
                if (equipListIdx < ownedEquipment.Count)
                {
                    string equipID = ownedEquipment[equipListIdx];
                    UpdateSlotUI(slotObj, equipID, 1);
                    HighlightEquippedSlot(slotObj, equipID == currentEquippedItem, true);
                }
                else
                {
                    ClearSlotUI(slotObj);
                    HighlightEquippedSlot(slotObj, false, false);
                }
            }
        }
    }

    private void HighlightEquippedSlot(GameObject slot, bool isActive, bool hasItem)
    {
        // Cari child image bernama "Highlight" atau "Outline" atau "Selected"
        Transform hlTransform = slot.transform.Find("Highlight") ?? slot.transform.Find("Outline") ?? slot.transform.Find("Selected");
        if (hlTransform != null)
        {
            Image hlImage = hlTransform.GetComponent<Image>();
            if (hlImage != null)
            {
                hlImage.enabled = isActive;
                return;
            }
        }

        // Fallback: Ubah sprite / warna background slot itu sendiri
        Image slotImage = slot.GetComponent<Image>();
        if (slotImage != null)
        {
            if (!hasItem)
            {
                // Jika slot kosong, kembalikan ke background bawaan asli (atau sprite kosong kustom jika di-assign)
                slotImage.sprite = (equipmentEmptyBgSprite != null) ? equipmentEmptyBgSprite : (originalSlotSprites.ContainsKey(slot) ? originalSlotSprites[slot] : null);
                
                Color emptyColor;
                if (ColorUtility.TryParseHtmlString("#472C17", out emptyColor))
                {
                    slotImage.color = emptyColor;
                }
                else
                {
                    slotImage.color = Color.white;
                }
            }
            else if (isActive)
            {
                // Jika aktif dipakai, ubah ke sprite aktif dari user
                if (equipmentActiveBgSprite != null)
                {
                    slotImage.sprite = equipmentActiveBgSprite;
                    slotImage.color = Color.white;
                }
                else
                {
                    // Fallback jika sprite tidak di-assign: warna kuning
                    slotImage.sprite = null;
                    slotImage.color = new Color(1f, 0.92f, 0.016f, 0.8f);
                }
            }
            else
            {
                // Jika dimiliki tapi tidak dipakai, hilangkan background putih (buat transparan)
                if (equipmentNormalBgSprite != null)
                {
                    slotImage.sprite = equipmentNormalBgSprite;
                    slotImage.color = Color.white;
                }
                else
                {
                    // Membuat background transparan penuh (menghilangkan bg putih)
                    slotImage.sprite = null;
                    slotImage.color = new Color(1f, 1f, 1f, 0f);
                }
            }
        }
    }

    private void UpdateSlotUI(GameObject slot, string itemID, int count)
    {
        // Pertahankan warna background slot agar tidak menjadi putih polos saat ada item
        Image slotImage = slot.GetComponent<Image>();
        if (slotImage != null)
        {
            Color emptyColor;
            if (ColorUtility.TryParseHtmlString("#472C17", out emptyColor))
            {
                slotImage.color = emptyColor;
            }
        }

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
        // Set warna background slot menjadi #472C17 saat kosong
        Image slotImage = slot.GetComponent<Image>();
        if (slotImage != null)
        {
            Color emptyColor;
            if (ColorUtility.TryParseHtmlString("#472C17", out emptyColor))
            {
                slotImage.color = emptyColor;
            }
        }

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