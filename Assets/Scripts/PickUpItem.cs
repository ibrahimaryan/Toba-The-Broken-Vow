using UnityEngine;
using System.Collections;

public class PickUpItem : MonoBehaviour
{
    [SerializeField] private string itemID;
    [SerializeField] private int amount = 1;
    [SerializeField] private AudioClip collectSound;

    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 1.5f; 
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.4f; 

    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Start()
    {
        if (spriteRenderer != null)
        {
            blinkCoroutine = StartCoroutine(BlinkEffect());
        }
    }

    private void OnDisable()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
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
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(itemID, amount);
                
                // Update quest UI jika Chapter 4 sedang aktif
                if (Chapter4StoryManager.Instance != null)
                {
                    Chapter4StoryManager.Instance.UpdateQuestStatus();
                }
            }

            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, Camera.main != null ? Camera.main.transform.position : transform.position);
            }

            Destroy(gameObject);
        }
    }
}
