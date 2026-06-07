using UnityEngine;
using UnityEngine.EventSystems;

public class PuzzleSlot : MonoBehaviour, IDropHandler
{
    [Tooltip("ID item yang benar untuk slot ini (misal: sisik_part_0, sisik_part_1, dll.)")]
    public string correctItemID;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        DraggableItem draggable = dropped.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            // Jika slot ini masih kosong, pindahkan item ke dalam slot
            if (transform.childCount == 0)
            {
                draggable.parentAfterDrag = transform;
                dropped.transform.SetParent(transform);
                
                // Posisikan tepat di tengah slot
                RectTransform droppedRect = dropped.GetComponent<RectTransform>();
                if (droppedRect != null)
                {
                    droppedRect.anchoredPosition = Vector2.zero;
                }
                
                Debug.Log($"Item '{draggable.itemID}' diletakkan di Slot dengan target '{correctItemID}'");
            }
        }
    }
}
