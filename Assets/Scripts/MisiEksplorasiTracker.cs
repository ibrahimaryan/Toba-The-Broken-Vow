using UnityEngine;

public class MisiEksplorasiTracker : MonoBehaviour
{
    [Header("Pengaturan Misi")]
    [Tooltip("Berapa banyak objek yang harus diperiksa Samosir agar misi selesai?")]
    public int totalObjekDiRuangan = 4; 
    
    private int objekSelesaiDicek = 0;

    // Fungsi ini akan dipanggil setiap kali Samosir selesai membaca 1 objek
    public void TambahProgressEksplorasi()
    {
        objekSelesaiDicek++;
        Debug.Log("Progres Eksplorasi: " + objekSelesaiDicek + "/" + totalObjekDiRuangan);

        // Jika jumlah objek yang dicek sudah mencapai target, coret misinya!
        if (objekSelesaiDicek >= totalObjekDiRuangan)
        {
            if (ToDoManager.Instance != null)
            {
                // Angka 0 = Misi urutan PERTAMA di layar (Eksplorasi ruangan)
                ToDoManager.Instance.SelesaikanMisi(0); 
            }
        }
    }
}