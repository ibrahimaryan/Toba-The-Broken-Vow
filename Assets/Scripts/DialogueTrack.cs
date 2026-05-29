using UnityEngine.Timeline;

// Menjadikannya trek khusus yang muncul warnanya di Timeline
[TrackColor(0.855f, 0.903f, 0.87f)] 
[TrackClipType(typeof(DialogueClip))]
public class DialogueTrack : TrackAsset
{
    // Hanya penanda bahwa ini adalah Trek Playable buatan kita  
}