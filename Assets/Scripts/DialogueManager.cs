using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; 

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance; 

    [Header("Screen Box UI (Dialog Biasa)")]
    public GameObject screenBoxPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;

    [Header("Cutscene Box UI (Otomatis)")]
    public GameObject cutsceneBoxPanel;
    public TextMeshProUGUI cutsceneNameText;
    public TextMeshProUGUI cutsceneDialogText;

    [Header("Speech Bubble UI (Otomatis)")]
    public GameObject bubblePanel;
    public TextMeshProUGUI bubbleText;

    [Header("Pengaturan Waktu (Untuk Cutscene/Bubble)")]
    [Tooltip("Berapa detik jeda sebelum dialog otomatis memuat kalimat berikutnya?")]
    public float durasiOtomatis = 3f; 

    private Queue<DialogueLine> sentences; 
    private bool isTyping = false; 

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        sentences = new Queue<DialogueLine>();
        screenBoxPanel.SetActive(false);
        bubblePanel.SetActive(false);
        
        if (cutsceneBoxPanel != null) cutsceneBoxPanel.SetActive(false);
    }

    void Update()
    {
        if (!isTyping)
        {
            bool nextPressed = false;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    nextPressed = true;
                }
            }

            // Dialog Biasa (screenBoxPanel) bisa lanjut dengan tombol E, Enter, atau Space
            if (screenBoxPanel.activeInHierarchy && (Keyboard.current.eKey.wasPressedThisFrame || nextPressed))
            {
                DisplayNextSentence();
            }
            // Dialog Bubble (bubblePanel) bisa lanjut dengan tombol Enter atau Space
            else if (bubblePanel.activeInHierarchy && nextPressed)
            {
                DisplayNextSentence();
            }
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        // Paksa berhenti seluruh ketikan lama yang menggantung!
        StopAllCoroutines(); 
        isTyping = false; 

        sentences.Clear(); 
        foreach (DialogueLine line in dialogue.lines)
        {
            sentences.Enqueue(line);
        }
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (isTyping) return; 

        if (sentences.Count == 0)
        {
            TutupPaksaSeluruhPanel(); // Ganti rujukan tutup
            return;
        }

        DialogueLine currentLine = sentences.Dequeue(); 
        StopAllCoroutines(); // Paksa matikan TypeSentence lama lagi
        StartCoroutine(TypeSentence(currentLine));
    }

    private IEnumerator TypeSentence(DialogueLine line)
    {
        isTyping = true;
        
        // Sembunyikan semuanya sebelum menentukan yang mana yg aktif
        screenBoxPanel.SetActive(false);
        bubblePanel.SetActive(false);
        if (cutsceneBoxPanel != null) cutsceneBoxPanel.SetActive(false);

        TextMeshProUGUI activeTextDisplay;

        if (line.isSpeechBubble)
        {
            bubblePanel.SetActive(true);
            activeTextDisplay = bubbleText;
            AdjustBubblePosition(line.characterName);
        }
        else if (line.isCutsceneStyle && cutsceneBoxPanel != null) 
        {
            cutsceneBoxPanel.SetActive(true);
            if (cutsceneNameText != null) cutsceneNameText.text = line.characterName;
            activeTextDisplay = cutsceneDialogText;
        }
        else 
        {
            screenBoxPanel.SetActive(true);
            nameText.text = line.characterName; 
            activeTextDisplay = dialogText;
        }

        activeTextDisplay.text = ""; 

        // Efek mesin tik (Typewriter) mengeja satu persatu
        foreach (char letter in line.sentence.ToCharArray())
        {
            activeTextDisplay.text += letter;
            yield return new WaitForSeconds(0.02f); 
        }

        isTyping = false;

        // FITUR OTOMATIS LANJUT SEPERTI FILM (Hanya untuk bubble Samosir/Player/Narrator, sedangkan NPC menunggu Enter)
        bool isPlayerOrNarratorBubble = line.isSpeechBubble && 
            (line.characterName == "Samosir" || line.characterName == "Player" || string.IsNullOrEmpty(line.characterName));

        if (isPlayerOrNarratorBubble)
        {
            yield return new WaitForSeconds(durasiOtomatis);
            DisplayNextSentence(); 
        }
        else if (line.isCutsceneStyle)
        {
            // [DIKOSONGKAN] Tipe Cutscene akan diam menunggu ditutup oleh panjang balok Timeline!
            yield return new WaitForSeconds(line.holdTime);
            TutupPaksaSeluruhPanel();
        }
    }

    private void EndDialogue()
    {
        screenBoxPanel.SetActive(false);
        bubblePanel.SetActive(false);
        if (cutsceneBoxPanel != null) cutsceneBoxPanel.SetActive(false);

        ResetBubbleTarget();
    }

    public void TutupPaksaSeluruhPanel()
    {
        StopAllCoroutines(); 
        isTyping = false;     // Matikan sisa ketikan yang nanggung
        sentences.Clear(); 
        
        // Jangan panggil EndDialogue(), kita matikan manual saja di sini:
        if(screenBoxPanel != null) screenBoxPanel.SetActive(false);
        if(bubblePanel != null) bubblePanel.SetActive(false);
        if(cutsceneBoxPanel != null) cutsceneBoxPanel.SetActive(false);

        ResetBubbleTarget();
    }

    private void ResetBubbleTarget()
    {
        if (bubblePanel != null)
        {
            var dynamicBubble = bubblePanel.GetComponent<DynamicBubblePosition>();
            if (dynamicBubble != null)
            {
                dynamicBubble.targetCharacter = null;
            }
        }
    }

    private void AdjustBubblePosition(string characterName)
    {
        if (bubblePanel == null) return;
        
        // Pengaman jika nama kosong (narrator/dialog tanpa nama)
        if (string.IsNullOrEmpty(characterName)) return;

        // 1. Cari GameObject pembicara berdasarkan nama persis di scene
        GameObject speakerGo = GameObject.Find(characterName);
        
        // 2. Coba cari parsial jika tidak ketemu persis dan bukan Player/Samosir
        if (speakerGo == null && characterName != "Samosir" && characterName != "Player")
        {
            var transforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach (var t in transforms)
            {
                if (t.gameObject.name.IndexOf(characterName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    speakerGo = t.gameObject;
                    break;
                }
            }
        }

        // 3. Fallback jika nama Samosir/Player, atau jika pencarian nama gagal
        if (speakerGo == null && (characterName == "Samosir" || characterName == "Player"))
        {
            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null) speakerGo = player.gameObject;
        }
        else if (speakerGo == null)
        {
            // Jika pembicara bukan player/Samosir, coba cari dari story manager aktif
            if (Chapter4StoryManager.Instance != null && Chapter4StoryManager.Instance.npcGameObject != null)
            {
                speakerGo = Chapter4StoryManager.Instance.npcGameObject;
            }
            else if (Chapter3StoryManager.Instance != null && Chapter3StoryManager.Instance.npcGameObject != null)
            {
                speakerGo = Chapter3StoryManager.Instance.npcGameObject;
            }
        }

        // Tambahan: jika ada script DynamicBubblePosition, set targetCharacter agar diikuti secara real-time
        var dynamicBubble = bubblePanel.GetComponent<DynamicBubblePosition>();
        if (dynamicBubble != null)
        {
            dynamicBubble.targetCharacter = speakerGo != null ? speakerGo.transform : null;
        }

        // Debug Log untuk mempermudah pelacakan posisi bubble
        if (speakerGo != null)
        {
            Debug.Log($"[DialogueManager] Memosisikan bubble chat untuk '{characterName}' pada GameObject '{speakerGo.name}' di posisi {speakerGo.transform.position}");
        }
        else
        {
            Debug.LogWarning($"[DialogueManager] Tidak dapat menemukan GameObject untuk pembicara '{characterName}'.");
        }

        if (speakerGo != null && Camera.main != null)
        {
            // Offset Y agar bubble berada di atas kepala karakter (sesuaikan dengan tinggi sprite Anda)
            Vector3 worldOffset = new Vector3(0, 2.0f, 0); 
            
            // Dapatkan referensi Canvas parent untuk mendeteksi Render Mode
            Canvas parentCanvas = bubblePanel.GetComponentInParent<Canvas>();
            
            if (parentCanvas != null)
            {
                if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    // Konversi posisi dunia ke posisi layar (Screen Space)
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(speakerGo.transform.position + worldOffset);
                    bubblePanel.transform.position = screenPos;
                }
                else if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    // Konversi posisi dunia ke posisi layar (Screen Space)
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(speakerGo.transform.position + worldOffset);
                    
                    // Konversi posisi layar ke posisi world relatif terhadap Camera Canvas
                    if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        bubblePanel.transform.parent as RectTransform,
                        screenPos,
                        parentCanvas.worldCamera != null ? parentCanvas.worldCamera : Camera.main,
                        out Vector3 worldPos))
                    {
                        bubblePanel.transform.position = worldPos;
                    }
                }
                else if (parentCanvas.renderMode == RenderMode.WorldSpace)
                {
                    // Untuk World Space Canvas, tempatkan langsung di koordinat dunia karakter + offset
                    bubblePanel.transform.position = speakerGo.transform.position + worldOffset;
                }
            }
            else
            {
                // Fallback jika tidak ada Canvas parent
                Vector3 screenPos = Camera.main.WorldToScreenPoint(speakerGo.transform.position + worldOffset);
                bubblePanel.transform.position = screenPos;
            }
        }
    }
}