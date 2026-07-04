using UnityEngine;
using UnityEngine.UI; // Wajib dipanggil untuk memanipulasi UI (Image, Button, dll)

public class PengaturanAudio : MonoBehaviour
{
    [Header("Pengaturan Visual Tombol")]
    [Tooltip("Masukkan komponen Image dari tombol Mute di sini")]
    public Image targetIkon; 
    
    [Tooltip("Gambar saat suara menyala normal")]
    public Sprite ikonSoundOn; 
    
    [Tooltip("Gambar saat suara dimatikan (silang)")]
    public Sprite ikonSoundOff;

    private bool isMuted = false;

    public void ToggleMute()
    {
        // Membalikkan status (true jadi false, false jadi true)
        isMuted = !isMuted;
        
        // Mengubah volume global Unity
        AudioListener.volume = isMuted ? 0f : 1f; 
        
        // Mengubah gambar (sprite) ikon pada tombol
        if (targetIkon != null)
        {
            // Jika isMuted true, pakai ikonSoundOff. Jika false, pakai ikonSoundOn.
            targetIkon.sprite = isMuted ? ikonSoundOff : ikonSoundOn;
        }
    }
}