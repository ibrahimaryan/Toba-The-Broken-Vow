using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New VNDialogueData", menuName = "Visual Novel/Dialogue Data")]
public class VNDialogueData : ScriptableObject
{
    public List<VNDialogueLine> lines = new List<VNDialogueLine>();
}
