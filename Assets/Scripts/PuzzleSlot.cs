using UnityEngine;
using UnityEngine.EventSystems;

public class PuzzleSlot : MonoBehaviour, IDropHandler
{
    [Tooltip("ID item yang benar untuk slot ini (misal: sisik_part_0, sisik_part_1, dll.)")]
    public string correctItemID;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip dropSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void PlayDropSound()
    {
        if (audioSource != null && dropSound != null)
        {
            audioSource.PlayOneShot(dropSound);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        DraggableItem draggable = dropped.GetComponent<DraggableItem>();
        Chapter3PuzzleItem c3Draggable = dropped.GetComponent<Chapter3PuzzleItem>();

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
                
                PlayDropSound();
                Debug.Log($"Item '{draggable.itemID}' diletakkan di Slot dengan target '{correctItemID}'");
            }
        }
        else if (c3Draggable != null)
        {
            // Jika slot ini masih kosong, pindahkan item ke dalam slot
            if (transform.childCount == 0)
            {
                c3Draggable.parentAfterDrag = transform;
                dropped.transform.SetParent(transform);
                
                // Posisikan tepat di tengah slot
                RectTransform droppedRect = dropped.GetComponent<RectTransform>();
                if (droppedRect != null)
                {
                    droppedRect.anchoredPosition = Vector2.zero;
                }
                
                PlayDropSound();
                Debug.Log($"Chapter 3 Item '{c3Draggable.itemID}' diletakkan di Slot dengan target '{correctItemID}'");
            }
            else
            {
                // Jika slot terisi, lakukan SWAP (tukar posisi) dengan item yang ada di slot ini
                Chapter3PuzzleItem occupyingItem = GetComponentInChildren<Chapter3PuzzleItem>();
                if (occupyingItem != null && occupyingItem != c3Draggable)
                {
                    Transform sourceSlot = c3Draggable.parentAfterDrag; // Slot asal item yang sedang didrag
                    
                    // Pindahkan item yang menempati slot ini ke slot asal
                    occupyingItem.transform.SetParent(sourceSlot);
                    occupyingItem.parentAfterDrag = sourceSlot;
                    occupyingItem.originalParent = sourceSlot;
                    RectTransform occRect = occupyingItem.GetComponent<RectTransform>();
                    if (occRect != null)
                    {
                        occRect.anchoredPosition = Vector2.zero;
                    }
                    
                    // Pindahkan item yang sedang didrag ke slot ini
                    c3Draggable.parentAfterDrag = transform;
                    dropped.transform.SetParent(transform);
                    RectTransform droppedRect = dropped.GetComponent<RectTransform>();
                    if (droppedRect != null)
                    {
                        droppedRect.anchoredPosition = Vector2.zero;
                    }
                    
                    PlayDropSound();
                    Debug.Log($"Swap Chapter 3: Tukar '{c3Draggable.itemID}' ke slot '{correctItemID}' dan '{occupyingItem.itemID}' ke slot asal.");
                }
            }
        }
    }
}
