using UnityEngine;
using UnityEngine.Playables;

public class TimelineDialogTrigger : MonoBehaviour
{
    [Header("Daftar Obrolan (Cutscene)")]
    public Dialogue[] listDialogs;
    public string triggerID; 

    public void PutarDialogNomor0() => EksekusiDialogAman(0);
    public void PutarDialogNomor1() => EksekusiDialogAman(1);
    public void PutarDialogNomor2() => EksekusiDialogAman(2);

    private void EksekusiDialogAman(int index)
    {
        if (DialogueManager.instance == null) return;
        
        string uniqueFlag = string.IsNullOrEmpty(triggerID) ? "" : $"{triggerID}_{index}";

        // Tembok 1: Cek apakah dialog urutan INI pernah diputar
        if (!string.IsNullOrEmpty(uniqueFlag) && GameManager.Instance != null && GameManager.Instance.IsFlagSet(uniqueFlag)) 
        {
            Debug.Log($"<color=yellow>Dialog ke-{index} diblokir, karena ID '{uniqueFlag}' sudah True.</color>");
            return; 
        }

        if (index >= 0 && index < listDialogs.Length)
        {
            Debug.Log($"<color=green>Memutar Dialog ke-{index}. Dipanggil oleh Timeline!</color>");
            DialogueManager.instance.StartDialogue(listDialogs[index]);

            if (GameManager.Instance != null && !string.IsNullOrEmpty(uniqueFlag))
            {
                GameManager.Instance.SetFlag(uniqueFlag, true);
            }
        }
    }

    public void PutarDialogDariString(string textIndex)
    {
        if (DialogueManager.instance == null) return;

        if (int.TryParse(textIndex, out int parsedIndex))
        {
            string uniqueFlag = string.IsNullOrEmpty(triggerID) ? "" : $"{triggerID}_{parsedIndex}";

            if (!string.IsNullOrEmpty(uniqueFlag) && GameManager.Instance != null && GameManager.Instance.IsFlagSet(uniqueFlag))
            {
                Debug.Log($"<color=yellow>Dialog ke-{parsedIndex} diblokir, karena ID '{uniqueFlag}' sudah True.</color>");
                return;
            }

            if (parsedIndex >= 0 && parsedIndex < listDialogs.Length)
            {
                Debug.Log($"<color=green>Memutar Dialog ke-{parsedIndex}. Dipanggil oleh Timeline!</color>");
                DialogueManager.instance.StartDialogue(listDialogs[parsedIndex]);

                if (GameManager.Instance != null && !string.IsNullOrEmpty(uniqueFlag))
                {
                    GameManager.Instance.SetFlag(uniqueFlag, true);
                }
            }
        }
    }
}