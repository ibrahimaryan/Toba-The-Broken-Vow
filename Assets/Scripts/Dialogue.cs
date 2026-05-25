using UnityEngine;

// System.Serializable wajib ditambahkan agar class ini muncul di Inspector Unity
[System.Serializable]
public class DialogueLine
{
    public string characterName; // Nama karakter yang bicara
    
    [TextArea(3, 10)] // Membuat kolom teks di Inspector jadi lebih lebar
    public string sentence; // Isi dialognya
    
    public bool isSpeechBubble; // Centang ini di Inspector jika dialognya mau muncul sebagai Bubble
}

[System.Serializable]
public class Dialogue
{
    // Array untuk menyimpan urutan percakapan
    public DialogueLine[] lines; 
}