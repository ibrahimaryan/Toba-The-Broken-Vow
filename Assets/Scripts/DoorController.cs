using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    [SerializeField] private string nextSceneName; // Nama scene berikutnya
    [SerializeField] private GameObject transitionZone; // Objek TransitionZone (anak dari Gate)

    private BoxCollider2D solidCollider;
    private AudioSource audioSource;
    private bool isOpen = false;

    private void Awake()
    {
        solidCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>(); // Mengambil komponen Audio Source
    }

    public void OpenDoor()
    {
        isOpen = true;
        
        // 1. Putar Suara Pintu Terbuka
        if (audioSource != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource tidak ditemukan di objek Gate!");
        }

        // 2. Matikan Tembok (Collider Padat) agar Player bisa lewat
        if (solidCollider != null)
        {
            solidCollider.enabled = false;
        }

        // 3. Nyalakan Zona Transisi Pindah Scene
        if (transitionZone != null)
        {
            transitionZone.SetActive(true);
        }

        Debug.Log("Pintu Terbuka (Suara diputar)! Silakan masuk.");
    }

    public void GoToNextScene()
    {
        if (isOpen)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}