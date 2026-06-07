using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSave : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Menyimpan otomatis HANYA ketika Samosir menyentuh area ini
        if (collision.CompareTag("Player"))
        {
            // 1. Simpan Nama Scene
            string currentScene = SceneManager.GetActiveScene().name;
            PlayerPrefs.SetString("SavedScene", currentScene);

            // 2. Simpan Posisi X dan Y Samosir
            PlayerPrefs.SetFloat("SavedPosX", collision.transform.position.x);
            PlayerPrefs.SetFloat("SavedPosY", collision.transform.position.y);

            PlayerPrefs.Save(); // Tulis ke memori
            
            Debug.Log("Game Tersimpan! Scene: " + currentScene);
        }
    }
}