using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PipeType
{
    Straight, // Lurus (Horizontal/Vertikal)
    LBend,    // Belok L (Siku-siku)
    TBend,    // Pertigaan T (T-junction)
    Cross     // Salib 4 arah
}

public class BambooPipeTile : MonoBehaviour
{
    [Header("Pipe Configurations")]
    public PipeType pipeType;
    public bool isLocked = false;
    [Range(0, 3)] public int currentRotationIndex = 0;

    [Header("Grid Position")]
    public int gridX;
    public int gridY;

    [Header("Visual Components")]
    [SerializeField] private Image pipeImage;      // Image utama untuk pipa bambu
    [SerializeField] private Image glowImage;      // Image penunjuk blinking biru tipis
    
    [Header("Color Settings")]
    [SerializeField] private Color dryColor = Color.white;
    [SerializeField] private Color wetColor = new Color(0.3f, 0.8f, 1f, 1f); // Warna kebiruan saat dialiri air

    [Header("Blink Settings")]
    [SerializeField] private float blinkSpeed = 3f;
    [SerializeField] private float minAlpha = 0.1f;
    [SerializeField] private float maxAlpha = 0.7f;

    private Button buttonComponent;
    private BambooPuzzleManager manager;
    private bool isFilled = false;

    // Arah koneksi dasar pada rotasi 0 derajat:
    // 0 = Top, 1 = Right, 2 = Bottom, 3 = Left
    private static readonly Dictionary<PipeType, List<int>> BaseConnections = new Dictionary<PipeType, List<int>>()
    {
        { PipeType.Straight, new List<int> { 0, 2 } },       // Atas dan Bawah (Vertikal secara default)
        { PipeType.LBend, new List<int> { 0, 1 } },          // Atas dan Kanan
        { PipeType.TBend, new List<int> { 0, 1, 2 } },       // Atas, Kanan, Bawah (Pertigaan T default cabang kanan)
        { PipeType.Cross, new List<int> { 0, 1, 2, 3 } }     // Keempat arah
    };

    private void Awake()
    {
        buttonComponent = GetComponent<Button>();
        manager = GetComponentInParent<BambooPuzzleManager>();

        // FAIL-SAFE: Jika pipeImage belum ditarik di Inspector, ambil Image dari GameObject ini
        if (pipeImage == null)
        {
            pipeImage = GetComponent<Image>();
        }

        // Sinkronisasi currentRotationIndex dari Z rotation di Unity Editor jika diputar manual
        float zRot = transform.localEulerAngles.z;
        int calculatedIndex = Mathf.RoundToInt((360f - zRot) / 90f) % 4;
        if (calculatedIndex < 0) calculatedIndex += 4;
        currentRotationIndex = calculatedIndex;
    }

    private void Start()
    {
        if (buttonComponent != null)
        {
            if (isLocked)
            {
                buttonComponent.interactable = false;
            }
            else
            {
                buttonComponent.onClick.AddListener(OnClickTile);
            }
        }

        if (glowImage != null)
        {
            glowImage.gameObject.SetActive(!isLocked);
        }

        UpdateVisuals();
    }

    private void Update()
    {
        // Efek kedip biru lembut jika pipa tidak terkunci
        if (!isLocked && glowImage != null && glowImage.gameObject.activeSelf)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f);
            Color c = glowImage.color;
            c.a = alpha;
            glowImage.color = c;
        }
    }

    private void OnClickTile()
    {
        if (manager != null)
        {
            manager.OnPipeClicked(this);
        }
    }

    // Memutar pipa secara visual dan logis sebesar 90 derajat searah jarum jam (clockwise)
    public void RotatePipe()
    {
        if (isLocked) return;

        currentRotationIndex = (currentRotationIndex + 1) % 4;
        UpdateVisuals();
    }

    // Mengacak rotasi pipa (hanya jika tidak dikunci)
    public void Shuffle()
    {
        if (isLocked) return;

        currentRotationIndex = Random.Range(0, 4);
        UpdateVisuals();
    }

    // Dipanggil otomatis oleh Unity saat nilai di Inspector diubah (Design-time helper)
    private void OnValidate()
    {
        // Update rotasi visual secara instan di editor
        transform.localRotation = Quaternion.Euler(0, 0, -currentRotationIndex * 90f);
    }

    // Memperbarui visual rotasi dan warna aliran air
    public void UpdateVisuals()
    {
        // Putar visual Image (searah jarum jam -> rotasi negatif pada sumbu Z)
        transform.localRotation = Quaternion.Euler(0, 0, -currentRotationIndex * 90f);

        // Update warna berdasarkan status air mengalir
        if (pipeImage != null)
        {
            pipeImage.color = isFilled ? wetColor : dryColor;
        }
    }

    // Mengatur status aliran air
    public void SetFilled(bool filled)
    {
        if (isFilled != filled)
        {
            isFilled = filled;
            UpdateVisuals();
        }
    }

    public bool IsFilled()
    {
        return isFilled;
    }

    // Mendapatkan semua arah koneksi aktif saat ini setelah diputar
    public HashSet<int> GetActiveConnections()
    {
        HashSet<int> activeConnections = new HashSet<int>();
        if (BaseConnections.TryGetValue(pipeType, out var baseDirs))
        {
            foreach (int dir in baseDirs)
            {
                // Putar arah searah jarum jam: (arah dasar + jumlah putaran) % 4
                int rotatedDir = (dir + currentRotationIndex) % 4;
                activeConnections.Add(rotatedDir);
            }
        }
        return activeConnections;
    }
}
