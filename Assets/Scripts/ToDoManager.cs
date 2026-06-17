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
        Instance = this;
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