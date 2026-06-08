using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Memanggil New Input System

public class PauseMenuManager : MonoBehaviour
{
    // Variabel statis agar script lain tahu game sedang di-pause atau tidak
    public static bool GameIsPaused = false; 

    [Header("Referensi UI")]
    public GameObject pauseMenuUI;

    void Update()
    {
        // Mengecek apakah tombol Escape ditekan
        // if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        // {
        //     if (GameIsPaused)
        //     {
        //         Resume();
        //     }
        //     else
        //     {
        //         Pause();
        //     }
        // }
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