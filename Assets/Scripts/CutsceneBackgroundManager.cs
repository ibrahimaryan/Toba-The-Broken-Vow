using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneBackgroundManager : MonoBehaviour
{
    public Image backgroundImage;

    [Header("Fade")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 0.5f;

    public IEnumerator ChangeBackground(Sprite newBackground)
    {
        if (newBackground == null)
            yield break;

        yield return Fade(1f);

        backgroundImage.sprite = newBackground;

        yield return Fade(0f);
    }

    IEnumerator Fade(float target)
    {
        if (fadePanel == null)
            yield break;

        fadePanel.gameObject.SetActive(true);

        while (!Mathf.Approximately(
            fadePanel.alpha,
            target))
        {
            fadePanel.alpha =
                Mathf.MoveTowards(
                    fadePanel.alpha,
                    target,
                    Time.deltaTime / fadeDuration);

            yield return null;
        }

        if (target <= 0)
            fadePanel.gameObject.SetActive(false);
    }
}