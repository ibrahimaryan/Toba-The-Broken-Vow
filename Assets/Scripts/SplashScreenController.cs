using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenController : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "MainMenu";

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup; // Masukkan SplashImage ke sini
    [SerializeField] private float fadeInDuration = 1.5f;   
    [SerializeField] private float stayDuration = 2f;      
    [SerializeField] private float fadeOutDuration = 1.5f;  

    [Header("Scale Settings")]
    [SerializeField] private float startScale = 0.95f;    // Ukuran awal logo (e.g. 95%)
    [SerializeField] private float endScale = 1.05f;      // Ukuran akhir logo setelah membesar (e.g. 105%)

    void Start()
    {
        if (fadeCanvasGroup != null)
        {
            StartCoroutine(SplashScreenSequence());
        }
        else
        {
            Debug.LogError("Tolong hubungkan Canvas Group dari SplashImage ke Inspector!");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private IEnumerator SplashScreenSequence()
    {
        float totalDuration = fadeInDuration + stayDuration + fadeOutDuration;
        float elapsed = 0f;

        // Set ukuran awal logo
        fadeCanvasGroup.transform.localScale = new Vector3(startScale, startScale, 1f);

        // 1. FADE IN
        float counter = 0f;
        while (counter < fadeInDuration)
        {
            counter += Time.deltaTime;
            elapsed += Time.deltaTime;
            
            // Efek memudar masuk (Alpha)
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, counter / fadeInDuration);
            
            // Efek membesar perlahan (Scale)
            float currentScale = Mathf.Lerp(startScale, endScale, elapsed / totalDuration);
            fadeCanvasGroup.transform.localScale = new Vector3(currentScale, currentScale, 1f);
            
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;

        // 2. STAY (Tetap membesar perlahan saat gambar penuh)
        float stayCounter = 0f;
        while (stayCounter < stayDuration)
        {
            stayCounter += Time.deltaTime;
            elapsed += Time.deltaTime;

            float currentScale = Mathf.Lerp(startScale, endScale, elapsed / totalDuration);
            fadeCanvasGroup.transform.localScale = new Vector3(currentScale, currentScale, 1f);

            yield return null;
        }

        // 3. FADE OUT
        counter = 0f;
        while (counter < fadeOutDuration)
        {
            counter += Time.deltaTime;
            elapsed += Time.deltaTime;

            // Efek memudar keluar (Alpha)
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, counter / fadeOutDuration);

            // Efek membesar perlahan (Scale)
            float currentScale = Mathf.Lerp(startScale, endScale, elapsed / totalDuration);
            fadeCanvasGroup.transform.localScale = new Vector3(currentScale, currentScale, 1f);

            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.transform.localScale = new Vector3(endScale, endScale, 1f);

        yield return new WaitForSeconds(0.3f);

        // 4. PINDAH SCENE
        SceneManager.LoadScene(nextSceneName);
    }
}