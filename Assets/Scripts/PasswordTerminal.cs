using UnityEngine;
using TMPro;
using System.Collections; // Wajib ada untuk Coroutine

public class PasswordTerminal : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private string[] correctCodes;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private SecretItem firstItem;
    [SerializeField] private GameObject panel;
    [SerializeField] private DoorController door;

    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 1.5f; // Kecepatan kedip papan
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.4f; // Batas paling tipis saat memudar

    private int attemptCount = 0;
    private bool isPlayerInRange = false;
    private bool isPuzzleSolved = false; // Pengaman agar berhenti berkedip jika sudah menang

    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Mulai efek kedip sejak awal level berjalan
        StartBlink();
    }

    private void OnEnable()
    {
        PlayerControllerScript.OnInteractPressed += HandleInteraction;
        PlayerControllerScript.OnClosePressed += ClosePanel;
    }

    private void OnDisable()
    {
        PlayerControllerScript.OnInteractPressed -= HandleInteraction;
        PlayerControllerScript.OnClosePressed -= ClosePanel;
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
            Debug.Log("Kode Benar! Pintu Terbuka.");
            isPuzzleSolved = true;

            if (door != null) door.OpenDoor();
            
            StopBlink(); // Hentikan kedip selamanya karena puzzle sudah selesai
            ClosePanel();
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
                firstItem.ResetInteractions(); // Mengacak ulang gambar di benda pertama
                ClosePanel(); 
            }
        }
    }

    public void OpenPanel()
    {
        if (panel != null)
        {
            panel.SetActive(true);
            inputField.ActivateInputField(); 
            
            inputField.onEndEdit.RemoveAllListeners(); 
            inputField.onEndEdit.AddListener(delegate { CheckPassword(); });

            StopBlink(); // Hentikan kedip saat panel UI sedang dibuka agar tidak mengganggu background
        }
    }

    public void ClosePanel()
    {
        if (panel != null)
        {
            inputField.text = ""; 
            panel.SetActive(false);

            // Mulai kedip lagi saat panel ditutup (jika puzzle belum sukses dipecahkan)
            StartBlink();
        }
    }

    // --- FUNGSI UNTUK MENGATUR COROUTINE KEDIP ---
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
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white; 
        }
    }
    // ---------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            ClosePanel(); 
        }
    }
}