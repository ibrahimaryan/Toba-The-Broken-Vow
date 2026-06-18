using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackgroundFader : MonoBehaviour
{
    public Image backgroundImage;
    public Image fadeOverlay;
    public float fadeDuration = 0.5f;

    public void SetBackground(Sprite newBg)
    {
        if (backgroundImage != null && backgroundImage.sprite != newBg)
        {
            gameObject.SetActive(true); // Pastikan objek aktif agar Coroutine bisa berjalan
            StartCoroutine(FadeRoutine(newBg));
        }
    }

    private IEnumerator FadeRoutine(Sprite newBg)
    {
        if (fadeOverlay != null)
        {
            // Fade Out
            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeOverlay.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, t / fadeDuration));
                yield return null;
            }
        }

        // Swap Sprite
        if (backgroundImage != null)
        {
            if (newBg == null)
            {
                backgroundImage.sprite = null;
                backgroundImage.color = new Color(1, 1, 1, 0); // Tembus pandang
            }
            else
            {
                backgroundImage.sprite = newBg;
                backgroundImage.color = new Color(1, 1, 1, 1); // Muncul normal
            }
        }

        if (fadeOverlay != null)
        {
            // Fade In
            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeOverlay.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, t / fadeDuration));
                yield return null;
            }
            
            fadeOverlay.color = new Color(0, 0, 0, 0);
        }
    }
}
