using UnityEngine;

public class EksplorasiLintasScene : MonoBehaviour
{
    [Header("Pengaturan Lokasi")]
    public string namaSceneIni; 
    public string[] targetSceneYangHarusDikunjungi;

    void Start()
    {
        // 1. Simpan ingatan secara instan saat Samosir masuk ruangan
        PlayerPrefs.SetInt("Visited_" + namaSceneIni, 1);
        PlayerPrefs.Save();

        // 2. BERI JEDA 0.2 DETIK! 
        // Biarkan ChapterQuests (yang punya jeda 0.1 detik) selesai menata UI terlebih dahulu.
        Invoke("CekProgressEksplorasi", 0.2f);
    }

    void CekProgressEksplorasi()
    {
        int totalDikunjungi = 0;

        foreach (string scene in targetSceneYangHarusDikunjungi)
        {
            if (PlayerPrefs.GetInt("Visited_" + scene, 0) == 1)
            {
                totalDikunjungi++;
            }
        }

        // Tampilkan hasil hitungan di Console Unity agar kita tahu apa yang dipikirkan gamemu
        Debug.Log($"[Detektif Scene] Samosir sudah mengunjungi: {totalDikunjungi} dari {targetSceneYangHarusDikunjungi.Length} ruangan.");

        if (totalDikunjungi >= targetSceneYangHarusDikunjungi.Length)
        {
            if (ToDoManager.Instance != null)
            {
                ToDoManager.Instance.SelesaikanMisi(0); // 0 adalah urutan misinya
                Debug.Log("Misi Eksplorasi Lintas Scene Selesai!");
            }
        }
    }
}