using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Chapter3PuzzleManager : MonoBehaviour
{
    [Header("Puzzle Slots")]
    [SerializeField] private PuzzleSlot[] slots; // List 3 slot penyusunan (biasanya diatur vertikal)

    [Header("Puzzle Items")]
    [SerializeField] private Chapter3PuzzleItem[] puzzleItems; // List 3 item puzzle yang diseret/diputar

    [System.Serializable]
    public struct PuzzleSet
    {
        public string setName; // e.g. "ikan", "sawit", "pancing"
        public Sprite pieceSprite0;
        public Sprite pieceSprite1;
        public Sprite pieceSprite2;
    }

    [Header("Puzzle Sets (Motif)")]
    [SerializeField] private PuzzleSet[] puzzleSets; // 3 set: ikan, sawit, pancing

    [Header("UI Panels")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject submitButton;

    [Header("Axe Reward Settings")]
    [Tooltip("GameObject Kapak di scene yang akan diaktifkan setelah puzzle selesai")]
    [SerializeField] private GameObject axeWorldGameObject;

    [Header("Rantang Objects Settings")]
    [Tooltip("GameObject Rantang Trigger di scene yang akan dinonaktifkan setelah puzzle selesai")]
    [SerializeField] private GameObject rantangTriggerObject;

    [Tooltip("GameObject Rantang Solved di scene yang akan diaktifkan setelah puzzle selesai")]
    [SerializeField] private GameObject rantangSolvedObject;

    [Header("Events On Success")]
    [SerializeField] private UnityEvent OnPuzzleSolved;

    [Header("Dialogue (Optional)")]
    [SerializeField] private Dialogue failDialogue; // Dialog saat susunan masih salah
    [SerializeField] private Dialogue successDialogue; // Dialog saat puzzle berhasil diselesaikan

    [Header("Audio Settings")]
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private bool hasRandomized = false;

    // Menyimpan parent awal asli dari masing-masing item sebelum diacak
    private Dictionary<Chapter3PuzzleItem, Transform> defaultParents = new Dictionary<Chapter3PuzzleItem, Transform>();

    private void Start()
    {
        // Catat parent bawaan untuk keperluan reset penuh jika dibutuhkan
        foreach (var item in puzzleItems)
        {
            if (item != null)
            {
                defaultParents[item] = item.transform.parent;
            }
        }

        // PENGAMAN: Jika puzzle sudah selesai, pastikan status visual rantang terupdate
        if (GameManager.Instance != null && GameManager.Instance.IsFlagSet("chapter3_puzzle_solved"))
        {
            if (rantangTriggerObject != null)
            {
                rantangTriggerObject.SetActive(false);
                Debug.Log("Start: Rantang Trigger dinonaktifkan karena chapter3_puzzle_solved.");
            }

            if (rantangSolvedObject != null)
            {
                rantangSolvedObject.SetActive(true);
                Debug.Log("Start: Rantang Solved diaktifkan karena chapter3_puzzle_solved.");
            }
        }
    }

    public void OpenPuzzle()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
            
            // Matikan pergerakan player
            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null) player.ToggleInput(false);

            // Acak posisi item ke slot dan acak rotasinya hanya saat pertama kali dibuka
            if (!hasRandomized)
            {
                RandomizePuzzleState();
                hasRandomized = true;
            }
        }
    }

    private void RandomizePuzzleState()
    {
        Debug.Log("Mengacak susunan item puzzle Chapter 3...");

        // 0. Pilih motif puzzle secara acak jika ada set motif yang ditentukan
        if (puzzleSets != null && puzzleSets.Length > 0 && puzzleItems.Length >= 3)
        {
            PuzzleSet chosenSet = puzzleSets[Random.Range(0, puzzleSets.Length)];
            Debug.Log($"Motif puzzle terpilih secara acak: {chosenSet.setName}");

            // Pasang sprite motif ke masing-masing item puzzle
            AssignSpriteToItem(puzzleItems[0], chosenSet.pieceSprite0);
            AssignSpriteToItem(puzzleItems[1], chosenSet.pieceSprite1);
            AssignSpriteToItem(puzzleItems[2], chosenSet.pieceSprite2);
        }

        if (slots.Length != puzzleItems.Length)
        {
            Debug.LogWarning("Jumlah slot dan item puzzle tidak sama! Pengacakan mungkin tidak sempurna.");
        }

        // 1. Acak penempatan slot menggunakan Fisher-Yates Shuffle
        List<int> slotIndices = new List<int>();
        for (int i = 0; i < slots.Length; i++)
        {
            slotIndices.Add(i);
        }

        // Lakukan pengacakan list index slot
        for (int i = 0; i < slotIndices.Count; i++)
        {
            int temp = slotIndices[i];
            int randomIndex = Random.Range(i, slotIndices.Count);
            slotIndices[i] = slotIndices[randomIndex];
            slotIndices[randomIndex] = temp;
        }

        // 2. Tempatkan item ke slot hasil pengacakan dan berikan rotasi acak
        float[] rotations = new float[] { -90f, -180f, -270f }; // Sudut miring acak (tidak boleh 0/lurus)

        for (int i = 0; i < puzzleItems.Length; i++)
        {
            if (puzzleItems[i] == null) continue;

            // Pastikan tidak index out of bounds
            int slotIdx = slotIndices[i % slots.Length];
            PuzzleSlot targetSlot = slots[slotIdx];

            if (targetSlot != null)
            {
                // Set parent ke slot baru
                puzzleItems[i].transform.SetParent(targetSlot.transform);
                
                // Atur posisi tepat di tengah slot
                RectTransform rect = puzzleItems[i].GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero;
                }

                // Perbarui state internal item agar drag berikutnya bekerja dari slot baru ini
                puzzleItems[i].parentAfterDrag = targetSlot.transform;
                puzzleItems[i].originalParent = targetSlot.transform;

                // Acak rotasi
                float randomAngle = rotations[Random.Range(0, rotations.Length)];
                puzzleItems[i].targetRotationAngle = randomAngle;
                rect.localRotation = Quaternion.Euler(0, 0, randomAngle);
            }
        }
    }

    // Panggil fungsi ini lewat button "Check" atau "Selesai" di UI
    public void CheckSolution()
    {
        bool allCorrect = true;

        foreach (PuzzleSlot slot in slots)
        {
            if (slot == null) continue;

            // 1. Cek apakah slot memiliki item
            if (slot.transform.childCount == 0)
            {
                allCorrect = false;
                break;
            }

            // 2. Ambil script Chapter3PuzzleItem dari anak slot
            Chapter3PuzzleItem placedItem = slot.GetComponentInChildren<Chapter3PuzzleItem>();
            
            // Cek apakah item benar
            if (placedItem == null || placedItem.itemID != slot.correctItemID)
            {
                allCorrect = false;
                break;
            }

            // 3. Cek apakah rotasi item sudah lurus (0 derajat)
            if (!placedItem.IsRotationCorrect())
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            if (ToDoManager.Instance != null)
                {
                    // Angka 1 berarti mencoret misi urutan KEDUA di daftar misi Chapter tersebut
                    ToDoManager.Instance.SelesaikanMisi(0); 
                }
            Debug.Log("Puzzle Chapter 3 Sukses! Pola tersusun rapi.");
            HandleSuccess();
        }
        else
        {
            Debug.Log("Pola susunan masih salah atau miring!");
            HandleFailure();
        }
    }

    private void HandleSuccess()
    {
        // Tutup panel puzzle
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        // Putar suara sukses
        if (audioSource != null && correctSound != null)
        {
            audioSource.PlayOneShot(correctSound);
        }

        // Aktifkan Game Object Kapak di dunia game
        if (axeWorldGameObject != null)
        {
            axeWorldGameObject.SetActive(true);
            Debug.Log("GameObject Kapak di-spawn/diaktifkan di scene!");
        }
        else
        {
            Debug.LogWarning("Reward GameObject Kapak belum di-assign di Inspector!");
        }

        // Simpan status puzzle selesai ke GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag("chapter3_puzzle_solved", true);
        }

        if (Chapter3StoryManager.Instance != null)
        {
            Chapter3StoryManager.Instance.UpdateObjectivePointer();
        }

        // Nonaktifkan trigger rantang lama
        if (rantangTriggerObject != null)
        {
            rantangTriggerObject.SetActive(false);
            Debug.Log("Rantang Trigger dinonaktifkan.");
        }

        // Aktifkan rantang solved baru
        if (rantangSolvedObject != null)
        {
            rantangSolvedObject.SetActive(true);
            Debug.Log("Rantang Solved diaktifkan.");
        }

        // Kembalikan pergerakan player atau pertahankan lock jika ada dialog
        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (successDialogue != null && DialogueManager.instance != null)
        {
            if (player != null) player.ToggleInput(false); // Lock gerakan player saat dialog sukses
            DialogueManager.instance.StartDialogue(successDialogue);
            StartCoroutine(WaitDialogueAndUnlockShard(player));
        }
        else
        {
            if (player != null) player.ToggleInput(true);
            UnlockMemoryShardIfAny();
        }

        // Jalankan event sukses
        OnPuzzleSolved?.Invoke();
    }

    private void UnlockMemoryShardIfAny()
    {
        if (MemoryShardManager.Instance != null)
        {
            MemoryShardManager.Instance.UnlockShard("Chapter3");
        }
    }

    private System.Collections.IEnumerator WaitDialogueAndUnlockShard(PlayerControllerScript player)
    {
        yield return null; // Tunggu satu frame agar dialog sempat aktif

        while (DialogueManager.instance != null && 
               ((DialogueManager.instance.screenBoxPanel != null && DialogueManager.instance.screenBoxPanel.activeInHierarchy) ||
                (DialogueManager.instance.bubblePanel != null && DialogueManager.instance.bubblePanel.activeInHierarchy) ||
                (DialogueManager.instance.cutsceneBoxPanel != null && DialogueManager.instance.cutsceneBoxPanel.activeInHierarchy)))
        {
            yield return null;
        }

        UnlockMemoryShardIfAny();

        if (player != null && MemoryShardManager.Instance == null)
        {
            player.ToggleInput(true);
        }
    }

    private void HandleFailure()
    {
        // Putar suara gagal
        if (audioSource != null && wrongSound != null)
        {
            audioSource.PlayOneShot(wrongSound);
        }

        // Tampilkan dialog gagal jika dipasang
        if (failDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(failDialogue);
        }
        else
        {
            Debug.Log("Gagal menyusun pola! Coba lagi.");
        }
        
        // Kita tidak mereset penuh posisi item agar player tidak frustrasi, 
        // tapi jika ingin reset pengacakan kembali, panggil ResetAllItems()
    }

    public void ResetAllItems()
    {
        foreach (var item in puzzleItems)
        {
            if (item != null)
            {
                item.ResetToOriginalState();
            }
        }
        hasRandomized = false;
        RandomizePuzzleState();
        hasRandomized = true;
    }

    public void ClosePuzzlePanel()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        // Nyalakan kembali input player
        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null) player.ToggleInput(true);
    }

    private void Update()
    {
        // Memungkinkan keluar dengan ESC
        if (puzzlePanel != null && puzzlePanel.activeSelf)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ClosePuzzlePanel();
                PauseMenuManager.PanelWasClosedThisFrame = true;
            }
        }
    }

    private void AssignSpriteToItem(Chapter3PuzzleItem item, Sprite sprite)
    {
        if (item == null || sprite == null) return;

        UnityEngine.UI.Image img = item.GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            img.sprite = sprite;
        }
        else
        {
            SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = sprite;
            }
        }
    }

    public bool IsPanelActive()
    {
        return puzzlePanel != null && puzzlePanel.activeSelf;
    }
}

