using UnityEngine;
using UnityEditor;

public class ResetDataMenu : MonoBehaviour
{
    // Ini akan memunculkan menu baru di bilah atas (Toolbar) Unity
    [MenuItem("Tools/Reset Semua Data Game (PlayerPrefs)")]
    public static void ResetPlayerPrefs()
    {
        // Baris ampuh untuk menghapus seluruh data/memori PlayerPrefs yang tersimpan.
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save(); // Terapkan penghapusan segera

        // Tampilkan pesan konfirmasi (bisa dilihat di Console atau Pop-up)
        Debug.Log("<b>SUCCESS:</b> Semua data PlayerPrefs (termasuk Cutscene ID dan skor) berhasil di-reset menjadi 0!");
        
        // (Opsional) Munculkan dialog peringatan di tengah editor
        EditorUtility.DisplayDialog("Reset Berhasil", "Seluruh data penyimpanan PlayerPrefs berhasil dihapus. Semua Cutscene akan tayang ulang dari awal.", "OK");
    }
}