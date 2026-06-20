using UnityEngine;
using TMPro;
using System.Collections;

public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance { get; private set; }

    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string defaultMessage = "Tekan E untuk interaksi";
    [SerializeField] private float fadeDuration = 0.5f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Jika promptPanel tidak diisi, gunakan GameObject tempat script ini menempel
        GameObject targetGO = promptPanel != null ? promptPanel : gameObject;

        canvasGroup = targetGO.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = targetGO.AddComponent<CanvasGroup>();
        }

        // Set awal tidak terlihat dan tidak memblokir raycast
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void ShowPrompt(string message = "")
    {
        if (canvasGroup != null)
        {
            if (promptText != null)
            {
                promptText.text = string.IsNullOrEmpty(message) ? defaultMessage : message;
            }

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeRoutine(1f));
        }
    }

    public void HidePrompt()
    {
        if (canvasGroup != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeRoutine(0f));
        }
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (targetAlpha > 0f)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        else
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}
