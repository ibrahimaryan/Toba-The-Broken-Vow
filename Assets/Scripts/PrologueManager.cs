using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PrologueManager : MonoBehaviour
{
    [Header("Prologue Data")]
    public VNDialogueData prologueData;

    [Header("UI References")]
    public GameObject prologuePanel;
    public TextMeshProUGUI prologueText;

    [Header("Audio Settings")]
    public AudioSource sfxSource;
    [Tooltip("Jeda waktu sebelum otomatis pindah ke baris teks selanjutnya")]
    public float defaultAutoPlayDelay = 2.5f;
    [Tooltip("Kecepatan ketikan teks (semakin kecil semakin cepat)")]
    public float typingSpeed = 0.04f;

    [Header("Events")]
    [Tooltip("Nama Scene Gameplay yang akan dimuat setelah prolog selesai (Kosongkan jika tidak pindah scene)")]
    public string nextSceneToLoad;
    [Tooltip("Apa yang terjadi setelah prolog selesai? (Bisa dipakai untuk efek transisi / hal lain)")]
    public UnityEvent onPrologueFinished;

    private bool skipRequested = false;

    private void Start()
    {
        Debug.Log("[PrologueManager] Memulai pengecekan Prologue Data...");
        if (prologueData != null && prologueData.lines.Count > 0)
        {
            Debug.Log($"[PrologueManager] Data Prologue ditemukan: {prologueData.lines.Count} baris teks. Menyalakan UI ProloguePanel...");
            if (prologuePanel != null) prologuePanel.SetActive(true);
            StartCoroutine(RunPrologueSequence());
        }
        else
        {
            Debug.LogError("[PrologueManager] ERROR: Prolog Data kosong atau belum dimasukkan ke PrologueManager di Inspector!");
        }
    }

    private void Update()
    {
        // Deteksi klik kiri atau spasi/enter untuk mempercepat/skip
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame) skipRequested = true;
        if (UnityEngine.InputSystem.Keyboard.current != null && (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)) skipRequested = true;
#else
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) skipRequested = true;
#endif
    }

    private IEnumerator RunPrologueSequence()
    {
        foreach (var line in prologueData.lines)
        {
            skipRequested = false;

            // 1. Mainkan Audio / Voice Over
            if (line.sfxClip != null && sfxSource != null)
            {
                sfxSource.clip = line.sfxClip;
                sfxSource.Play();
            }

            // 2. Ketik teks perlahan
            Debug.Log($"[PrologueManager] Menampilkan teks: {line.text}");
            if (prologueText != null) prologueText.text = "";
            
            if (!string.IsNullOrEmpty(line.text) && prologueText != null)
            {
                foreach (char c in line.text.ToCharArray())
                {
                    if (skipRequested)
                    {
                        // Jika pemain mengeklik, langsung tampilkan semua teks seketika
                        prologueText.text = line.text;
                        skipRequested = false; // Reset permintaan skip agar tidak langsung meloncati jeda
                        break;
                    }
                    prologueText.text += c;
                    yield return new WaitForSeconds(typingSpeed);
                }
            }

            // 3. Jeda waktu tunggu default setelah teks utuh
            float timer = 0f;
            while (timer < defaultAutoPlayDelay)
            {
                // Jika pemain mengeklik saat jeda, langsung akhiri jeda dan pindah baris
                if (skipRequested) break; 
                timer += Time.deltaTime;
                yield return null;
            }

            // 4. Wait for audio - Selalu pastikan suara habis sebelum lanjut ke elemen teks berikutnya
            if (!skipRequested && sfxSource != null && sfxSource.isPlaying)
            {
                while (sfxSource.isPlaying)
                {
                    // Jika pemain tidak sabar dan mengeklik saat mendengarkan audio panjang
                    if (skipRequested)
                    {
                        sfxSource.Stop(); // Hentikan audio secara paksa
                        break;
                    }
                    yield return null;
                }
            }
        }

        // 5. Prologue Selesai Sepenuhnya
        Debug.Log("[PrologueManager] Prologue selesai! Mematikan UI dan memanggil Event OnPrologueFinished...");
        if (prologuePanel != null) prologuePanel.SetActive(false);
        if (prologueText != null) prologueText.text = "";
        
        // Panggil event (misalnya: transisi fade out)
        onPrologueFinished?.Invoke();

        // Pindah Scene jika namanya diisi
        if (!string.IsNullOrEmpty(nextSceneToLoad))
        {
            SceneManager.LoadScene(nextSceneToLoad);
        }
    }
}
