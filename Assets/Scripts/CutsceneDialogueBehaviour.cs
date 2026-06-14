using UnityEngine;
using UnityEngine.Playables;

public class CutsceneDialogueBehaviour
    : PlayableBehaviour
{
    public CutsceneDialogue dialogueData;

    bool played;

    public override void ProcessFrame(
        Playable playable,
        FrameData info,
        object playerData)
    {
        if (played)
            return;

        if (CutsceneDialogueManager.Instance == null)
            return;

        played = true;

        CutsceneDialogueManager.Instance
            .StartDialogue(dialogueData);
    }

    public override void OnBehaviourPause(
        Playable playable,
        FrameData info)
    {
        played = false;
    }
}