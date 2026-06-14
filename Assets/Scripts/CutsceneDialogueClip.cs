using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class CutsceneDialogueClip
    : PlayableAsset
{
    public CutsceneDialogue dialogueData;

    public override Playable CreatePlayable(
        PlayableGraph graph,
        GameObject owner)
    {
        var playable =
            ScriptPlayable
            <CutsceneDialogueBehaviour>
            .Create(graph);

        playable
            .GetBehaviour()
            .dialogueData =
            dialogueData;

        return playable;
    }
}