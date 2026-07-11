using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Quest
{
    public string namaMisi;    
    public bool sudahSelesai;  
}

public class ToDoManager : MonoBehaviour
{
    public static ToDoManager Instance;

    [Header("Referensi UI")]
    public GameObject toDoPanel; 
    public TextMeshProUGUI listText;

    [Header("Daftar Misi Jangka Panjang")]
    public string currentChapterID; // Mengetahui ID chapter yang sedang aktif saat ini
    public List<Quest> daftarMisi; 

    // Urutan prioritas chapter — semakin tinggi indeks, semakin maju
    private static readonly string[] chapterOrder = new string[]
    {
        "Chapter_1", "Chapter_2", "Chapter_3", "Chapter_4", "Chapter_5"
    };

    // Kembalikan true jika newID lebih maju atau sama dengan currentChapterID
    public bool IsChapterAtLeast(string newID)
    {
        int newIndex = System.Array.IndexOf(chapterOrder, newID);
        int curIndex = System.Array.IndexOf(chapterOrder, currentChapterID);
        // Jika salah satu tidak dikenal, izinkan
        if (newIndex < 0 || curIndex < 0) return true;
        return newIndex >= curIndex;
    }

    public void SetCurrentChapterID(string newID)
    {
        if (!IsChapterAtLeast(newID)) return; // Tolak jika mundur
        currentChapterID = newID;
        PlayerPrefs.SetString("ToDoManager_ChapterID", currentChapterID);
        PlayerPrefs.Save();
    }

    private bool isPanelAktif = false; 

    void Awake()
    {
       if (Instance == null)
        {
            Instance = this;
            // Restore chapter ID yang tersimpan agar tidak mundur setelah restart
            string saved = PlayerPrefs.GetString("ToDoManager_ChapterID", "");
            if (!string.IsNullOrEmpty(saved))
            {
                currentChapterID = saved;
            }
        }
    }

    void Start()
    {
        if (toDoPanel != null)
        {
            toDoPanel.SetActive(false);
            isPanelAktif = false;
        }
        UpdateTampilanUI();
    }

    public void ToggleToDoList()
    {
        isPanelAktif = !isPanelAktif; 
        toDoPanel.SetActive(isPanelAktif); 
    }

    public void SelesaikanMisi(int urutanMisi)
    {
        if (urutanMisi >= 0 && urutanMisi < daftarMisi.Count)
        {
            daftarMisi[urutanMisi].sudahSelesai = true;
            UpdateTampilanUI(); 
        }
    }

    public void UpdateTampilanUI()
    {
        listText.text = ""; 

        if (GameManager.Instance != null && GameManager.Instance.IsFlagSet("chapter1_statue_solved") &&
            (currentChapterID == "Chapter_1"))
        {
            listText.text = "<color=#808080><s>- Tantangan chapter ini sudah selesai</s></color>\n";
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsFlagSet("sisik_puzzle_solved") &&
            (currentChapterID == "Chapter_1" || currentChapterID == "Chapter_2"))
        {
            listText.text = "<color=#808080><s>- Tantangan chapter ini sudah selesai</s></color>\n";
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsFlagSet("chapter5_completed"))
        {
            listText.text = "<color=#808080><s>- Semua tantangan sudah selesai</s></color>\n";
            return;
        }

        foreach (Quest misi in daftarMisi)
        {
            if (misi.sudahSelesai)
            {
                listText.text += "<color=#808080><s>- " + misi.namaMisi + "</s></color>\n";
            }
            else
            {
                listText.text += "- " + misi.namaMisi + "\n";
            }
        }
    }
}