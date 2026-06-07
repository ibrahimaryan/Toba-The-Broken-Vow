using UnityEngine;

public class LoadPlayerPosition : MonoBehaviour
{
    // Ini adalah "KTP" Samosir Asli
    public static LoadPlayerPosition instance;

    void Awake()
    {
        // Pengecekan Duplikat (Singleton)
        if (instance == null)
        {
            // Jika belum ada Samosir di dunia ini, tetapkan Samosir ini sebagai yang Asli
            instance = this;
            
            // Buat Samosir tidak hancur saat pindah map
            DontDestroyOnLoad(gameObject); 
        }
        else if (instance != this)
        {
            // Jika sudah ada Samosir Asli yang terbawa dari map sebelumnya, 
            // HANCURKAN Samosir kloningan yang ada di map baru ini seketika!
            Destroy(gameObject);
            return; // Hentikan script agar tidak error
        }
    }

    void Start()
    {
        // Hanya Samosir yang Asli yang boleh menjalankan perintah muat posisi ini
        if (PlayerPrefs.HasKey("SavedPosX") && PlayerPrefs.HasKey("SavedPosY"))
        {
            float posisiX = PlayerPrefs.GetFloat("SavedPosX");
            float posisiY = PlayerPrefs.GetFloat("SavedPosY");

            transform.position = new Vector3(posisiX, posisiY, transform.position.z);
        }
    }
}