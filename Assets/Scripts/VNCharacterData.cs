using UnityEngine;

[CreateAssetMenu(fileName = "New VNCharacterData", menuName = "Visual Novel/Character Data")]
public class VNCharacterData : ScriptableObject
{
    public string characterName;
    public Sprite neutralPortrait;
    public Sprite happyPortrait;
    public Sprite angryPortrait;
    public Sprite shockedPortrait;
    public Sprite confusedPortrait;

    public Sprite GetPortrait(VNEmotion emotion)
    {
        switch (emotion)
        {
            case VNEmotion.Happy: return happyPortrait;
            case VNEmotion.Angry: return angryPortrait;
            case VNEmotion.Shocked: return shockedPortrait;
            case VNEmotion.Confused: return confusedPortrait;
            case VNEmotion.Neutral:
            default: 
                return neutralPortrait;
        }
    }
}
