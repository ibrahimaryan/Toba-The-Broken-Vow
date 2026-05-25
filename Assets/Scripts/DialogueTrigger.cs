using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue; // Tempat memasukkan data dialog di Inspector

    [Header("Pengaturan Pemicu")]
    [Tooltip("Centang jika dialog ini hanya boleh muncul satu kali saja saat dilewati.")]
    public bool triggerOnlyOnce = true; 
    
    private bool hasTriggered = false; // Penanda dari sistem apakah dialog sudah pernah jalan

    // Fungsi ini otomatis terpanggil SEKETIKA saat ada objek menyentuh area Box Collider 2D (Is Trigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Mengecek apakah objek yang menyentuh area ini memiliki Tag "Player" (Samosir)
        if (collision.CompareTag("Player"))
        {
            // Jika disetting hanya boleh sekali, dan ternyata sudah pernah jalan, maka batalkan perintah
            if (triggerOnlyOnce && hasTriggered)
            {
                return;
            }

            // Memanggil Manager untuk memulai percakapan
            DialogueManager.instance.StartDialogue(dialogue);
            
            // Menandai bahwa dialog sudah berhasil dijalankan
            hasTriggered = true; 
        }
    }
}