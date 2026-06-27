using UnityEngine;

public class Chapter5OpungTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Chapter5StoryManager.Instance != null)
            {
                Chapter5StoryManager.Instance.SetPlayerInOpungZone(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Chapter5StoryManager.Instance != null)
            {
                Chapter5StoryManager.Instance.SetPlayerInOpungZone(false);
            }
        }
    }
}
