using UnityEngine;
using UnityEngine.UI; // Wajib untuk komponen Image

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject inventoryPanel; 
    
    [Header("Item Slots")]
    [SerializeField] private Image fishingRodSlotImage; // KITA UBAH JADI Image

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
        PlayerControllerScript.OnInventoryPressed -= TransitionToggle; 
    }

    private void TransitionToggle()
    {
        PlayerControllerScript.OnInventoryPressed -= ToggleInventory;
    }

    public void GetFishingRod()
    {
        hasFishingRod = true;
        UpdateInventoryUI(); 
        Debug.Log("Kail Pancing masuk ke inventory!");
    }

    public void UseFishingRod()
    {
        hasFishingRod = false;
        UpdateInventoryUI(); 
        Debug.Log("Kail Pancing telah digunakan!");
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

    private void UpdateInventoryUI()
    {
        if (fishingRodSlotImage != null)
        {
            // Mengaktifkan/mematikan komponen gambarnya saja, objek UI tetap aman hidup
            fishingRodSlotImage.enabled = hasFishingRod;
        }
    }
}