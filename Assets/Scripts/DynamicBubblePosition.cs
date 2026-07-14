using UnityEngine;

public class DynamicBubblePosition : MonoBehaviour
{
    [Header("Pengaturan Posisi Lokal")]
    [Tooltip("Posisi Y saat bubble di atas kepala")]
    public Vector3 posisiAtas = new Vector3(0, 1.5f, 0); 
    
    [Tooltip("Posisi Y saat bubble di bawah kaki")]
    public Vector3 posisiBawah = new Vector3(0, -1.5f, 0);

    [Header("Batas Layar (0.0 sampai 1.0)")]
    [Tooltip("Jika posisi Y pemain di layar lebih besar dari nilai ini, bubble akan pindah ke bawah.")]
    public float batasAtas = 0.7f; 

    private Camera kameraUtama;
    private Transform letakPlayer;

    // Target karakter saat ini (bisa diset secara dinamis oleh DialogueManager)
    [HideInInspector]
    public Transform targetCharacter;

    void Awake()
    {
        kameraUtama = Camera.main;
        letakPlayer = transform.root; 
    }

    void LateUpdate()
    {
        if (kameraUtama == null) kameraUtama = Camera.main;
        if (kameraUtama == null) return;

        // Tentukan target yang akan diikuti
        Transform currentTarget = targetCharacter != null ? targetCharacter : letakPlayer;
        if (currentTarget == null) return;

        // Cek posisi target di layar untuk menentukan di atas kepala atau di bawah kaki
        Vector3 posisiDiLayar = kameraUtama.WorldToViewportPoint(currentTarget.position);
        Vector3 offset = (posisiDiLayar.y > batasAtas) ? posisiBawah : posisiAtas;

        // Hitung posisi target di dunia dengan offset Y
        Vector3 targetWorldPos = currentTarget.position + new Vector3(0, offset.y, 0);

        // Ambil padding dari ukuran RectTransform agar gelembung dialog tidak terpotong layar
        RectTransform rectTransform = transform as RectTransform;
        float paddingX = 150f; // Nilai fallback horizontal (setengah lebar bubble)
        float paddingY = 80f;  // Nilai fallback vertikal (setengah tinggi bubble)
        if (rectTransform != null)
        {
            Vector2 sizeInPixels = GetSizeInScreenPixels(rectTransform);
            paddingX = sizeInPixels.x * 0.5f;
            paddingY = sizeInPixels.y * 0.5f;
        }

        if (paddingX <= 0) paddingX = 150f;
        if (paddingY <= 0) paddingY = 80f;

        // Posisikan bubble sesuai dengan Render Mode Canvas parent-nya
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Vector3 screenPos = kameraUtama.WorldToScreenPoint(targetWorldPos);
                screenPos.x = Mathf.Clamp(screenPos.x, paddingX, Screen.width - paddingX);
                screenPos.y = Mathf.Clamp(screenPos.y, paddingY, Screen.height - paddingY);
                transform.position = screenPos;
            }
            else if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Vector3 screenPos = kameraUtama.WorldToScreenPoint(targetWorldPos);
                screenPos.x = Mathf.Clamp(screenPos.x, paddingX, Screen.width - paddingX);
                screenPos.y = Mathf.Clamp(screenPos.y, paddingY, Screen.height - paddingY);

                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    transform.parent as RectTransform,
                    screenPos,
                    parentCanvas.worldCamera != null ? parentCanvas.worldCamera : kameraUtama,
                    out Vector3 worldPos))
                {
                    transform.position = worldPos;
                }
            }
            else // WorldSpace
            {
                // Hitung posisi di layar, lakukan pembatasan (clamping), lalu kembalikan ke posisi dunia (World Space)
                Vector3 screenPos = kameraUtama.WorldToScreenPoint(targetWorldPos);
                screenPos.x = Mathf.Clamp(screenPos.x, paddingX, Screen.width - paddingX);
                screenPos.y = Mathf.Clamp(screenPos.y, paddingY, Screen.height - paddingY);

                Vector3 worldPos = kameraUtama.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, screenPos.z));
                transform.position = worldPos;
            }
        }
        else
        {
            Vector3 screenPos = kameraUtama.WorldToScreenPoint(targetWorldPos);
            screenPos.x = Mathf.Clamp(screenPos.x, paddingX, Screen.width - paddingX);
            screenPos.y = Mathf.Clamp(screenPos.y, paddingY, Screen.height - paddingY);
            transform.position = screenPos;
        }
    }

    private Vector2 GetSizeInScreenPixels(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        
        Vector2 screenCorner0, screenCorner1, screenCorner3;
        
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // For overlay canvas, world corners are already in screen space
            screenCorner0 = corners[0];
            screenCorner1 = corners[1];
            screenCorner3 = corners[3];
        }
        else
        {
            // For ScreenSpaceCamera or WorldSpace, convert world coordinates to screen pixels
            screenCorner0 = kameraUtama.WorldToScreenPoint(corners[0]);
            screenCorner1 = kameraUtama.WorldToScreenPoint(corners[1]);
            screenCorner3 = kameraUtama.WorldToScreenPoint(corners[3]);
        }
        
        float width = Vector2.Distance(screenCorner0, screenCorner3);
        float height = Vector2.Distance(screenCorner0, screenCorner1);
        
        return new Vector2(width, height);
    }
}