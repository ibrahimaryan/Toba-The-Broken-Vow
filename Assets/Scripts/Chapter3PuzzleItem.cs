using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class Chapter3PuzzleItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Item Settings")]
    public string itemID; // ID unik kepingan puzzle (misal: piece_0, piece_1, piece_2)
    public bool constrainToYAxis = true; // Batasi pergerakan hanya vertikal (ke atas / bawah)

    [HideInInspector] public Transform parentAfterDrag; // Slot penampung setelah dilepas
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Vector2 startPosition;

    [Header("Rotation Settings")]
    [Tooltip("Sudut rotasi saat ini dalam derajat")]
    public float targetRotationAngle = 0f;
    [SerializeField] private float rotationSpeed = 10f; // Kecepatan animasi rotasi

    [Header("Audio Settings")]
    [SerializeField] private AudioClip rotateSound;
    private AudioSource audioSource;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector2 dragOffset;
    private float currentRotation = 0f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        originalParent = transform.parent;
        startPosition = rectTransform.anchoredPosition;
        currentRotation = targetRotationAngle;
        rectTransform.localRotation = Quaternion.Euler(0, 0, currentRotation);
    }

    private void Update()
    {
        // Animasi rotasi halus menuju target angle
        if (Mathf.Abs(currentRotation - targetRotationAngle) > 0.01f)
        {
            currentRotation = Mathf.Lerp(currentRotation, targetRotationAngle, Time.deltaTime * rotationSpeed);
            rectTransform.localRotation = Quaternion.Euler(0, 0, currentRotation);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvas == null) canvas = GetComponentInParent<Canvas>();

        parentAfterDrag = transform.parent;

        if (canvas != null)
        {
            transform.SetParent(canvas.transform);
        }
        transform.SetAsLastSibling(); // Paling depan

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false; // Matikan raycast agar slot terdeteksi

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
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, 
                eventData.position, 
                canvas.worldCamera, 
                out Vector2 localPoint
            );

            if (constrainToYAxis)
            {
                // Hanya ubah posisi Y, X tetap konstan dari posisi start sebelum drag
                float targetY = localPoint.y - dragOffset.y;
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, targetY);
            }
            else
            {
                rectTransform.anchoredPosition = localPoint - dragOffset;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        transform.SetParent(parentAfterDrag);

        if (parentAfterDrag == originalParent)
        {
            rectTransform.anchoredPosition = startPosition;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Jangan putar jika user sedang menyeret (dragging)
        if (eventData.dragging) return;

        // Putar suara rotasi jika ada
        if (audioSource != null && rotateSound != null)
        {
            audioSource.PlayOneShot(rotateSound);
        }

        // Klik kiri/kanan memutar 90 derajat searah jarum jam (atau berlawanan jika mau)
        targetRotationAngle -= 90f;
        
        // Jaga agar nilai target tetap dalam rentang 360 derajat secara bersih untuk mempermudah perbandingan
        if (targetRotationAngle <= -360f)
        {
            targetRotationAngle += 360f;
            currentRotation += 360f; // Menghindari perputaran balik penuh secara visual
        }

        Debug.Log($"Item '{itemID}' diklik. Target rotasi sekarang: {targetRotationAngle} derajat");
    }

    public void ResetToOriginalState()
    {
        parentAfterDrag = originalParent;
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = startPosition;
        targetRotationAngle = 0f;
        currentRotation = 0f;
        rectTransform.localRotation = Quaternion.identity;
    }

    // Mengecek apakah rotasi sudah lurus (0 derajat atau kelipatan 360)
    public bool IsRotationCorrect()
    {
        float normalizedAngle = Mathf.Abs(targetRotationAngle) % 360f;
        return normalizedAngle < 0.1f || normalizedAngle > 359.9f;
    }
}
