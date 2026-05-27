using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PasswordTerminal : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private string[] correctCodes;
    
    // UBAH: Menggunakan array untuk menampung 4 kotak input field terpisah
    [SerializeField] private TMP_InputField[] digitFields = new TMP_InputField[4];
    
    [SerializeField] private SecretItem firstItem;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private PatungStatue targetStatue; 

    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 1.5f; 
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.4f; 

    private int attemptCount = 0;
    private bool isPlayerInRange = false;
    private bool isPuzzleSolved = false; 

    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StartBlink();
    }

    private void OnEnable()
    {
        PlayerControllerScript.OnInteractPressed += HandleInteraction;
        PlayerControllerScript.OnClosePressed += CloseAllPanels;
    }

    private void OnDisable()
    {
        PlayerControllerScript.OnInteractPressed -= HandleInteraction;
        PlayerControllerScript.OnClosePressed -= CloseAllPanels;
    }

    private void HandleInteraction()
    {
        if (isPlayerInRange && !isPuzzleSolved)
        {
            OpenPanel();
        }
    }

    // --- FUNGSI KETIK ANGKA UNTUK 4 KOTAK ---
    public void KetikAngka(string angka)
    {
        if (isPuzzleSolved) return;
        
        // Cari kotak mana yang masih kosong dari urutan pertama (0 sampai 3)
        for (int i = 0; i < digitFields.Length; i++)
        {
            if (digitFields[i] != null && string.IsNullOrEmpty(digitFields[i].text))
            {
                digitFields[i].text = angka;
                break; // Keluar dari loop setelah mengisi 1 kotak
            }
        }
    }

    // --- FUNGSI HAPUS ANGKA (Mundur dari belakang) ---
    public void HapusAngka()
    {
        if (isPuzzleSolved) return;

        // Cari kotak paling belakang yang ada isinya, lalu hapus
        for (int i = digitFields.Length - 1; i >= 0; i--)
        {
            if (digitFields[i] != null && !string.IsNullOrEmpty(digitFields[i].text))
            {
                digitFields[i].text = "";
                break; // Keluar dari loop setelah menghapus 1 kotak
            }
        }
    }

    // --- FUNGSI AMBIL TOTAL KODE YANG DIKETIK ---
    private string GetCombinedCode()
    {
        string fullCode = "";
        for (int i = 0; i < digitFields.Length; i++)
        {
            if (digitFields[i] != null)
            {
                fullCode += digitFields[i].text;
            }
        }
        return fullCode;
    }

    // --- FUNGSI RESET SEMUA KOTAK ---
    private void ResetAllFields()
    {
        for (int i = 0; i < digitFields.Length; i++)
        {
            if (digitFields[i] != null) digitFields[i].text = "";
        }
    }

    public void CheckPassword()
    {
        string inputPassword = GetCombinedCode();

        // Jika kotak belum terisi penuh 4 digit, abaikan/jangan submit dulu
        if (inputPassword.Length < 4) return;

        int actualIndex = firstItem.GetCurrentSecretIndex();

        if (inputPassword == correctCodes[actualIndex])
        {
            Debug.Log("Kode Benar! Pemain Mendapatkan Kail Pancing.");
            isPuzzleSolved = true;

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.GetFishingRod();
            }

            if (panel != null) panel.SetActive(false);
            if (rewardPanel != null) rewardPanel.SetActive(true);

            StopBlink(); 

            if (targetStatue != null)
            {
                targetStatue.StartBlinkEffect();
            }
        }
        else
        {
            Debug.Log("Kode Salah!");
            attemptCount++;
            ResetAllFields(); // Kosongkan semua kotak jika salah
            
            if (attemptCount >= 3)
            {
                attemptCount = 0;
                firstItem.ResetInteractions(); 
                ClosePanel(); 
            }
        }
    }

    public void OpenPanel()
    {
        if (isPuzzleSolved || (rewardPanel != null && rewardPanel.activeSelf)) return;

        if (panel != null)
        {
            panel.SetActive(true);
            ResetAllFields(); // Pastikan kosong saat dibuka

            // Matikan interaksi keyboard langsung di semua kotak
            for (int i = 0; i < digitFields.Length; i++)
            {
                if (digitFields[i] != null) digitFields[i].DeactivateInputField();
            }

            StopBlink(); 
        }
    }

    public void ClosePanel()
    {
        if (panel != null)
        {
            ResetAllFields();
            panel.SetActive(false);

            if (!isPuzzleSolved) StartBlink();
        }
    }

    public void CloseAllPanels()
    {
        ClosePanel();

        if (rewardPanel != null && rewardPanel.activeSelf)
        {
            rewardPanel.SetActive(false);
        }
    }

    private void StartBlink()
    {
        if (!isPuzzleSolved && blinkCoroutine == null && spriteRenderer != null)
        {
            blinkCoroutine = StartCoroutine(BlinkEffect());
        }
    }

    private void StopBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        ResetSpriteColor();
    }

    private IEnumerator BlinkEffect()
    {
        while (!isPuzzleSolved && spriteRenderer != null)
        {
            float lerpTime = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            float alpha = Mathf.Lerp(minAlpha, 1f, lerpTime);
            spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            yield return null; 
        }
    }

    private void ResetSpriteColor()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.white; 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            CloseAllPanels(); 
        }
    }
}