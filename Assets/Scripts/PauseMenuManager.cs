using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Memanggil New Input System

public class PauseMenuManager : MonoBehaviour
{
    // Variabel statis agar script lain tahu game sedang di-pause atau tidak
    public static bool GameIsPaused = false; 
    public static bool PanelWasClosedThisFrame = false;

    [Header("Referensi UI")]
    public GameObject pauseMenuUI;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Pastikan UI Pause selalu ditutup dan waktu kembali normal saat pindah/muat scene baru
        Resume();
    }

    void Update()
    {
        // Mengecek apakah tombol Escape ditekan
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (PanelWasClosedThisFrame)
            {
                return;
            }

            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                // Jangan jeda game jika ada panel overlay lain yang sedang aktif
                if (IsAnyOtherPanelActive())
                {
                    return;
                }
                Pause();
            }
        }
    }

    private void LateUpdate()
    {
        PanelWasClosedThisFrame = false;
    }

    private bool IsAnyOtherPanelActive()
    {
        // 1. Check Inventory
        if (InventoryManager.Instance != null && InventoryManager.Instance.IsInventoryOpen())
        {
            return true;
        }

        // 2. Check PasswordTerminal panels
        PasswordTerminal[] terminals = FindObjectsByType<PasswordTerminal>(FindObjectsSortMode.None);
        foreach (var terminal in terminals)
        {
            if (terminal != null && terminal.IsPanelActive())
            {
                return true;
            }
        }

        // 3. Check SisikPuzzle panels
        SisikPuzzleManager[] sisikManagers = FindObjectsByType<SisikPuzzleManager>(FindObjectsSortMode.None);
        foreach (var sm in sisikManagers)
        {
            if (sm != null && sm.IsPanelActive())
            {
                return true;
            }
        }

        // 4. Check Chapter 3 puzzle panel
        Chapter3PuzzleManager[] c3Managers = FindObjectsByType<Chapter3PuzzleManager>(FindObjectsSortMode.None);
        foreach (var c3m in c3Managers)
        {
            if (c3m != null && c3m.IsPanelActive())
            {
                return true;
            }
        }

        // 5. Check BambooPuzzle panel
        BambooPuzzleManager[] bambooManagers = FindObjectsByType<BambooPuzzleManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var bm in bambooManagers)
        {
            if (bm != null && bm.IsPanelActive())
            {
                return true;
            }
        }

        // 6. Check ToDo list
        if (ToDoManager.Instance != null && ToDoManager.Instance.IsToDoListOpen())
        {
            return true;
        }

        // 7. Check EquipmentPickupTrigger panels
        EquipmentPickupTrigger[] equipPickups = FindObjectsByType<EquipmentPickupTrigger>(FindObjectsSortMode.None);
        foreach (var ep in equipPickups)
        {
            if (ep != null && ep.IsPanelActive())
            {
                return true;
            }
        }

        // 8. Check AxePickupTrigger panels
        AxePickupTrigger[] axePickups = FindObjectsByType<AxePickupTrigger>(FindObjectsSortMode.None);
        foreach (var ap in axePickups)
        {
            if (ap != null && ap.IsPanelActive())
            {
                return true;
            }
        }

        // 9. Check Chapter4StoryManager panels
        Chapter4StoryManager[] c4StoryManagers = FindObjectsByType<Chapter4StoryManager>(FindObjectsSortMode.None);
        foreach (var c4sm in c4StoryManagers)
        {
            if (c4sm != null && c4sm.IsAnyPanelActive())
            {
                return true;
            }
        }

        // 10. Check SecretItem panels
        SecretItem[] secretItems = FindObjectsByType<SecretItem>(FindObjectsSortMode.None);
        foreach (var si in secretItems)
        {
            if (si != null && si.IsPanelActive())
            {
                return true;
            }
        }

        return false;
    }

    // Fungsi untuk melanjutkan game
    public void Resume()
    {
        pauseMenuUI.SetActive(false); // Sembunyikan UI Menu
        Time.timeScale = 1f;          // Kembalikan waktu berjalan normal (1x speed)
        GameIsPaused = false;
    }

    // Fungsi untuk menjeda game
    // Fungsi untuk menjeda game (sekarang sudah public!)
    public void Pause()
    {
        pauseMenuUI.SetActive(true);  
        Time.timeScale = 0f;          
        GameIsPaused = true;
    }

    // Fungsi untuk kembali ke Main Menu
    public void LoadMainMenu()
    {
        // SANGAT PENTING: Kembalikan waktu ke normal sebelum pindah scene
        // Jika tidak, Main Menu kamu akan ikut membeku!
        Time.timeScale = 1f; 
        GameIsPaused = false;
        
        // Pindah ke Main Menu
        SceneManager.LoadScene("MainMenu"); 
    }
}