using UnityEngine;
using UnityEngine.InputSystem;
using System;

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

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        // Ambil referensi komponen agar tidak null
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        
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

    private void AdjustPlayerFacingDirection()
    {
        // Cara baru mengambil posisi mouse di Input System
        Vector3 mousePos = Mouse.current.position.ReadValue(); 
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);
        
        // if (mySpriteRenderer != null)
        // {
        //     mySpriteRenderer.flipX = mousePos.x < playerScreenPoint.x;
        // }
    }
}