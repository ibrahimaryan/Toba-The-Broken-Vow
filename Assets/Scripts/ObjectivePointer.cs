using UnityEngine;

public class ObjectivePointer : MonoBehaviour
{
    public static ObjectivePointer Instance { get; private set; }

    [Header("Target & Player Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform targetTransform;

    [Header("Visual Settings")]
    [SerializeField] private GameObject arrowVisual; // Child GameObject yang berisi SpriteRenderer panah
    [SerializeField] private float radius = 1.5f; // Jarak panah mengitari Player
    [SerializeField] private float hideDistance = 3.0f; // Sembunyikan panah jika player sudah dekat target

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Cari player otomatis jika belum di-assign
        if (playerTransform == null)
        {
            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        // Pemicu agar Story Manager memperbarui pointer setelah ObjectivePointer siap
        if (Chapter3StoryManager.Instance != null)
        {
            Chapter3StoryManager.Instance.UpdateObjectivePointer();
        }
        else if (Chapter4StoryManager.Instance != null)
        {
            Chapter4StoryManager.Instance.UpdateQuestStatus();
        }

        // Pastikan visual panah mati jika belum ada target
        if (targetTransform == null && arrowVisual != null)
        {
            arrowVisual.SetActive(false);
        }
    }

    private void Update()
    {
        // Pengaman ekstra untuk mendeteksi missing reference / destroyed player object
        bool isPlayerDestroyedOrNull = false;
        try
        {
            if (playerTransform == null || playerTransform.gameObject == null)
            {
                isPlayerDestroyedOrNull = true;
            }
        }
        catch (System.Exception)
        {
            isPlayerDestroyedOrNull = true;
        }

        if (isPlayerDestroyedOrNull)
        {
            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("[ObjectivePointer] Player berhasil ditemukan kembali secara dinamis.");
            }
            else
            {
                if (arrowVisual != null && arrowVisual.activeSelf)
                {
                    arrowVisual.SetActive(false);
                }
                return;
            }
        }

        // Pengaman ekstra untuk targetTransform (karena target bisa hancur/hilang)
        bool isTargetDestroyedOrNull = false;
        try
        {
            if (targetTransform == null || targetTransform.gameObject == null)
            {
                isTargetDestroyedOrNull = true;
            }
        }
        catch (System.Exception)
        {
            isTargetDestroyedOrNull = true;
        }

        if (isTargetDestroyedOrNull || arrowVisual == null)
        {
            if (arrowVisual != null && arrowVisual.activeSelf)
            {
                arrowVisual.SetActive(false);
            }
            return;
        }

        // Hitung arah dan jarak ke target
        Vector3 targetDirection = targetTransform.position - playerTransform.position;
        float distance = targetDirection.magnitude;

        // Jika player sudah sangat dekat dengan target, sembunyikan panah
        if (distance < hideDistance)
        {
            if (arrowVisual.activeSelf)
            {
                arrowVisual.SetActive(false);
            }
            return;
        }

        // Aktifkan visual panah jika tersembunyi
        if (!arrowVisual.activeSelf)
        {
            arrowVisual.SetActive(true);
        }

        // 1. Tempatkan panah melingkar di sekitar player
        Vector3 dirNormalized = targetDirection.normalized;
        transform.position = playerTransform.position + (dirNormalized * radius);

        // 2. Putar panah agar menghadap ke target
        float angle = Mathf.Atan2(dirNormalized.y, dirNormalized.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    // Mengubah target secara dinamis
    public void SetTarget(Transform newTarget)
    {
        targetTransform = newTarget;
        Debug.Log($"[ObjectivePointer] SetTarget dipanggil. Target baru: {(newTarget != null ? newTarget.name : "NULL")}");

        if (newTarget == null && arrowVisual != null)
        {
            arrowVisual.SetActive(false);
        }
        else if (newTarget != null && arrowVisual != null)
        {
            arrowVisual.SetActive(true);
        }
    }

    // Membersihkan target (menghilangkan panah)
    public void ClearTarget()
    {
        targetTransform = null;
        if (arrowVisual != null)
        {
            arrowVisual.SetActive(false);
        }
    }
}
