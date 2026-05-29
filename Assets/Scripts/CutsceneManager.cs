using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;

public class CutsceneManager : MonoBehaviour
{
    [Header("Referensi")]
    [Tooltip("Kosongi jika belum ada timeline-nya")]
    public PlayableDirector timelineDirector;
    public Dialogue cutsceneDialogue;
    
    [Header("Pengaturan Fade & Flash")]
    [Tooltip("Panel UI Canvas Group warna hitam untuk transisi")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;
    [Tooltip("Centang jika ingin ada kilatan/flash sebelum Cutscene mulai")]
    public bool gunakanEfekKilatan = true; 
    
    [Header("Pengaturan Eksekusi")]
    public bool putarSaatMulai = false;
    public string cutsceneID = "Cutscene_1";

    [Header("Event Sesudah Cutscene")]
    public UnityEvent OnCutsceneSelesai;

    private PlayerControllerScript player;
    private bool hasPlayed = false;

    private void Start()
    {
        player = FindAnyObjectByType<PlayerControllerScript>();

        if (PlayerPrefs.GetInt(cutsceneID, 0) == 1)
        {
            hasPlayed = true;
            if (fadePanel != null) fadePanel.alpha = 0; 
            return;
        }

        if (putarSaatMulai)
        {
            StartCoroutine(JalankanCutscene());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasPlayed && !putarSaatMulai && collision.CompareTag("Player"))
        {
            StartCoroutine(JalankanCutscene());
        }
    }

    public void PutarManual()
    {
        if (!hasPlayed)
        {
            StartCoroutine(JalankanCutscene());
        }
    }

    private IEnumerator JalankanCutscene()
    {
        hasPlayed = true;
        PlayerPrefs.SetInt(cutsceneID, 1); 

        // 1. Matikan kontrol
        if (player != null) player.ToggleInput(false); 

        // 2. FADE OUT & EFEK KILATAN (Flash)
        if (fadePanel != null)
        {
            if (gunakanEfekKilatan)
            {
                // Eksekusi kilatan (Gelap -> Cepat Terang -> Gelap)
                fadePanel.alpha = 1f;                  // Mulai dari gelap total
                yield return new WaitForSeconds(0.1f); 
                yield return StartCoroutine(FadeRoutine(0f, 0.05f)); // Kilat terang! (sangat cepat)
                yield return StartCoroutine(FadeRoutine(1f, 0.05f)); // Kilat gelap! (sangat cepat)
                yield return new WaitForSeconds(0.5f); // Tahan gelap sebentar biar mantap
            }
            else
            {
                // Fade out standar
                yield return StartCoroutine(FadeRoutine(1f, fadeDuration)); 
            }
        }

        // 3. FADE IN (Layar perlahan terang dan mulai Timeline)
        if (fadePanel != null)
        {
            yield return StartCoroutine(FadeRoutine(0f, fadeDuration));
        }

        // 4. MEMUTAR TIMELINE ANIMASI
        if (timelineDirector != null)
        {
            timelineDirector.Play();
            yield return new WaitUntil(() => timelineDirector.state != PlayState.Playing);
        }

        // 5. MENJALANKAN DIALOG
        if (cutsceneDialogue != null && cutsceneDialogue.lines.Length > 0 && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(cutsceneDialogue);
            yield return new WaitUntil(() => 
                !DialogueManager.instance.screenBoxPanel.activeInHierarchy && 
                !DialogueManager.instance.bubblePanel.activeInHierarchy);
        }

        // 6. MENGAKTIFKAN EVENT SETELAH SELESAI
        OnCutsceneSelesai?.Invoke();

        // 7. MENGEMBALIKAN KONTROL PLAYER
        if (player != null)
        {
            player.ToggleInput(true);
        }
    }

    // Fungsi transparan UI Panel (Diperbarui dengan parameter kecepatan custom)
    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float startAlpha = fadePanel.alpha;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        fadePanel.alpha = targetAlpha;
    }
}
