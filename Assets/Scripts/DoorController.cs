using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    [SerializeField] private string nextSceneName; 
    
    // PERBAIKAN: Langsung kunci ke komponen BoxCollider2D milik Transition Zone
    [SerializeField] private BoxCollider2D transitionZoneCollider; 
    
    [SerializeField] private string doorID; 

    private BoxCollider2D solidCollider;
    private AudioSource audioSource;
    private bool isOpen = false;

    private void Awake()
    {
        solidCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>(); 
    }

    public void OpenDoor()
    {
        isOpen = true;
        
        if (audioSource != null) audioSource.Play();
        if (solidCollider != null) solidCollider.enabled = false; // Matikan tembok padat
        
        // PERBAIKAN: Nyalakan langsung collider-nya secara instan
        if (transitionZoneCollider != null) 
        {
            Debug.Log("Transition Active: " + transitionZoneCollider.gameObject.activeInHierarchy);
            Debug.Log("Collider Enabled: " + transitionZoneCollider.enabled);
            // Nyalakan GameObject tempat kolider itu berada jika statusnya mati
            transitionZoneCollider.gameObject.SetActive(true); 
            
            // Nyalakan fungsi trigger fisika
            transitionZoneCollider.enabled = true;
            
            Debug.Log("Pintu Terbuka! Sensor BoxCollider2D pada Transition Zone BERHASIL DIAKTIFKAN.");
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