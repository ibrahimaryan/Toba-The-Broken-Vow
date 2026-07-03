using UnityEngine;

public enum VNEmotion
{
    Neutral,
    Happy,
    Angry,
    Shocked,
    Confused,
    Sad,
    Optional
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

    [Header("Auto-Play Khusus (Transisi/Animasi)")]
    [Tooltip("Jika dicentang, baris ini akan berjalan otomatis tanpa perlu di-klik, dan menyembunyikan panel dialog.")]
    public bool isAutoPlay = false;
    [Tooltip("Berapa detik jeda sebelum lanjut ke baris berikutnya secara otomatis.")]
    public float autoPlayDelay = 2.0f;
}
