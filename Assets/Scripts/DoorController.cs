using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    [SerializeField] private string nextSceneName; 
    [SerializeField] private BoxCollider2D transitionZoneCollider; 
    [SerializeField] private string doorID; 
    
    // TAMBAHAN: Flag untuk membedakan door yang perlu puzzle
    [SerializeField] private bool requiresPuzzle = false;

    private BoxCollider2D solidCollider;
    private AudioSource audioSource;
    private bool isOpen = false;

    private void Awake()
    {
        solidCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>(); 

        // CEK: Apakah pintu ini sudah pernah dibuka sebelumnya?
        if (GameManager.Instance != null && GameManager.Instance.IsDoorOpened(doorID))
        {
            isOpen = true;
            ApplyOpenState();
            Debug.Log("Pintu " + doorID + " restore dari state sebelumnya (TERBUKA)");
        }
        // CEK: Jika door tidak perlu puzzle, langsung buka
        else if (!requiresPuzzle)
        {
            OpenDoor();
            Debug.Log("Pintu " + doorID + " dibuka otomatis (tidak perlu puzzle)");
        }
    }

    public void OpenDoor()
    {
        isOpen = true;
        
        // SIMPAN STATUS KE GAMEMANAGER
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetDoorOpened(doorID, true);
        }
        
        if (audioSource != null) audioSource.Play();
        ApplyOpenState();
        
        Debug.Log("Pintu Terbuka! Sensor BoxCollider2D pada Transition Zone BERHASIL DIAKTIFKAN.");
    }

    // Method terpisah untuk apply state terbuka
    private void ApplyOpenState()
    {
        if (solidCollider != null) solidCollider.enabled = false;
        
        if (transitionZoneCollider != null) 
        {
            transitionZoneCollider.gameObject.SetActive(true);
            transitionZoneCollider.enabled = true;
        }
        else
        {
            Debug.LogError("Gagal mengaktifkan sensor! Slot 'Transition Zone Collider' di Inspector masih kosong.");
        }
    }

    public void GoToNextScene()
    {
        if (isOpen)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.lastExitDoorID = doorID;
            }

            SceneManager.LoadScene(nextSceneName);
        }
    }
}