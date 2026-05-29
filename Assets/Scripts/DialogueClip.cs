using UnityEngine;
using UnityEngine.Playables;

// Ini adalah data yang disimpan di dalam BALOK TIMELINE
[System.Serializable]
public class DialogueClip : PlayableAsset
{
    [Tooltip("Isi dengan kalimat dialog di sini")]
    public Dialogue dialogueData;

    // Fungsi wajib pembuat instance balok
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DialogueBehaviour>.Create(graph);
        DialogueBehaviour behaviour = playable.GetBehaviour();
        behaviour.dialogueData = dialogueData;
        return playable;
    }
}