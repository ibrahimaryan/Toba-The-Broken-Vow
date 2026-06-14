using UnityEngine;

public class SortingDinamis : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Mengubah sorting order secara dinamis berdasarkan posisi Y.
        // Dikalikan -100 agar perubahan nilai Y yang kecil tetap sensitif mengubah order.
        spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * -100);
    }
}