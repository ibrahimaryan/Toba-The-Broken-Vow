using UnityEngine;
using System.Collections;

public class SisikItem : MonoBehaviour
{
    [Header("Settings")]
    public string sisikID; // ID Unik untuk spawn point ini (misal: sisik_loc_0, sisik_loc_1, dll)
    [SerializeField] private float blinkSpeed = 1.5f; 
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.3f; 
    [SerializeField] private Sprite[] possibleSprites; // Daftar 7 alternatif sprite sisik

    [Header("Audio Settings")]
    [SerializeField] private AudioClip collectsound; 

    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;
    private bool isPlayerInRange = false;
    private AudioSource audioSource;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        Debug.Log($"[SisikItem] Objek {gameObject.name} terdeteksi dengan sisikID: '{sisikID}'");

        // Acak sprite secara konsisten berdasarkan sisikID agar tidak berubah saat reload scene
        if (possibleSprites != null && possibleSprites.Length > 0)
        {
            Random.InitState(sisikID.GetHashCode());
            int randomIndex = Random.Range(0, possibleSprites.Length);
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = possibleSprites[randomIndex];
            }
            Random.InitState((int)System.DateTime.Now.Ticks); // Kembalikan seed random default
        }

        SetVisibility(false);
    }

    private void Update()
    {
        if (GameManager.Instance != null)
        {
            bool shouldBeVisible = GameManager.Instance.IsFlagSet("sisik_spawning_active") 
                                   && GameManager.Instance.IsFlagSet("sisik_active_" + sisikID)
                                   && !GameManager.Instance.IsFlagSet("sisik_collected_" + sisikID);

            if (shouldBeVisible)
            {
                if (spriteRenderer != null && !spriteRenderer.enabled)
                {
                    Debug.Log($"[SisikItem] Sisik dengan ID '{sisikID}' AKTIF dan MUNCUL!");
                    SetVisibility(true);
                }
            }
            else
            {
                if (spriteRenderer != null && spriteRenderer.enabled)
                {
                    SetVisibility(false);
                }
            }
        }
    }

    private void SetVisibility(bool visible)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = visible;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = visible;

        if (visible)
        {
            if (blinkCoroutine == null && spriteRenderer != null)
            {
                blinkCoroutine = StartCoroutine(BlinkEffect());
            }
        }
        else
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
            isPlayerInRange = false;
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

        if (collectsound != null)
        {
            AudioSource.PlayClipAtPoint(collectsound, Camera.main != null ? Camera.main.transform.position : transform.position);
        }

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
