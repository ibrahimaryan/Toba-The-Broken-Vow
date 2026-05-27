using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Menyimpan ID pintu terakhir yang dimasuki player
    public string lastExitDoorID;

    private void Awake()
    {
        Debug.Log("GameManager Awake");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}