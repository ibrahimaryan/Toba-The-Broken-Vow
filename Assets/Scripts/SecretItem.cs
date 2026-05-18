using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic; // Dibutuhkan untuk List

public class SecretItem : MonoBehaviour
{
    [SerializeField] private GameObject popUpPanel;
    [SerializeField] private Image displayImage;
    [SerializeField] private Sprite[] secretSprites;
    
    private int currentSpriteIndex = 0;
    private bool canInteract = true;
    private bool isPlayerInRange = false;

    // Tambahkan variabel untuk menyimpan urutan acak
    private List<int> randomizedIndices = new List<int>();

    private void Awake()
    {
        RandomizeOrder();
    }

    private void RandomizeOrder()
    {
        // Masukkan semua index (0, 1, 2) ke dalam list
        for (int i = 0; i < secretSprites.Length; i++)
        {
            randomizedIndices.Add(i);
        }

        // Acak urutan list menggunakan algoritma Fisher-Yates sederhana
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
        // Ambil sprite berdasarkan urutan yang sudah diacak
        int spriteToDisplay = randomizedIndices[currentSpriteIndex];
        displayImage.sprite = secretSprites[spriteToDisplay];
        
        popUpPanel.SetActive(true);
        canInteract = false; 
    }

    public void ResetInteractions() {
        canInteract = true;
        // Pindah ke urutan acak berikutnya
        currentSpriteIndex = (currentSpriteIndex + 1) % secretSprites.Length;
    }

    // Fungsi tambahan agar PasswordTerminal tahu index mana yang sedang aktif
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