using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections; 
using System.Collections.Generic;

public class SecretItem : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject popUpPanel;
    [SerializeField] private Image displayImage;
    [SerializeField] private Sprite[] secretSprites;
    
    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 1.5f; 
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.4f; 

    private int currentSpriteIndex = 0;
    private bool canInteract = true;
    private bool isPlayerInRange = false;

    private List<int> randomizedIndices = new List<int>();
    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        RandomizeOrder();
    }

    private void Start()
    {
        if (canInteract)
        {
            blinkCoroutine = StartCoroutine(BlinkEffect());
        }
    }

    private void RandomizeOrder()
    {
        for (int i = 0; i < secretSprites.Length; i++)
        {
            randomizedIndices.Add(i);
        }

        for (int i = 0; i < randomizedIndices.Count; i++)
        {
            int temp = randomizedIndices[i];
            int randomIndex = UnityEngine.Random.Range(i, randomizedIndices.Count);
            randomizedIndices[i] = randomizedIndices[randomIndex];
            randomizedIndices[randomIndex] = temp;
        }
    }

    private void OnEnable() {
        PlayerControllerScript.OnInteractPressed += HandleInteraction;
        PlayerControllerScript.OnClosePressed += ClosePopUp;
    }

    private void OnDisable() {
        PlayerControllerScript.OnInteractPressed -= HandleInteraction;
        PlayerControllerScript.OnClosePressed -= ClosePopUp;
    }

    public void ClosePopUp() {
        if (popUpPanel.activeSelf) {
            popUpPanel.SetActive(false);
        }
    }

    private void HandleInteraction() {
        if (isPlayerInRange && canInteract) {
            ShowPopUp();
        }
    }

    void ShowPopUp() {
        int spriteToDisplay = randomizedIndices[currentSpriteIndex];
        displayImage.sprite = secretSprites[spriteToDisplay];
        
        popUpPanel.SetActive(true);
        canInteract = false; 

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            ResetSpriteColor(); 
        }
    }

    public void ResetInteractions() {
        canInteract = true;
        currentSpriteIndex = (currentSpriteIndex + 1) % secretSprites.Length;

        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkEffect());
    }

    // PERBAIKAN: Menggunakan IEnumerator non-generik bawaan System.Collections
    private IEnumerator BlinkEffect()
    {
        while (canInteract && spriteRenderer != null)
        {
            float lerpTime = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            float alpha = Mathf.Lerp(minAlpha, 1f, lerpTime);

            spriteRenderer.color = new Color(1f, 1f, 1f, alpha);

            yield return null; 
        }
    }

    private void ResetSpriteColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white; 
        }
    }

    public int GetCurrentSecretIndex()
    {
        return randomizedIndices[currentSpriteIndex];
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) isPlayerInRange = false;
    }
}