using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReturnHelper : MonoBehaviour
{
    // Fungsi ini dipanggil dari UnityEvent CutsceneManager saat video tuntas
    public void KembaliKeChapterMula()
    {
        // Panggil kembali scene asal yang kita simpan tadi. 
        // Jika karena suatu hal tidak tersimpan, default ke Chapter 1
        string sceneSemula = PlayerPrefs.GetString("ReturnSceneName", "Chapter 1");
        SceneManager.LoadScene(sceneSemula);
    }
}