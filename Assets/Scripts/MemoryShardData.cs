using UnityEngine;

[CreateAssetMenu(fileName = "New MemoryShardData", menuName = "Visual Novel/Memory Shard Data")]
public class MemoryShardData : ScriptableObject
{
    public string shardID;
    public string title;
    public Sprite thumbnail;
    public VNDialogueData dialogueData;
    public bool isUnlocked = false;

    [Header("Chapter Intro Settings (Optional)")]
    public string chapterName;
    public Sprite silhouetteSprite;
    [TextArea(3, 5)]
    public string loreTextContent;
}
