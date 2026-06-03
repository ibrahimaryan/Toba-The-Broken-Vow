using UnityEngine;
using System.Collections.Generic;

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
        }
        else
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
    }

    public bool IsDoorOpened(string doorID)
    {
        return IsFlagSet("door_" + doorID);
    }

    public void SetDoorOpened(string doorID, bool isOpen)
    {
        SetFlag("door_" + doorID, isOpen);
    }
}