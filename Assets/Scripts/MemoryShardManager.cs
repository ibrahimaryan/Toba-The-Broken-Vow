using UnityEngine;
using System.Collections.Generic;

public class MemoryShardManager : MonoBehaviour
{
    [Header("Memory Shards")]
    public List<MemoryShardData> allShards = new List<MemoryShardData>();
    
    [Header("References")]
    public DialogueManagerCS dialogueManager;

    public void UnlockShard(string shardID)
    {
        foreach (var shard in allShards)
        {
            if (shard.shardID == shardID)
            {
                shard.isUnlocked = true;
                Debug.Log($"Memory Shard Unlocked: {shard.title}");
                break;
            }
        }
    }

    public void PlayShardDialogue(MemoryShardData shard)
    {
        if (shard.isUnlocked && shard.dialogueData != null && dialogueManager != null)
        {
            dialogueManager.PlayDialogue(shard.dialogueData);
        }
        else if (!shard.isUnlocked)
        {
            Debug.LogWarning("Cannot play dialogue: Memory Shard is locked.");
        }
    }
}
