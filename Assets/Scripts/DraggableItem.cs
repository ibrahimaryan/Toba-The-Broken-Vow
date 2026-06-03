using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string itemID; // ID unik untuk mengidentifikasi kepingan sisik ini (misal: sisik_part_0)
    [HideInInspector] public Transform parentAfterDrag; // Slot yang akan menampung item ini setelah dilepas
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector3 startPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        startPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Catat parent lama dan keluarkan dari layout agar bisa digeser bebas di atas elemen UI lain
        parentAfterDrag = transform.parent;
        transform.SetAsLastSibling(); // Menampilkan item di depan elemen UI lainnya

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false; // Mematikan raycast agar UI di belakangnya (Slot) bisa mendeteksi item
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) canvas = GetComponentInParent<Canvas>();

        // Menggeser posisi RectTransform berdasarkan delta pergeseran pointer mouse
        if (canvas != null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true; // Mengembalikan raycast agar item bisa diseret lagi nantinya

        // Jika setelah dilepas tidak ada slot yang menampung (parentAfterDrag tidak berubah)
        if (transform.parent == parentAfterDrag)
        {
            // Kembalikan ke posisi awal sebelum diseret
            rectTransform.anchoredPosition = startPosition;
        }
    }

    public void ResetToStart()
    {
        rectTransform.anchoredPosition = startPosition;
    }
}
