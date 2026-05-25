using UnityEngine;
using UnityEngine.SceneManagement; // Wajib dipanggil untuk berpindah Scene

public class MainMenuController : MonoBehaviour
{
    // Fungsi ini akan dipanggil saat tombol Play ditekan
    public void PlayGame()
    {
        // "PrologueScene" adalah nama scene pertama kalian nanti. 
        // Pastikan huruf besar/kecilnya sama persis!
        SceneManager.LoadScene("chapter1_kamar"); 
    }

    // Fungsi ini akan dipanggil saat tombol Quit ditekan
    public void QuitGame()
    {
        Debug.Log("Game Keluar! (Perintah ini hanya terlihat di Editor)");
        Application.Quit(); // Mematikan aplikasi (hanya berfungsi saat game sudah di-build)
    }
}