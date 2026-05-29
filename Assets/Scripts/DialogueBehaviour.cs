using UnityEngine;
using UnityEngine.Playables;

public class DialogueBehaviour : PlayableBehaviour
{
    public Dialogue dialogueData;
    private bool hasPlayed = false;

    // Saat jarum Timeline MASUK ke titik awal balok (Frame demi frame update)
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // Pakai ProcessFrame lebih andal dibanding OnBehaviourPlay saat digeser-geser.
        if (!hasPlayed && Application.isPlaying && info.weight > 0)
        {
            hasPlayed = true;
            if (DialogueManager.instance != null && dialogueData != null)
            {
                DialogueManager.instance.StartDialogue(dialogueData);
            }
        }
    }

    // Saat jarum Timeline KELUAR dari titik akhir balok
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (hasPlayed && Application.isPlaying)
        {
            // Reset state agar clip ini bisa jalan lagi kalau di-rewind
            hasPlayed = false;

            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.TutupPaksaSeluruhPanel();
            }
        }
    }
}