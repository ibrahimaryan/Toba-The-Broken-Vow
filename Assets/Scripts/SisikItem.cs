using UnityEngine;
using System.Collections;

public class SisikItem : MonoBehaviour
{
    [Header("Settings")]
    public string sisikID; // ID Unik untuk spawn point ini (misal: sisik_loc_0, sisik_loc_1, dll)
    [SerializeField] private float blinkSpeed = 1.5f; 
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.3f; 

    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;
    private bool isPlayerInRange = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Pengecekan status spawn dan koleksi dari GameManager
        if (GameManager.Instance != null)
        {
            // Jika proses pencarian sisik belum aktif, atau lokasi ini tidak terpilih dalam 7 lokasi acak
            if (!GameManager.Instance.IsFlagSet("sisik_spawning_active") || !GameManager.Instance.IsFlagSet("sisik_active_" + sisikID))
            {
                gameObject.SetActive(false);
                return;
            }

            // Jika sisik di lokasi ini sudah pernah diambil oleh player
            if (GameManager.Instance.IsFlagSet("sisik_collected_" + sisikID))
            {
                gameObject.SetActive(false);
                return;
            }
        }

        // Mulai efek berkedip
        if (spriteRenderer != null)
        {
            blinkCoroutine = StartCoroutine(BlinkEffect());
        }
    }

    private void OnEnable()
    {
        PlayerControllerScript.OnInteractPressed += HandleInteraction;
    }

    private void OnDisable()
    {
        PlayerControllerScript.OnInteractPressed -= HandleInteraction;
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
    }

    private void HandleInteraction()
    {
        if (isPlayerInRange)
        {
            Collect();
        }
    }

    private void Collect()
    {
        Debug.Log($"Sisik {sisikID} berhasil diambil!");

        // Masukkan ke inventory dinamis
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem("sisik", 1);
        }

        // Tandai di GameManager bahwa sisik ini sudah dikumpulkan
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag("sisik_collected_" + sisikID, true);
        }

        // Hancurkan/Nonaktifkan object ini agar hilang dari scene
        gameObject.SetActive(false);
    }

    private IEnumerator BlinkEffect()
    {
        while (spriteRenderer != null)
        {
            float lerpTime = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            float alpha = Mathf.Lerp(minAlpha, 1f, lerpTime);

            spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}
