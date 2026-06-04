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
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;
    public bool gunakanEfekKilatan = true; 
    
    [Header("Pengaturan Eksekusi")]
    public bool putarSaatMulai = false;
    public string cutsceneID = "Cutscene_1";

    [Header("Event Sesudah Cutscene")]
    public UnityEvent OnCutsceneSelesai;

    private PlayerControllerScript player;
    private bool hasPlayed = false;

    // PINDAH KE AWAKE: Agar langsung dieksekusi sebelum PlayableDirector mulai jalan otomatis
    private void Awake() 
    {
        bool alreadyPlayed = false;
        
        // Cek apakah sudah pernah main
        if (GameManager.Instance != null && GameManager.Instance.IsFlagSet(cutsceneID))
        {
            alreadyPlayed = true;
        }
        else if (PlayerPrefs.GetInt(cutsceneID, 0) == 1)
        {
            alreadyPlayed = true;
        }

        if (alreadyPlayed)
        {
            hasPlayed = true;
            Debug.Log($"<color=red>CutsceneManager ({cutsceneID}): SUDAH PERNAH DIMAINKAN. MENONAKTIFKAN TIMELINE!</color>");
            
            if (fadePanel != null) fadePanel.alpha = 0; 
            
            // Matikan timeline seketika itu juga
            if (timelineDirector != null)
            {
                timelineDirector.playOnAwake = false;
                timelineDirector.Stop();
                timelineDirector.enabled = false; 
            }
            else 
            {
                Debug.LogWarning("PERHATIAN: Kolom 'Timeline Director' di Inspector CutsceneManager kosong! Timeline mungkin masih jalan sendiri.");
            }
        }
    }

    private void Start()
    {
        player = FindAnyObjectByType<PlayerControllerScript>();

        // Lakukan pemutaran jika belum pernah dan emang disuruh pas mulai
        if (!hasPlayed && putarSaatMulai)
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag(cutsceneID, true);
        }

        if (player != null) player.ToggleInput(false); 

        if (fadePanel != null)
        {
            if (gunakanEfekKilatan)
            {
                fadePanel.alpha = 1f;                  
                yield return new WaitForSeconds(0.1f); 
                yield return StartCoroutine(FadeRoutine(0f, 0.05f)); 
                yield return StartCoroutine(FadeRoutine(1f, 0.05f)); 
                yield return new WaitForSeconds(0.5f); 
            }
            else
            {
                yield return StartCoroutine(FadeRoutine(1f, fadeDuration)); 
            }
            yield return StartCoroutine(FadeRoutine(0f, fadeDuration));
        }

        if (timelineDirector != null)
        {
            timelineDirector.Play();
            yield return new WaitUntil(() => timelineDirector.state != PlayState.Playing);
        }

        OnCutsceneSelesai?.Invoke();

        if (player != null)
        {
            player.ToggleInput(true);
        }
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float startAlpha = fadePanel.alpha;
        float time = 0;
        while (time < duration)
        {
            if (fadePanel == null) yield break;
            time += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        if (fadePanel != null) fadePanel.alpha = targetAlpha;
    }
}
