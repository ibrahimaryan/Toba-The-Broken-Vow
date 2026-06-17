using UnityEngine;

public enum VNEmotion
{
    Neutral,
    Happy,
    Angry,
    Shocked,
    Confused
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
    
    public VNEmotion emotion;
    public VNPortraitPosition position;
    
    [Header("Optional Overrides")]
    public Sprite backgroundOverride;
}
