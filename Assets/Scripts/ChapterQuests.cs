using System.Collections.Generic;
using UnityEngine;

public class ChapterQuests : MonoBehaviour
{
    [Header("Pengaturan Urutan Cerita")]
    [Tooltip("Beri nama ID yang sama untuk scene-scene yang berada di chapter yang sama (Contoh: Chapter_2)")]
    public string chapterID; 

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
            // JIKA CHAPTER ID NYA SAMA, MAKA JANGAN RESET MISINYA!
            // Ini artinya Player hanya berpindah ruangan/scene di dalam chapter yang sama.
            if (ToDoManager.Instance.currentChapterID == chapterID)
            {
                Debug.Log("Pindah scene dalam Chapter yang sama. Mempertahankan progres misi.");
                return; // Langsung keluar, tidak mereset data lama
            }

            // JIKA CHAPTER ID NYA BERBEDA, BARU TIMPA DENGAN MISI BARU (Pindah Chapter Utama)
            ToDoManager.Instance.daftarMisi = new List<Quest>(misiDiSceneIni);
            ToDoManager.Instance.currentChapterID = chapterID; // Update ID chapter aktif di ToDoManager
            
            ToDoManager.Instance.UpdateTampilanUI();
            Debug.Log("Chapter Baru Terdeteksi! Memuat daftar misi untuk: " + chapterID);
        }
    }
}