using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public string lastExitDoorID;

    private Dictionary<string, bool> persistentFlags = new Dictionary<string, bool>();

    private void Awake()
    {
        Debug.Log("GameManager Awake");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFlags(); // Muat flags dari PlayerPrefs saat game mulai
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Hancurkan GameManager jika masuk ke scene MainMenu
        if (scene.name == "MainMenu")
        {
            Destroy(gameObject);
        }
    }

    public bool IsFlagSet(string flagID)
    {
        return persistentFlags.ContainsKey(flagID) && persistentFlags[flagID];
    }

    public void SetFlag(string flagID, bool value = true)
    {
        persistentFlags[flagID] = value;
        Debug.Log($"GameManager: flag {flagID} = {value}");
        SaveFlags(); // Simpan otomatis setiap ada perubahan flag
    }

    public bool IsDoorOpened(string doorID)
    {
        return IsFlagSet("door_" + doorID);
    }

    public void SetDoorOpened(string doorID, bool isOpen)
    {
        SetFlag("door_" + doorID, isOpen);
    }

    // --- SAVE & LOAD PERSISTENT FLAGS ---
    private void SaveFlags()
    {
        List<string> keys = new List<string>(persistentFlags.Keys);
        string serializedKeys = string.Join(",", keys);
        PlayerPrefs.SetString("GameManager_FlagKeys", serializedKeys);

        foreach (var kvp in persistentFlags)
        {
            PlayerPrefs.SetInt("GameManager_Flag_" + kvp.Key, kvp.Value ? 1 : 0);
        }
        PlayerPrefs.Save();
        Debug.Log("GameManager: Flags autosaved successfully.");
    }

    private void LoadFlags()
    {
        persistentFlags.Clear();
        if (PlayerPrefs.HasKey("GameManager_FlagKeys"))
        {
            string serializedKeys = PlayerPrefs.GetString("GameManager_FlagKeys");
            if (!string.IsNullOrEmpty(serializedKeys))
            {
                string[] keys = serializedKeys.Split(',');
                foreach (string key in keys)
                {
                    if (PlayerPrefs.HasKey("GameManager_Flag_" + key))
                    {
                        persistentFlags[key] = PlayerPrefs.GetInt("GameManager_Flag_" + key) == 1;
                    }
                }
            }
            Debug.Log("GameManager: Flags loaded successfully from PlayerPrefs.");
        }
    }
}