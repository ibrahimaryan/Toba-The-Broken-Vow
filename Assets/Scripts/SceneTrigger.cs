using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    // Kita buat slot manual agar kamu bisa tarik langsung objek Gate-nya di Inspector
    [SerializeField] private DoorController doorController; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah yang menyentuh adalah Player
        if (other.CompareTag("Player"))
        {
            Debug.Log("ZONA TRANSISI TERKONTAMINASI PLAYER! Mencoba pindah scene...");

            // Cek jika slot sudah diisi
            if (doorController != null)
            {
                doorController.GoToNextScene();
            }
            else
            {
                // Jika lupa ditarik, script akan mencoba mencari otomatis ke atas (parent)
                doorController = GetComponentInParent<DoorController>();
                
                if (doorController != null)
                {
                    doorController.GoToNextScene();
                }
                else
                {
                    Debug.LogError("Error: DoorController tidak ditemukan! Tarik objek Gate ke slot Door Controller di Inspector objek ini.");
                }
            }
        }
    }
}