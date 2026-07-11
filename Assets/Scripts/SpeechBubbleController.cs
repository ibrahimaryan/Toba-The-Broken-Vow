using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem; // Tambahkan library pemanggil New Input System
using UnityEngine.SceneManagement;

public class SpeechBubbleController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject bubblePanel;
    public TextMeshProUGUI dialogText;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (bubblePanel != null)
        {
            bubblePanel.SetActive(false);
        }
    }

    void Start()
    {
        // Pastikan saat game mulai, bubble dalam keadaan tersembunyi
        if (bubblePanel != null)
        {
            bubblePanel.SetActive(false);
        }
    }

    void Update()
    {
        // Pencegahan error jika tidak ada keyboard yang terhubung
        if (Keyboard.current == null) return;

        // Menggunakan sintaks New Input System untuk mendeteksi tombol T
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            ShowBubble("Hah!, Dimana ini? Dapur ini berantakan sekali...", 3f);
        }
    }

    public void ShowBubble(string message, float duration)
    {
        StopAllCoroutines(); 
        StartCoroutine(RoutineShowBubble(message, duration));
    }

    private IEnumerator RoutineShowBubble(string message, float duration)
    {
        dialogText.text = message; 
        bubblePanel.SetActive(true); 

        yield return new WaitForSeconds(duration); 

        bubblePanel.SetActive(false); 
    }
}