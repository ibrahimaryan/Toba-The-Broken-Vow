using UnityEngine;
using UnityEngine.UI;

public class CutscenePortraitManager : MonoBehaviour
{
    [System.Serializable]
    public class PortraitSlot
    {
        public string slotName;
        public Image portraitImage;

        [HideInInspector]
        public string currentCharacter;
    }

    public PortraitSlot[] slots;

    public Color activeColor = Color.white;
    public Color inactiveColor =
        new Color(0.45f, 0.45f, 0.45f, 1f);

    public void ShowPortrait(
        string slotName,
        string characterName,
        Sprite sprite)
    {
        foreach (PortraitSlot slot in slots)
        {
            if (slot.slotName != slotName)
                continue;

            slot.currentCharacter = characterName;
            slot.portraitImage.sprite = sprite;
            slot.portraitImage.gameObject.SetActive(true);

            return;
        }
    }

    public void SetSpeaker(string characterName)
    {
        foreach (PortraitSlot slot in slots)
        {
            if (!slot.portraitImage.gameObject.activeSelf)
                continue;

            slot.portraitImage.color =
                slot.currentCharacter == characterName
                ? activeColor
                : inactiveColor;
        }
    }

    public void HideCharacter(string characterName)
    {
        foreach (PortraitSlot slot in slots)
        {
            if (slot.currentCharacter == characterName)
            {
                slot.portraitImage.gameObject.SetActive(false);
            }
        }
    }

    public void HideAll()
    {
        foreach (PortraitSlot slot in slots)
        {
            slot.portraitImage.gameObject.SetActive(false);
        }
    }
}