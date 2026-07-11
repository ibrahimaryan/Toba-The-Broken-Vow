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
                // Pindah scene biasa dalam chapter yang sama (atau load game pada chapter yang sama)
                // Pastikan daftar misi terisi dan statusnya dimuat dari PlayerPrefs
                if (ToDoManager.Instance.daftarMisi == null || ToDoManager.Instance.daftarMisi.Count == 0)
                {
                    ToDoManager.Instance.daftarMisi = new List<Quest>(misiDiSceneIni);
                }
                ToDoManager.Instance.LoadQuestStatus();
                return; // Pindah scene biasa dalam chapter yang sama, lagu jangan diganti
            }

            // Gunakan SetCurrentChapterID agar chapter tidak bisa mundur
            if (!ToDoManager.Instance.IsChapterAtLeast(chapterID))
            {
                // Chapter ini lebih rendah dari yang sedang aktif, tolak penggantian misi
                // tapi tetap perbarui UI agar misi yang benar tampil
                ToDoManager.Instance.UpdateTampilanUI();

                // KODE UNTUK MENGGANTI LAGU SECARA OTOMATIS
                if (BGMManager.Instance != null && laguChapterIni != null)
                {
                    BGMManager.Instance.GantiLagu(laguChapterIni, 1.5f);
                }
                return;
            }

            // JIKA MASUK CHAPTER BARU YANG LEBIH MAJU:
            ToDoManager.Instance.daftarMisi = new List<Quest>(misiDiSceneIni);
            ToDoManager.Instance.SetCurrentChapterID(chapterID); 
            ToDoManager.Instance.LoadQuestStatus();

            // KODE UNTUK MENGGANTI LAGU SECARA OTOMATIS
            if (BGMManager.Instance != null && laguChapterIni != null)
            {
                BGMManager.Instance.GantiLagu(laguChapterIni, 1.5f); // Angka 1.5f adalah durasi pudar (detik)
            }
        }
    }
}