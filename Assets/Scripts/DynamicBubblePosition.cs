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

        // Posisikan bubble sesuai dengan Render Mode Canvas parent-nya
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                transform.position = kameraUtama.WorldToScreenPoint(targetWorldPos);
            }
            else if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Vector3 screenPos = kameraUtama.WorldToScreenPoint(targetWorldPos);
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
                transform.position = targetWorldPos;
            }
        }
        else
        {
            transform.position = kameraUtama.WorldToScreenPoint(targetWorldPos);
        }
    }
}