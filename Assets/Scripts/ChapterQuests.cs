using System.Collections.Generic;
using UnityEngine;

public class ChapterQuests : MonoBehaviour
{
    [Header("Pengaturan Urutan Cerita")]
    [Tooltip("Beri nama ID yang sama untuk scene-scene yang berada di chapter yang sama (Contoh: Chapter_2)")]
    public string chapterID; 

    [Header("Pengaturan Audio")]
    [Tooltip("Masukkan lagu khusus untuk chapter ini. Biarkan kosong jika ingin melanjutkan lagu dari scene sebelumnya.")]
    public AudioClip laguChapterIni;
    [Header("Daftar Misi Khusus Chapter Ini")]
    public List<Quest> misiDiSceneIni;

    void Start()
    {
        Invoke("SetorMisi", 0.1f);
    }

    void SetorMisi()
    {
        if (ToDoManager.Instance != null)
        {
            if (ToDoManager.Instance.currentChapterID == chapterID)
            {
                return; // Pindah scene biasa dalam chapter yang sama, lagu jangan diganti
            }

            // JIKA MASUK CHAPTER BARU:
            ToDoManager.Instance.daftarMisi = new List<Quest>(misiDiSceneIni);
            ToDoManager.Instance.currentChapterID = chapterID; 
            ToDoManager.Instance.UpdateTampilanUI();

            // KODE UNTUK MENGGANTI LAGU SECARA OTOMATIS
            if (BGMManager.Instance != null && laguChapterIni != null)
            {
                BGMManager.Instance.GantiLagu(laguChapterIni, 1.5f); // Angka 1.5f adalah durasi pudar (detik)
            }
        }
    }
}