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
            Clear();
            return;
        }
        
        portraitImage.sprite = sprite;
        portraitImage.color = new Color(1, 1, 1, 1);
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
    }
}
