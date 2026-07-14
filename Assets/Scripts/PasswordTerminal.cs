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
    [SerializeField] private TMP_Text attemptText;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip wrongPasswordSound;
    [SerializeField] private AudioClip rewardSound;

    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue solvedCloseDialogue;
    [SerializeField] private string solvedCloseDialogueFlag = "chapter1_terminal_solved_dialogue";

    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 1.5f; 
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.4f; 

    private int attemptCount = 0;
    private bool isPlayerInRange = false;
    private bool isPuzzleSolved = false; 

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsFlagSet("chapter1_puzzle_solved"))
        {
            isPuzzleSolved = true;
            StopBlink();
        }
        else
        {
            StartBlink();
        }
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
                if (audioSource != null && buttonClickSound != null)
                {
                    audioSource.PlayOneShot(buttonClickSound);
                }
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
                if (audioSource != null && buttonClickSound != null)
                {
                    audioSource.PlayOneShot(buttonClickSound);
                }
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

    private void UpdateAttemptText()
    {
        if (attemptText != null)
        {
            int remainingAttempts = 3 - attemptCount;
            attemptText.text = "*Anda memiliki " + remainingAttempts + " kesempatan lagi";
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
            if (ToDoManager.Instance != null)
            {
                // Angka 1 berarti mencoret misi urutan KEDUA di daftar misi Chapter tersebut
                ToDoManager.Instance.SelesaikanMisi(1); 
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetFlag("chapter1_puzzle_solved", true);
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.GetFishingRod();
            }

            if (panel != null) panel.SetActive(false);
            if (rewardPanel != null)
            {
                rewardPanel.SetActive(true);
                if (audioSource != null && rewardSound != null)
                {
                    audioSource.PlayOneShot(rewardSound);
                }
            }

            StopBlink(); 

            if (firstItem != null)
            {
                firstItem.SetPuzzleSolved();
            }

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

            if (audioSource != null && wrongPasswordSound != null)
            {
                audioSource.PlayOneShot(wrongPasswordSound);
            }
            
            if (attemptCount >= 3)
            {
                attemptCount = 0;
                firstItem.ResetInteractions(); 
                ClosePanel(); 
            }
            else
            {
                UpdateAttemptText();
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

            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.HidePrompt();
            }

            // Matikan interaksi keyboard langsung di semua kotak
            for (int i = 0; i < digitFields.Length; i++)
            {
                if (digitFields[i] != null) digitFields[i].DeactivateInputField();
            }

            UpdateAttemptText();

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

            if (isPlayerInRange && !isPuzzleSolved && InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk akses terminal");
            }
        }
    }

    public void CloseAllPanels()
    {
        bool wasRewardActive = (rewardPanel != null && rewardPanel.activeSelf);
        bool wasAnyActive = (panel != null && panel.activeSelf) || wasRewardActive;

        ClosePanel();

        if (rewardPanel != null && rewardPanel.activeSelf)
        {
            rewardPanel.SetActive(false);
        }

        if (wasAnyActive && UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            PauseMenuManager.PanelWasClosedThisFrame = true;
        }

        if (wasRewardActive || (isPuzzleSolved && GameManager.Instance != null && GameManager.Instance.IsFlagSet("chapter1_puzzle_solved")))
        {
            if (solvedCloseDialogue != null && GameManager.Instance != null && !GameManager.Instance.IsFlagSet(solvedCloseDialogueFlag))
            {
                DialogueManager.instance.StartDialogue(solvedCloseDialogue);
                GameManager.Instance.SetFlag(solvedCloseDialogueFlag, true);
            }
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
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (!isPuzzleSolved && InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk akses terminal");
            }
        }
    }

    private void Update()
    {
        if ((panel != null && panel.activeSelf) || (rewardPanel != null && rewardPanel.activeSelf))
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseAllPanels();
                PauseMenuManager.PanelWasClosedThisFrame = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            CloseAllPanels(); 
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.HidePrompt();
            }
        }
    }

    public bool IsPanelActive()
    {
        return (panel != null && panel.activeSelf) || 
               (rewardPanel != null && rewardPanel.activeSelf);
    }
}
