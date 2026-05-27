using UnityEngine;

public class SceneLoadManager : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnPointSetup
    {
        public string doorID;          // Harus sama dengan doorID di DoorController scene sebelumnya
        public Transform spawnTransform; // Objek kosong penanda posisi berdiri player
    }

    [Header("Player Reference")]
    [SerializeField] private GameObject playerObject; // Tarik prefab/objek Player kamu ke sini

    [Header("Spawn Points List")]
    [SerializeField] private SpawnPointSetup[] spawnPoints;

    private void Start()
    {
        Debug.Log("SceneLoadManager Start");

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager tidak ada");
            return;
        }

        if (playerObject == null)
        {
            Debug.LogError("Player Object kosong");
            return;
        }

        Debug.Log("Last Door ID = " + GameManager.Instance.lastExitDoorID);

        foreach (SpawnPointSetup point in spawnPoints)
        {
            Debug.Log("Cek SpawnPoint = " + point.doorID);

            if (point.doorID == GameManager.Instance.lastExitDoorID)
            {
                Debug.Log("MATCH!");
                Debug.Log("SpawnPoint Pos = " + point.spawnTransform.position);
                Debug.Log("Player Sebelum = " + playerObject.transform.position);
                Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.position = point.spawnTransform.position;
                    rb.linearVelocity = Vector2.zero;
                }
                Debug.Log("Player Sesudah = " + playerObject.transform.position);
            }
        }
    }
}