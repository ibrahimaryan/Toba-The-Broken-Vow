using UnityEngine;

public class StoryHutTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Chapter3StoryManager.Instance != null)
            {
                Chapter3StoryManager.Instance.SetPlayerInHutZone(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Chapter3StoryManager.Instance != null)
            {
                Chapter3StoryManager.Instance.SetPlayerInHutZone(false);
            }
        }
    }
}
