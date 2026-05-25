using UnityEngine;
using UnityEngine.InputSystem; // Jika menggunakan New Input System

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue; // Tempat memasukkan data dialog di Inspector

    private bool playerInRange = false;

    void Update()
    {
        // Cek apakah pemain menekan tombol 'E' (Interact) dan berada di dekat objek
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        // Memanggil Manager untuk memulai percakapan
        DialogueManager.instance.StartDialogue(dialogue);
    }

    // Mendeteksi apakah Samosir ada di dekat objek ini (Butuh BoxCollider2D mode IsTrigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = false;
    }
}