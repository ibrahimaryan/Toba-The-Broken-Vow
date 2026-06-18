using UnityEngine;
using UnityEngine.UI;

public class PortraitSlot : MonoBehaviour
{
    public Image portraitImage;
    public VNPortraitPosition slotPosition;

    private void Awake()
    {
        if (portraitImage == null)
            portraitImage = GetComponent<Image>();
    }

    public void SetPortrait(Sprite sprite)
    {
        if (sprite == null)
        {
            Debug.Log($"[PortraitSlot] {gameObject.name}: GAGAL! Sprite kosong/null. Membersihkan wadah.");
            Clear();
            return;
        }
        
        portraitImage.sprite = sprite;
        portraitImage.color = new Color(1, 1, 1, 1);
        gameObject.SetActive(true); // Otomatis nyalakan wadah
        
        RectTransform rect = GetComponent<RectTransform>();
        Debug.Log($"[PortraitSlot] {gameObject.name}: BERHASIL dipasang gambar '{sprite.name}'. Posisi X: {rect.anchoredPosition.x}, Y: {rect.anchoredPosition.y}, Apakah wadah aktif? {gameObject.activeInHierarchy}");
    }

    public void SetDimmed(bool isDimmed)
    {
        if (portraitImage.sprite == null) return;
        
        Color c = portraitImage.color;
        c.a = isDimmed ? 0.4f : 1.0f;
        portraitImage.color = c;
    }
    
    public void Clear()
    {
        portraitImage.sprite = null;
        portraitImage.color = new Color(1, 1, 1, 0); // Hide image
        gameObject.SetActive(false); // Otomatis sembunyikan wadah
    }
}
