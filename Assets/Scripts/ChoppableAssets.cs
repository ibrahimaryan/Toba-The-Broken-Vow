using UnityEngine;

public class ChoppableAssets : MonoBehaviour
{
    [Header("Settings")]
    public int health = 3; // Berapa kali pukul sampai hancur
    public GameObject dropPrefab; // Prefab bongkahan kayu / batu item

    // Fungsi ini nanti dipanggil oleh script Kapak/Pemain kamu
    public void GetChopped(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            SpawnDropItem();
            Destroy(gameObject); // Pohon hancur & hilang dari hierarchy
        }
    }

    void SpawnDropItem()
    {
        if (dropPrefab != null)
        {
            // Memunculkan bongkahan kayu di posisi pohon
            Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }
    }
}