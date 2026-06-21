using UnityEngine;
using System.Collections;

public class ChoppableAssets : MonoBehaviour
{
    public enum AssetType { Tree, Rock }

    [Header("Asset Settings")]
    [SerializeField] private AssetType type = AssetType.Tree;
    [SerializeField] private int health = 3;
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private string hitAnimationTrigger = "hit"; // Parameter trigger di Animator Player

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip destroySound;

    private bool isPlayerInRange = false;
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

    private void OnEnable()
    {
        PlayerControllerScript.OnInteractPressed += HandleInteraction;
    }

    private void OnDisable()
    {
        PlayerControllerScript.OnInteractPressed -= HandleInteraction;
        if (isPlayerInRange && InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt();
        }
    }

    private void Update()
    {
        if (!IsAxeCollected()) return;

        if (isPlayerInRange)
        {
            UpdateInteractionPrompt();
        }
    }

    private bool IsAxeCollected()
    {
        return GameManager.Instance != null && GameManager.Instance.IsFlagSet("chapter3_axe_collected");
    }

    private void HandleInteraction()
    {
        if (!IsAxeCollected()) return;
        if (!isPlayerInRange) return;

        bool hasCorrectTool = false;
        if (InventoryManager.Instance != null)
        {
            string activeTool = InventoryManager.Instance.currentEquippedItem;
            if (type == AssetType.Tree && activeTool == "kapak")
            {
                hasCorrectTool = true;
            }
            else if (type == AssetType.Rock && activeTool == "equipment2") // equipment2 diisi pemecah_batu
            {
                hasCorrectTool = true;
            }
        }

        if (hasCorrectTool)
        {
            // Tentukan arah hadap ke objek
            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null)
            {
                // Jika objek berada di sebelah kiri player, balik player ke kiri
                bool itemIsOnLeft = transform.position.x < player.transform.position.x;
                player.LockFacingDirection(itemIsOnLeft, 0.6f); // Kunci arah hadap selama 0.4 detik (durasi animasi memukul)

                Animator animator = player.GetComponent<Animator>();
                if (animator != null && !string.IsNullOrEmpty(hitAnimationTrigger))
                {
                    animator.SetTrigger(hitAnimationTrigger);
                }
            }

            GetChopped(1);
        }
        else
        {
            Debug.Log($"Tidak bisa menghancurkan. Butuh alat yang cocok untuk: {type}");
            // Memainkan suara gagal / feedback visual jika ada
        }
    }

    public void GetChopped(int damage)
    {
        health -= damage;

        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        if (health <= 0)
        {
            if (destroySound != null)
            {
                AudioSource.PlayClipAtPoint(destroySound, Camera.main != null ? Camera.main.transform.position : transform.position);
            }
            SpawnDropItem();
            
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.HidePrompt();
            }

            Destroy(gameObject);
        }
    }

    private void SpawnDropItem()
    {
        if (dropPrefab != null)
        {
            Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }
    }

    private void UpdateInteractionPrompt()
    {
        if (InteractionPromptUI.Instance == null) return;

        bool hasCorrectTool = false;
        if (InventoryManager.Instance != null)
        {
            string activeTool = InventoryManager.Instance.currentEquippedItem;
            if (type == AssetType.Tree && activeTool == "kapak")
            {
                hasCorrectTool = true;
            }
            else if (type == AssetType.Rock && activeTool == "equipment2")
            {
                hasCorrectTool = true;
            }
        }

        if (type == AssetType.Tree)
        {
            if (hasCorrectTool)
                InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk menebang Pohon");
            else
                InteractionPromptUI.Instance.ShowPrompt("Butuh Kapak Aktif (Tekan Q untuk ganti equipment)");
        }
        else if (type == AssetType.Rock)
        {
            if (hasCorrectTool)
                InteractionPromptUI.Instance.ShowPrompt("Tekan E untuk memecah Batu");
            else
                InteractionPromptUI.Instance.ShowPrompt("Butuh Pemecah Batu Aktif (Tekan Q untuk ganti equipment)");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAxeCollected()) return;

        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            UpdateInteractionPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsAxeCollected()) return;

        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.HidePrompt();
            }
        }
    }
}