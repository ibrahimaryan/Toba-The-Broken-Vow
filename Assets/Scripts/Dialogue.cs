using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string characterName; // Nama karakter yang bicara
    
    [TextArea(3, 10)] // Membuat kolom teks di Inspector jadi lebih lebar
    public string sentence; // Isi dialognya
    
    [Tooltip("Centang ini jika dialognya mau muncul sebagai Bubble di atas objek")]
    public bool isSpeechBubble; // Centang ini di Inspector jika dialognya mau muncul sebagai Bubble
    
    [Tooltip("Centang ini jika dialognya mau muncul besar secara OTOMATIS ala Cutscene Film")]
    public bool isCutsceneStyle; // Centang ini di Inspector jika dialognya mau muncul besar secara OTOMATIS ala Cutscene Film
}

[System.Serializable]
public class Dialogue
{
    // Array untuk menyimpan urutan percakapan
    public DialogueLine[] lines; 
}