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

    private bool isPanelAktif = false; 

    void Awake()
    {
       if (Instance == null)
        {
            Instance = this;
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

        if (GameManager.Instance != null && GameManager.Instance.IsFlagSet("sisik_puzzle_solved") &&
            (currentChapterID == "Chapter_1" || currentChapterID == "Chapter_2"))
        {
            listText.text = "<color=#808080><s>- Semua tantangan sudah selesai</s></color>\n";
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