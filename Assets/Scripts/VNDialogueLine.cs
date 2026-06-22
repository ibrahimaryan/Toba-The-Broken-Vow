using UnityEngine;

public enum VNEmotion
{
    Neutral,
    Happy,
    Angry,
    Shocked,
    Confused,
    Sad
}

public enum VNPortraitPosition
{
    Left,
    Center,
    Right
}

[System.Serializable]
public class VNDialogueLine
{
    public VNCharacterData speaker;
    
    [TextArea(3, 5)]
    public string text;
    
    [Tooltip("Jika dicentang, teks ini akan muncul di tengah layar tanpa kotak dialog (Khusus Prolog).")]
    public bool isPrologueCenterText = false;
    
    public VNEmotion emotion;
    public VNPortraitPosition position;
    
    [Header("Optional Overrides")]
    public Sprite backgroundOverride;
    
    [Header("Audio")]
    public AudioClip sfxClip;
    [Tooltip("Jika dicentang, Auto-Play Prologue akan tertahan hingga audio selesai dimainkan.")]
    public bool waitForAudio = false;
}
