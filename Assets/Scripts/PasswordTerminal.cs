using UnityEngine;
using TMPro;
using System.Collections;

public class PasswordTerminal : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private string[] correctCodes;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private SecretItem firstItem;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject rewardPanel;

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
        PlayerControllerScript.OnClosePressed += CloseAllPanels; // Diubah agar menutup reward juga dengan ESC
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

    public void CheckPassword()
    {
        if (string.IsNullOrEmpty(inputField.text)) return;

        int actualIndex = firstItem.GetCurrentSecretIndex();

        if (inputField.text == correctCodes[actualIndex])
        {
            Debug.Log("Kode Benar! Pemain Mendapatkan Kail Pancing.");
            isPuzzleSolved = true;

            // 1. Masukkan kail pancing ke sistem Inventory
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.GetFishingRod();
            }

            // 2. Tutup panel input kode
            if (panel != null) panel.SetActive(false);

            // 3. Munculkan pop-up reward kail pancing di layar
            if (rewardPanel != null) rewardPanel.SetActive(true);

            StopBlink(); // Berhenti kedip selamanya karena laci/terminal sudah terpecahkan
        }
        else
        {
            Debug.Log("Kode Salah!");
            attemptCount++;
            inputField.text = "";
            inputField.ActivateInputField();
            
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
        // Jangan buka panel input jika reward sedang tampil atau teka-teki sudah selesai
        if (isPuzzleSolved || (rewardPanel != null && rewardPanel.activeSelf)) return;

        if (panel != null)
        {
            panel.SetActive(true);
            inputField.ActivateInputField(); 
            
            inputField.onEndEdit.RemoveAllListeners(); 
            inputField.onEndEdit.AddListener(delegate { CheckPassword(); });

            StopBlink(); 
        }
    }

    public void ClosePanel()
    {
        if (panel != null)
        {
            inputField.text = ""; 
            panel.SetActive(false);

            if (!isPuzzleSolved) StartBlink();
        }
    }

    // Fungsi baru untuk menutup segalanya saat tombol ESC ditekan
    public void CloseAllPanels()
    {
        ClosePanel();

        if (rewardPanel != null && rewardPanel.activeSelf)
        {
            rewardPanel.SetActive(false);
        }
    }

    // --- COROUTINE KEDIP (TETAP SAMA) ---
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