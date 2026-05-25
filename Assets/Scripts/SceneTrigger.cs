using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Panggil fungsi pindah scene di induknya (Gate)
            GetComponentInParent<DoorController>().GoToNextScene();
        }
    }
}