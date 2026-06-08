using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SecretItem : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject popUpPanel;
    [SerializeField] private Image displayImage;
    [SerializeField] private Sprite[] secretSprites;
    
    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 1.5f; 
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.4f; 

    [Header("Cutscene Redirect Settings")]
    [Tooltip("Centang jika item ini akan memicu pemuatan Cutscene dari Scene terpisah")]
    public bool pindahSceneCutscene = false;
    [Tooltip("Nama Scene Cutscene yang akan dituju, misal: Cutscene1")]
    public string namaSceneCutscene = "Cutscene1";
    [Tooltip("Lama menunggu sebelum layar diganti ke cutscene")]
    public float delaySebelumPindah = 1f;
    [Tooltip("ID dialog yang akan diputar setelah kembali dari scene cutscene. Cocokkan dengan PendingReturnDialoguePlayer di scene asal.")]
    public string returnDialogueID;

    private int currentSpriteIndex = 0;
    private bool canInteract = true;
    private bool isPlayerInRange = false;

    private List<int> randomizedIndices = new List<int>();
    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        RandomizeOrder();
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsFlagSet("chapter1_puzzle_solved"))
        {
            canInteract = false;
            ResetSpriteColor();
        }
        else if (canInteract)
        {
            blinkCoroutine = StartCoroutine(BlinkEffect());
        }
    }

    private void RandomizeOrder()
    {
        for (int i = 0; i < secretSprites.Length; i++)
        {
            randomizedIndices.Add(i);
        }

        for (int i = 0; i < randomizedIndices.Count; i++)
        {
            int temp = randomizedIndices[i];
            int randomIndex = UnityEngine.Random.Range(i, randomizedIndices.Count);
            randomizedIndices[i] = randomizedIndices[randomIndex];
            randomizedIndices[randomIndex] = temp;
        }
    }

    private void OnEnable() {
        PlayerControllerScript.OnInteractPressed += HandleInteraction;
        PlayerControllerScript.OnClosePressed += ClosePopUp;
    }

    private void OnDisable() {
        PlayerControllerScript.OnInteractPressed -= HandleInteraction;
        PlayerControllerScript.OnClosePressed -= ClosePopUp;
    }

    public void ClosePopUp() {
        // Jika tidak disetting pindah scene, maka tombol close berfugnsi normal menutup popup
        if (!pindahSceneCutscene && popUpPanel.activeSelf) {
            popUpPanel.SetActive(false);
        }
    }

    private void HandleInteraction() {
        if (isPlayerInRange && canInteract) {
            ShowPopUp();
        }
    }

    void ShowPopUp() {
        int spriteToDisplay = randomizedIndices[currentSpriteIndex];
        displayImage.sprite = secretSprites[spriteToDisplay];
        
        popUpPanel.SetActive(true);
        canInteract = false; 

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            ResetSpriteColor(); 
        }

        // --- FITUR BARU: Transisi ke Scene Cutscene Berbeda ---
        if (pindahSceneCutscene)
        {
            StartCoroutine(PindahKeCutsceneTunggu());
        }
    }
    
    private IEnumerator PindahKeCutsceneTunggu()
    {
        // 1. Tunggu X detik dengan canvas popup yang masih menyala (player sedang melihat foto)
        yield return new WaitForSeconds(delaySebelumPindah);
        
        // 2. Simpan scene tempat kita berada saat ini (misal "Chapter1") ke PlayerPrefs
        // Agar saat Cutscene kelar, Cutscene tahu harus balikin kita ke scene apa
        PlayerPrefs.SetString("ReturnSceneName", SceneManager.GetActiveScene().name);

        if (!string.IsNullOrEmpty(returnDialogueID))
        {
            PlayerPrefs.SetString("PendingReturnDialogueID", returnDialogueID);
        }
        
        // 3. Muat scene Cutscene1
        SceneManager.LoadScene(namaSceneCutscene);
    }

    public void ResetInteractions() {
        canInteract = true;
        currentSpriteIndex = (currentSpriteIndex + 1) % secretSprites.Length;

        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkEffect());
    }

    public void SetPuzzleSolved() {
        canInteract = false;
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        ResetSpriteColor();
    }

    // PERBAIKAN: Menggunakan IEnumerator non-generik bawaan System.Collections
    private IEnumerator BlinkEffect()
    {
        while (canInteract && spriteRenderer != null)
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

    public int GetCurrentSecretIndex() => randomizedIndices[currentSpriteIndex];

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) isPlayerInRange = false;
    }
}
