using UnityEngine;
using TMPro;

public class PasswordTerminal : MonoBehaviour
{
    [SerializeField] private string[] correctCodes;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private SecretItem firstItem;
    [SerializeField] private GameObject panel;

    private int attemptCount = 0;
    private int codeIndex = 0;
    private bool isPlayerInRange = false;

    private void OnEnable()
    {
        PlayerControllerScript.OnInteractPressed += HandleInteraction;
        PlayerControllerScript.OnClosePressed += ClosePanel;
    }

    private void OnDisable()
    {
        PlayerControllerScript.OnInteractPressed -= HandleInteraction;
        PlayerControllerScript.OnClosePressed -= ClosePanel;
    }

    private void HandleInteraction()
    {
        if (isPlayerInRange)
        {
            OpenPanel();
        }
    }

    public void CheckPassword()
    {
        if (string.IsNullOrEmpty(inputField.text)) return;

        // TANYA: Gambar index berapa yang tadi muncul di benda pertama?
        int actualIndex = firstItem.GetCurrentSecretIndex();

        if (inputField.text == correctCodes[actualIndex])
        {
            Debug.Log("Kode Benar!");
            ClosePanel();
        }
        else
        {
            Debug.Log("Kode Salah!");
            attemptCount++;
            inputField.text = "";
            inputField.ActivateInputField();
            
            if (attemptCount >= 3)
            {
                attemptCount = 0;
                // Kita tidak perlu menambah codeIndex manual lagi
                firstItem.ResetInteractions();
                ClosePanel();
            }
        }
    }

    public void OpenPanel()
    {
        if (panel != null)
            panel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = false;
    }
}