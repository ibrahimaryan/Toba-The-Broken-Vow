using UnityEngine;
using System.Collections; // Wajib ditambahkan untuk Coroutine

public class SceneLoadManager : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnPointSetup
    {
        public string doorID;          
        public Transform spawnTransform; 
    }

    [Header("Player Reference")]
    // Kosongkan saja di Inspector, biar script mencari otomatis
    [SerializeField] private GameObject playerObject; 

    [Header("Spawn Points List")]
    [SerializeField] private SpawnPointSetup[] spawnPoints;

   // 1. Ubah 'void' menjadi 'IEnumerator'
    private IEnumerator Start()
    {
        // 2. Tambahkan jeda 1 frame agar proses Singleton Samosir & GameManager selesai dulu
        yield return new WaitForEndOfFrame(); 

        // Cari player secara otomatis jika slot di inspector kosong
        if (playerObject == null)
        {
            if (PlayerControllerScript.Instance != null)
            {
                playerObject = PlayerControllerScript.Instance.gameObject;
            }
            else
            {
                playerObject = GameObject.FindGameObjectWithTag("Player");
            }
        }

        if (GameManager.Instance == null || playerObject == null)
        {
            Debug.LogError("GameManager atau Player tidak ditemukan!");
            yield break; // 3. Ubah 'return' menjadi 'yield break'
        }

        string lastDoor = GameManager.Instance.lastExitDoorID;
        Debug.Log("Mencari titik spawn untuk Pintu: " + lastDoor);

        foreach (SpawnPointSetup point in spawnPoints)
        {
            if (point.doorID == lastDoor)
            {
                Debug.Log("MATCH! Memaksa teleportasi Player...");

                // Matikan komponen pergerakan/fisika sementara (agar tidak melawan)
                Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.simulated = false; // Matikan fisika sementara
                    
                    // Pindahkan posisi Transform (Amankan Z agar tidak tertutup background atau keluar kamera)
                    Vector3 targetPosition = new Vector3(point.spawnTransform.position.x, point.spawnTransform.position.y, playerObject.transform.position.z);
                    playerObject.transform.position = targetPosition;
                    
                    // Pindahkan posisi Rigidbody
                    rb.position = new Vector2(targetPosition.x, targetPosition.y);
                    rb.linearVelocity = Vector2.zero;

                    // Sinkronisasi paksa
                    Physics2D.SyncTransforms();

                    // Nyalakan lagi fisikanya
                    rb.simulated = true;
                }
                else
                {
                    // Jika tidak pakai Rigidbody, langsung pindah transform (Amankan Z)
                    playerObject.transform.position = new Vector3(point.spawnTransform.position.x, point.spawnTransform.position.y, playerObject.transform.position.z);
                }

                Debug.Log("Player berhasil disitir ke: " + playerObject.transform.position);
                break; // Hentikan pencarian jika sudah ketemu
            }
        }
    }
}