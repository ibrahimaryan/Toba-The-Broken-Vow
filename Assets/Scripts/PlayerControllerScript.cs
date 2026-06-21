using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class PlayerControllerScript : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator myAnimator;
    private SpriteRenderer mySpriteRenderer;

    public static event Action OnInteractPressed;
    public static event Action OnClosePressed;
    public static event Action OnInventoryPressed;

    public static PlayerControllerScript Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            gameObject.SetActive(false);
            DestroyImmediate(gameObject);
            return;
        }
        
        // Ambil referensi komponen agar tidak null
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Hancurkan Player jika masuk ke scene MainMenu
        if (scene.name == "MainMenu")
        {
            Destroy(gameObject);
            return;
        }

        // Matikan simulasi fisika sementara agar tidak memicu transisi scene baru
        // di posisi lama sebelum di-teleport oleh SceneLoadManager
        if (rb != null)
        {
            rb.simulated = false;
            StartCoroutine(ReEnablePhysicsCoroutine());
        }
    }

    private IEnumerator ReEnablePhysicsCoroutine()
    {
        // Tunggu hingga akhir frame agar pemindahan posisi oleh SceneLoadManager selesai
        yield return new WaitForEndOfFrame();
        if (rb != null)
        {
            rb.simulated = true;
        }
    }

    private void OnEnable()
    {
        // PENGAMAN: Jika playerControls kosong, buat baru dulu sebelum di-Enable
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            
            // Daftarkan ulang callback agar input tidak macet setelah reset fisik
            playerControls.Movement.Move.performed += ctx => movement = ctx.ReadValue<Vector2>();
            playerControls.Movement.Move.canceled += ctx => movement = Vector2.zero;
            playerControls.Movement.Interact.performed += ctx => OnInteractPressed?.Invoke();
            playerControls.Movement.Close.performed += ctx => OnClosePressed?.Invoke();
            playerControls.Movement.Inventory.performed += ctx => OnInventoryPressed?.Invoke();
        }
        
        playerControls.Enable();
    }

    private void OnDisable()
    {
        // PENGAMAN: Hanya panggil Disable jika objeknya beneran ada (tidak null)
        if (playerControls != null)
        {
            playerControls.Disable();
        }
    }

    private void Update()
    {
        // Update parameter animasi setiap frame
        UpdateAnimationParameters();

        // Cek tombol Q untuk ganti equipment hanya jika input player sedang aktif
        if (playerControls != null && playerControls.Movement.enabled)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                UnityEngine.InputSystem.Keyboard.current.qKey.wasPressedThisFrame)
            {
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.CycleEquipment();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        AdjustPlayerFacingDirection();
        Move();
    }

    private void UpdateAnimationParameters()
    {
        if (myAnimator != null) 
        {
            myAnimator.SetFloat("moveX", movement.x);
            myAnimator.SetFloat("moveY", movement.y);
            
            // BARIS INI WAJIB ADA:
            myAnimator.SetFloat("speed", movement.sqrMagnitude); 
        }
    }

    private void Move()
    {
        // Menggunakan MovePosition untuk pergerakan Rigidbody2D yang halus
        rb.MovePosition(rb.position + movement * (speed * Time.fixedDeltaTime));
    }

    private bool isFacingLocked = false;
    private Coroutine lockFacingCoroutine;

    public void LockFacingDirection(bool faceLeft, float duration)
    {
        if (lockFacingCoroutine != null)
        {
            StopCoroutine(lockFacingCoroutine);
        }
        lockFacingCoroutine = StartCoroutine(LockFacingCoroutine(faceLeft, duration));
    }

    private IEnumerator LockFacingCoroutine(bool faceLeft, float duration)
    {
        transform.localScale = new Vector3(faceLeft ? -1f : 1f, 1f, 1f);
        isFacingLocked = true;
        yield return new WaitForSeconds(duration);
        isFacingLocked = false;
    }

    private void AdjustPlayerFacingDirection()
    {
        // Jika arah hadap sedang dikunci (saat memukul), biarkan tetap terkunci
        if (isFacingLocked) return;

        // Reset kembali ke skala normal (1) agar animasi jalan/idle dari Blend Tree tidak terbalik
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Dipanggil oleh CutsceneManager / DialogueManager untuk menghidupkan atau mematikan Input (bisa jalan & interaksi atau tidak).
    /// </summary>
    public void ToggleInput(bool enable)
    {
        if (playerControls == null) return;

        if (enable)
        {
            playerControls.Enable();
        }
        else
        {
            playerControls.Disable();
            StopMovement(); // Pastikan karakter tidak meluncur/sliding jika dinonaktifkan saat berjalan
        }
    }

    /// <summary>
    /// Menghentikan gaya gerak saat ini secara paksa.
    /// </summary>
    public void StopMovement()
    {
        movement = Vector2.zero;
        UpdateAnimationParameters(); // Paksa kecepatan animasi jadi 0 / Idle
    }
}