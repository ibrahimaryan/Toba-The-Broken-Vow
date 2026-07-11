using UnityEngine;

public class DynamicLayerTrigger : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Header("Sorting Order Settings")]
    [SerializeField] private int normalOrder = 0; // Order saat player di luar/di bawah pohon
    [SerializeField] private int highOrder = 5;   // Order saat player di belakang pohon (harus lebih besar dari Order Player)

    void Awake()
    {
        // Mengambil komponen Sprite Renderer dari objek ini
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = normalOrder;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            spriteRenderer.sortingOrder = highOrder;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            spriteRenderer.sortingOrder = normalOrder;
        }
    }
}
