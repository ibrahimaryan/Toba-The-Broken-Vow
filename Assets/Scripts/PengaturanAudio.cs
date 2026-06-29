using UnityEngine;
using UnityEngine.UI; // Wajib jika ingin mengubah ikon tombol nanti

public class PengaturanAudio : MonoBehaviour
{
    private bool isMuted = false;

    public void ToggleMute()
    {
        isMuted = !isMuted;
        
        // AudioListener mengatur volume Telinga (Kamera) di Unity
        // Jika isMuted true, volume jadi 0 (bisu). Jika false, volume jadi 1 (normal).
        AudioListener.volume = isMuted ? 0f : 1f; 
        
        Debug.Log("Status Mute: " + isMuted);
    }
}