using UnityEngine;

public class DynamicBubblePosition : MonoBehaviour
{
    [Header("Pengaturan Posisi Lokal")]
    [Tooltip("Posisi Y saat bubble di atas kepala")]
    public Vector3 posisiAtas = new Vector3(0, 1.5f, 0); 
    
    [Tooltip("Posisi Y saat bubble di bawah kaki")]
    public Vector3 posisiBawah = new Vector3(0, -1.5f, 0);

    [Header("Batas Layar (0.0 sampai 1.0)")]
    [Tooltip("Jika posisi Y pemain di layar lebih besar dari nilai ini, bubble akan pindah ke bawah.")]
    public float batasAtas = 0.7f; 

    private Camera kameraUtama;
    private Transform letakPlayer;

    void Awake()
    {
        kameraUtama = Camera.main;
        
        // Mengambil posisi parent utama (Karakter Samosir) tempat Canvas ini menempel
        letakPlayer = transform.root; 
    }

    void Update()
    {
        if (kameraUtama == null || letakPlayer == null) return;

        // Mengubah koordinat Player di game menjadi koordinat layar (Viewport)
        // Viewport: Y = 0 (paling bawah layar), Y = 1 (paling atas layar)
        Vector3 posisiDiLayar = kameraUtama.WorldToViewportPoint(letakPlayer.position);

        // Jika Samosir terlalu dekat dengan batas atas layar (misal berinteraksi dengan lukisan di atas)
        if (posisiDiLayar.y > batasAtas)
        {
            // Pindahkan bubble ke bawah kaki
            transform.localPosition = posisiBawah;
        }
        else
        {
            // Jika aman, biarkan bubble di atas kepala
            transform.localPosition = posisiAtas;
        }
    }
}