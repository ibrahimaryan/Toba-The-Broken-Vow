using UnityEngine;

public class Cameracontroller : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private bool followPlayer = true; // Centang di inspector luar_rumah, jangan dicentang di kamar
    [SerializeField] private Transform playerTarget;
    
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); // Offset Z agar kamera tidak menabrak player secara 2D

    private void Start()
    {
        // Jika target belum dipasang di Inspector, coba cari otomatis di scene
        if (playerTarget == null)
        {
            var player = FindAnyObjectByType<PlayerControllerScript>();
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }
    }

    private void LateUpdate()
    {
        // Jika diatur untuk mengikuti player dan player ada
        if (followPlayer && playerTarget != null)
        {
            Vector3 desiredPosition = playerTarget.position + offset;
            // Perpindahan kamera yang halus (Smooth)
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}
