using UnityEngine;
using UnityEngine.Playables;

public class TimelineDialogTrigger : MonoBehaviour
{
    [Header("Daftar Obrolan (Cutscene)")]
    [Tooltip("Isi daftar berbagai dialog. Element 0=Awal, Element 1=Cabang B, dsbgnya.")]
    public Dialogue[] listDialogs; 

    // --- FITUR TERBAIK UNTUK SIGNAL (Tanpa Parameter / Hardcoded per Indeks) ---

    // 1. Panggil ini untuk memaksa masuk ke Element 0
    public void PutarDialogNomor0()
    {
        EksekusiDialogAman(0);
    }

    // 2. Panggil ini untuk memaksa masuk ke Element 1
    public void PutarDialogNomor1()
    {
        EksekusiDialogAman(1);
    }

    // 3. Panggil ini untuk memaksa masuk ke Element 2 (Jika ada)
    public void PutarDialogNomor2()
    {
        EksekusiDialogAman(2);
    }
    
    // (Bisa tambahkan public void PutarDialogNomor3() seterusnya jika lebih banyak obrolan)

    // --------------------------------------------------------------------------

    // Fungsi rahasia internal saja agar penulisan ringkas
    private void EksekusiDialogAman(int index)
    {
        if (DialogueManager.instance == null) return;

        if (index >= 0 && index < listDialogs.Length)
        {
            DialogueManager.instance.StartDialogue(listDialogs[index]);
        }
        else
        {
            Debug.LogWarning("TIDAK ADA DIALOG! Dialog Element " + index + " belum dibuat di Inspector Cutscene_Intro Anda!");
        }
    }

    /// <summary>
    /// Dipanggil oleh Signal Emitter. Kita menangkap parameter tipe String dari Timeline Emitter itu sendiri.
    /// Anda cukup mengetikkan angkanya (misal "0", "1", "2") langsung di kolom String Inspector Emitter.
    /// </summary>
    public void PutarDialogDariString(string textIndex)
    {
        if (DialogueManager.instance == null) return;

        // Ubah teks "0" atau "1" yang dikirim Timeline menjadi Integer beneran
        if (int.TryParse(textIndex, out int parsedIndex))
        {
            if (parsedIndex >= 0 && parsedIndex < listDialogs.Length)
            {
                DialogueManager.instance.StartDialogue(listDialogs[parsedIndex]);
            }
            else
            {
                Debug.LogWarning($"Dialog Index {parsedIndex} tidak ditemukan di array listDialogs!");
            }
        }
        else
        {
            Debug.LogError($"Signal salah format! Harap masukkan Angka saja, bukan teks: '{textIndex}'");
        }
    }
}