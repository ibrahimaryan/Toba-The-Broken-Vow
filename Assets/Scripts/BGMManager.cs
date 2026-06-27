using System.Collections;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    private AudioSource audioSource;
    private float targetVolume; // Menyimpan volume asli awal

    void Awake()
    {
        // Sistem Singleton Abadi (DontDestroyOnLoad)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                targetVolume = audioSource.volume; // Catat volume awal dari Inspector
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Fungsi utama untuk mengganti lagu dengan efek pudar (Fade)
    public void GantiLagu(AudioClip laguBaru, float durasiFade = 1.5f)
    {
        // Jika lagu yang mau diputar sama dengan yang sedang berjalan, abaikan
        if (audioSource.clip == laguBaru) return;

        // Mulai proses transisi di balik layar (Coroutine)
        StartCoroutine(ProsesFadeLagu(laguBaru, durasiFade));
    }

    private IEnumerator ProsesFadeLagu(AudioClip laguBaru, float durasi)
    {
        float waktu = 0;
        float volumeAwal = audioSource.volume;

        // 1. FADE OUT: Mengecilkan volume lagu lama sampai 0
        while (waktu < durasi)
        {
            waktu += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(volumeAwal, 0, waktu / durasi);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = laguBaru;

        // Jika lagu barunya tidak kosong, putar dan besarkan volumenya
        if (laguBaru != null)
        {
            audioSource.Play();
            waktu = 0;

            // 2. FADE IN: Membesarkan volume lagu baru dari 0 ke volume asli
            while (waktu < durasi)
            {
                waktu += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(0, targetVolume, waktu / durasi);
                yield return null;
            }
            
            audioSource.volume = targetVolume; // Pastikan pas di volume target
        }
    }
}