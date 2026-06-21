using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Buttons")]
    public GameObject continueButton; // Tempat memasukkan tombol Continue

    void Start()
    {
        // Saat menu dibuka, cek apakah ada data save bernama "SavedScene"
        if (PlayerPrefs.HasKey("SavedScene"))
        {
            // Jika ada, munculkan tombol Continue
            continueButton.SetActive(true);
        }
        else
        {
            // Jika belum ada save sama sekali, sembunyikan tombol Continue
            continueButton.SetActive(false);
        }
    }

    // Dipanggil oleh tombol New Game / Mulai
    public void PlayNewGame()
    {
        // Hapus seluruh data save lama agar bersih dari awal
        PlayerPrefs.DeleteAll(); 
        PlayerPrefs.Save();

        SceneManager.LoadScene("chapter1_kamar"); 
    }

    // Dipanggil oleh tombol Continue / Lanjutkan
    public void ContinueGame()
    {
        // Ambil nama scene yang terakhir kali disimpan
        string sceneToLoad = PlayerPrefs.GetString("SavedScene");
        
        // Muat scene tersebut
        SceneManager.LoadScene(sceneToLoad);
    }

    // Dipanggil oleh tombol Quit
    public void QuitGame()
    {
        Debug.Log("Game Keluar!");
        Application.Quit();
    }
}