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
    private Transform originalParent;
    private Vector2 dragOffset;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        originalParent = transform.parent;
        startPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvas == null) canvas = GetComponentInParent<Canvas>();

        // Catat parent sebelum diseret
        parentAfterDrag = transform.parent;

        // Pindahkan ke Canvas root agar terbebas dari Layout Group (Grid/Vertical/Horizontal Layout Group)
        if (canvas != null)
        {
            transform.SetParent(canvas.transform);
        }
        transform.SetAsLastSibling(); // Menampilkan di paling depan

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false; // Mematikan raycast agar UI di belakang (Slot) terdeteksi

        // Hitung offset pointer agar item tidak melompat ketika mulai diseret
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out dragOffset
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            // Ambil posisi pointer relatif terhadap Canvas utama
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, 
                eventData.position, 
                canvas.worldCamera, 
                out Vector2 localPoint
            );
            
            // Set posisi rectTransform dikurangi offset awal
            rectTransform.anchoredPosition = localPoint - dragOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Pasang kembali ke parent target (bisa slot target atau originalParent jika dilepas di luar slot)
        transform.SetParent(parentAfterDrag);

        if (parentAfterDrag == originalParent)
        {
            rectTransform.anchoredPosition = startPosition;
        }
    }

    public void ResetToOriginalState()
    {
        parentAfterDrag = originalParent;
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = startPosition;
    }
}
