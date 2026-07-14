using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class BambooPuzzleManager : MonoBehaviour
{
    [Header("Grid Size Settings")]
    private const int GRID_WIDTH = 3;
    private const int GRID_HEIGHT = 4;

    [Header("Start & End Positions")]
    [Tooltip("Daftar koordinat Start (sumber air)")]
    [SerializeField] private List<Vector2Int> startPositions = new List<Vector2Int> { new Vector2Int(0, 3), new Vector2Int(2, 3) };
    [Tooltip("Daftar koordinat End (target air)")]
    [SerializeField] private List<Vector2Int> endPositions = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(2, 0) };

    [Header("Turn System Settings")]
    [SerializeField] private int maxTurns = 12;
    [SerializeField] private TMP_Text turnText; // UI Text sisa langkah

    [Header("UI Panels")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private GameObject failPanel; // Panel dengan tombol Retry
    [SerializeField] private GameObject winPanel;  // Panel Sukses

    [Header("Audio Settings")]
    [SerializeField] private AudioClip rotateSound;
    [SerializeField] private AudioClip retrySound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failSound;

    [Header("Events On Success")]
    [SerializeField] public UnityEvent OnPuzzleSolved; // Aksi setelah puzzle berhasil dipecahkan

    public bool IsPuzzlePanelActive => puzzlePanel != null && puzzlePanel.activeSelf;

    private AudioSource audioSource;
    private BambooPipeTile[,] pipeGrid = new BambooPipeTile[GRID_WIDTH, GRID_HEIGHT];
    private int[,] correctRotations = new int[GRID_WIDTH, GRID_HEIGHT];
    private int currentTurnsLeft;
    private bool isGameFinished = false;

    // Arah pergerakan dalam grid koordinat (Top, Right, Bottom, Left)
    // 0 = Top (Y bertambah), 1 = Right (X bertambah), 2 = Bottom (Y berkurang), 3 = Left (X berkurang)
    private readonly Vector2Int[] directionOffsets = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // 0: Top
        new Vector2Int(1, 0),   // 1: Right
        new Vector2Int(0, -1),  // 2: Bottom
        new Vector2Int(-1, 0)   // 3: Left
    };

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Cari semua objek bertipe BambooPipeTile di dalam anak Manager ini dan petakan ke grid 2D
        BambooPipeTile[] childTiles = GetComponentsInChildren<BambooPipeTile>();
        foreach (BambooPipeTile tile in childTiles)
        {
            if (tile.gridX >= 0 && tile.gridX < GRID_WIDTH && tile.gridY >= 0 && tile.gridY < GRID_HEIGHT)
            {
                if (pipeGrid[tile.gridX, tile.gridY] != null)
                {
                    Debug.LogError($"[BambooPuzzleManager] Koordinat ({tile.gridX}, {tile.gridY}) digunakan GANDA oleh '{pipeGrid[tile.gridX, tile.gridY].gameObject.name}' dan '{tile.gameObject.name}'! Pastikan Grid X dan Y setiap ubin di-set unik di Inspector.");
                }
                pipeGrid[tile.gridX, tile.gridY] = tile;
            }
            else
            {
                Debug.LogWarning($"Pipa {tile.gameObject.name} memiliki koordinat grid ({tile.gridX}, {tile.gridY}) di luar batas grid 3x4!");
            }
        }

        // Cek jika ada koordinat grid yang belum terisi ubin pipa
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (pipeGrid[x, y] == null)
                {
                    Debug.LogWarning($"[BambooPuzzleManager] Grid ({x}, {y}) kosong! Tidak ada pipa yang terhubung ke koordinat ini di Inspector.");
                }
            }
        }

        // Set kunci jawaban rotasi yang benar (sesuai layout visual yang terpecahkan)
        // Baris Y = 3 (Atas)
        correctRotations[0, 3] = 0; // L-Bend (Top-Right)
        correctRotations[1, 3] = 0; // Cross (Apapun cocok)
        correctRotations[2, 3] = 2; // T-Bend (Top-Bottom-Left)

        // Baris Y = 2
        correctRotations[0, 2] = 1; // L-Bend (Right-Bottom)
        correctRotations[1, 2] = 3; // T-Bend (Left-Right-Top)
        correctRotations[2, 2] = 3; // L-Bend (Left-Top)

        // Baris Y = 1
        correctRotations[0, 1] = 0; // T-Bend (Top-Right-Bottom)
        correctRotations[1, 1] = 1; // Straight (Horizontal)
        correctRotations[2, 1] = 2; // L-Bend (Left-Bottom)

        // Baris Y = 0 (Bawah)
        correctRotations[0, 0] = 0; // T-Bend (Top-Right-Bottom)
        correctRotations[1, 0] = 1; // Straight (Horizontal)
        correctRotations[2, 0] = 2; // T-Bend (Top-Left-Bottom)
    }

    private void Start()
    {
        // DEBUG DIAGNOSTIK: Cetak peta grid ke Console untuk memastikan mapping koordinat benar
        Debug.Log("[BambooPuzzleManager] --- MEMULAI DIAGNOSTIK GRID ---");
        for (int y = GRID_HEIGHT - 1; y >= 0; y--)
        {
            string rowStr = $"Baris {y}: ";
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                if (pipeGrid[x, y] != null)
                {
                    rowStr += $"[Tile '{pipeGrid[x, y].gameObject.name}', Type={pipeGrid[x, y].pipeType}, Locked={pipeGrid[x, y].isLocked}] | ";
                }
                else
                {
                    rowStr += $"[KOSONG/NULL] | ";
                }
            }
            Debug.Log(rowStr);
        }
        string starts = string.Join(", ", startPositions);
        string ends = string.Join(", ", endPositions);
        Debug.Log($"[BambooPuzzleManager] StartPos=[{starts}], EndPos=[{ends}]");
        Debug.Log("[BambooPuzzleManager] ---------------------------------");

        // Jika panel puzzle diset aktif langsung dari awal, inisialisasi
        if (puzzlePanel != null && puzzlePanel.activeSelf)
        {
            OpenPuzzle();
        }
    }

    private void Update()
    {
        // Logika keluar panel dengan tombol ESC menggunakan New Input System (seperti di SisikPuzzleManager)
        bool isAnyPanelActive = (puzzlePanel != null && puzzlePanel.activeSelf);
        if (isAnyPanelActive)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (isGameFinished)
                {
                    // Jika sudah selesai dan player tekan ESC, tutup panel dan picu dialog/cerita
                    ClosePuzzlePanel();
                    OnPuzzleSolved?.Invoke();
                }
                else
                {
                    // Jika belum selesai, tutup panel saja (keluar biasa)
                    ClosePuzzlePanel();
                }
                PauseMenuManager.PanelWasClosedThisFrame = true;
            }
        }
    }

    private bool isInitialized = false;

    // Panggil fungsi ini lewat interaksi di dunia game (misal: Trigger)
    public void OpenPuzzle()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
            
            // Matikan pergerakan player
            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null) player.ToggleInput(false);

            // Reset status game (kecuali jika sudah solved di GameManager)
            isGameFinished = GameManager.Instance != null && GameManager.Instance.IsFlagSet("bamboo_pipe_puzzle_solved");

            if (failPanel != null) failPanel.SetActive(false);
            if (winPanel != null) winPanel.SetActive(false);

            if (!isInitialized)
            {
                InitializePuzzle();
                isInitialized = true;
            }
            else
            {
                // Jika sudah pernah diinisialisasi sebelumnya, pertahankan sisa langkah & rotasi pipa
                UpdateTurnUI();
                UpdateWaterFlow();
            }
        }
    }

    // Inisialisasi langkah & acak rotasi pipa yang tidak terkunci
    public void InitializePuzzle()
    {
        currentTurnsLeft = maxTurns;
        UpdateTurnUI();

        // Acak pipa
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (pipeGrid[x, y] != null)
                {
                    pipeGrid[x, y].SetFilled(false);
                    if (!pipeGrid[x, y].isLocked)
                    {
                        pipeGrid[x, y].Shuffle();
                    }
                }
            }
        }

        // Update aliran air awal setelah diacak
        UpdateWaterFlow();
    }

    // Dipanggil dari BambooPipeTile ketika diklik oleh player
    public void OnPipeClicked(BambooPipeTile clickedTile)
    {
        if (isGameFinished || currentTurnsLeft <= 0) return;

        // Putar pipa
        clickedTile.RotatePipe();

        // Putar SFX Putaran
        PlaySound(rotateSound);

        // Kurangi langkah
        currentTurnsLeft--;
        UpdateTurnUI();

        // Hitung ulang aliran air
        UpdateWaterFlow();

        // Cek Solved State
        if (CheckPuzzleSolved())
        {
            HandleSuccess();
        }
        else if (currentTurnsLeft <= 0)
        {
            HandleFailure();
        }
    }

    // Hitung ulang air mengalir dari Start ke End menggunakan BFS
    // Hitung ulang air mengalir dari Start ke End menggunakan Row-by-Row Propagation
    private void UpdateWaterFlow()
    {
        // Pertama, matikan semua status terisi air di grid
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (pipeGrid[x, y] != null)
                {
                    pipeGrid[x, y].SetFilled(false);
                }
            }
        }

        // Proses baris demi baris dari atas (GRID_HEIGHT - 1) ke bawah (0)
        for (int y = GRID_HEIGHT - 1; y >= 0; y--)
        {
            // LANGKAH 1: Cek aliran air dari atas (Prioritas Pertama)
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                BambooPipeTile tile = pipeGrid[x, y];
                if (tile == null) continue;

                if (y == GRID_HEIGHT - 1)
                {
                    // Baris paling atas: Hanya aktif jika merupakan startPosition dan terhubung ke atas (arah 0)
                    if (startPositions.Contains(new Vector2Int(x, y)))
                    {
                        if (tile.GetActiveConnections().Contains(0))
                        {
                            tile.SetFilled(true);
                        }
                    }
                }
                else
                {
                    // Baris di bawahnya: Cek apakah ubin di atasnya aktif, memiliki koneksi ke bawah (arah 2),
                    // dan ubin saat ini memiliki koneksi ke atas (arah 0)
                    BambooPipeTile aboveTile = pipeGrid[x, y + 1];
                    if (aboveTile != null && aboveTile.IsFilled())
                    {
                        if (aboveTile.GetActiveConnections().Contains(2) && tile.GetActiveConnections().Contains(0))
                        {
                            tile.SetFilled(true);
                        }
                    }
                }
            }

            // LANGKAH 2: Rambatkan air ke samping (Prioritas Kedua)
            // Lakukan perulangan sebanyak GRID_WIDTH kali untuk memastikan air merambat ke seluruh ubin yang terhubung di baris tersebut
            for (int step = 0; step < GRID_WIDTH; step++)
            {
                for (int x = 0; x < GRID_WIDTH; x++)
                {
                    BambooPipeTile tile = pipeGrid[x, y];
                    if (tile == null || tile.IsFilled()) continue;

                    // Cek tetangga sebelah kiri (x - 1)
                    if (x > 0)
                    {
                        BambooPipeTile leftNeighbor = pipeGrid[x - 1, y];
                        if (leftNeighbor != null && leftNeighbor.IsFilled())
                        {
                            // Jika tetangga kiri terhubung ke kanan (1) dan ubin saat ini terhubung ke kiri (3)
                            if (leftNeighbor.GetActiveConnections().Contains(1) && tile.GetActiveConnections().Contains(3))
                            {
                                tile.SetFilled(true);
                                continue;
                            }
                        }
                    }

                    // Cek tetangga sebelah kanan (x + 1)
                    if (x < GRID_WIDTH - 1)
                    {
                        BambooPipeTile rightNeighbor = pipeGrid[x + 1, y];
                        if (rightNeighbor != null && rightNeighbor.IsFilled())
                        {
                            // Jika tetangga kanan terhubung ke kiri (3) dan ubin saat ini terhubung ke kanan (1)
                            if (rightNeighbor.GetActiveConnections().Contains(3) && tile.GetActiveConnections().Contains(1))
                            {
                                tile.SetFilled(true);
                            }
                        }
                    }
                }
            }
        }

        // DEBUG DIAGNOSTIK: Cetak status IsFilled masing-masing ubin setelah selesai
        Debug.Log("[BambooPuzzleManager] --- HASIL ALIRAN AIR (IsFilled) ---");
        for (int y = GRID_HEIGHT - 1; y >= 0; y--)
        {
            string rowStr = $"Baris {y}: ";
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                if (pipeGrid[x, y] != null)
                {
                    string connStr = string.Join(",", pipeGrid[x, y].GetActiveConnections());
                    rowStr += $"[{pipeGrid[x, y].gameObject.name} (Rot={pipeGrid[x, y].currentRotationIndex}, Conns={connStr}) = {pipeGrid[x, y].IsFilled()}] | ";
                }
                else
                {
                    rowStr += "[KOSONG] | ";
                }
            }
            Debug.Log(rowStr);
        }
        Debug.Log("[BambooPuzzleManager] -------------------------------------");
    }

    // Mengecek apakah seluruh pipa terisi air (berwarna biru) dan posisinya sesuai kunci jawaban
    private bool CheckPuzzleSolved()
    {
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                BambooPipeTile tile = pipeGrid[x, y];
                if (tile != null)
                {
                    // 1. Semua ubin pipa di grid wajib terisi air (biru)
                    if (!tile.IsFilled())
                    {
                        return false;
                    }

                    // 2. Rotasi pipa harus sesuai dengan kunci jawaban
                    if (!IsRotationCorrect(tile, correctRotations[x, y]))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private bool IsRotationCorrect(BambooPipeTile tile, int correctRot)
    {
        if (tile.pipeType == PipeType.Cross)
        {
            return true; // Cross cocok di semua rotasi
        }
        if (tile.pipeType == PipeType.Straight)
        {
            // Pipa lurus vertikal (0 & 2) atau horizontal (1 & 3) adalah identik
            return (tile.currentRotationIndex % 2) == (correctRot % 2);
        }
        // TBend dan LBend memiliki 4 variasi unik, jadi harus sama persis
        return tile.currentRotationIndex == correctRot;
    }

    private void UpdateTurnUI()
    {
        if (turnText != null)
        {
            turnText.text = $"Sisa Langkah: {currentTurnsLeft}";
        }
    }

    private void HandleSuccess()
    {
        isGameFinished = true;
        Debug.Log("Waterpipe Puzzle Berhasil Dipecahkan!");

        PlaySoundPersistent(successSound);

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        // Simpan flag kesuksesan di GameManager jika ada
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag("bamboo_pipe_puzzle_solved", true);
        }
    }

    private void HandleFailure()
    {
        Debug.Log("Langkah habis! Menampilkan opsi retry manual.");
        PlaySound(failSound);

        if (failPanel != null)
        {
            failPanel.SetActive(true);
        }
    }

    // Dipanggil lewat Button UI "Retry" di failPanel
    public void OnClickRetry()
    {
        PlaySound(retrySound);

        if (failPanel != null)
        {
            failPanel.SetActive(false);
        }

        InitializePuzzle();
    }

    // Dipanggil untuk menutup paksa/keluar dari panel puzzle
    public void ClosePuzzlePanel()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (failPanel != null) failPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        // Nyalakan kembali input player
        var player = FindAnyObjectByType<PlayerControllerScript>();
        if (player != null) player.ToggleInput(true);
    }

    private bool IsValidGridPos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < GRID_WIDTH && pos.y >= 0 && pos.y < GRID_HEIGHT;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void PlaySoundPersistent(AudioClip clip)
    {
        if (clip == null) return;
        GameObject tempGO = new GameObject("TempAudio_" + clip.name);
        AudioSource tempSource = tempGO.AddComponent<AudioSource>();
        tempSource.clip = clip;
        if (audioSource != null)
        {
            tempSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
            tempSource.volume = audioSource.volume;
            tempSource.pitch = audioSource.pitch;
            tempSource.spatialBlend = audioSource.spatialBlend;
        }
        else
        {
            tempSource.spatialBlend = 0f; // 2D Sound
        }
        tempSource.Play();
        Destroy(tempGO, clip.length);
    }

    public bool IsPanelActive()
    {
        return (puzzlePanel != null && puzzlePanel.activeSelf) || 
               (failPanel != null && failPanel.activeSelf) || 
               (winPanel != null && winPanel.activeSelf);
    }
}

