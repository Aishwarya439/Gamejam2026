using UnityEngine;
using UnityEngine.UI;

public class bottomLayerContainer : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private GameObject slotImg;

    private int slotIndex;
    private bool isUnlocked = false;
    private CanvasGroup canvasGroup;

    public void Setup(int index, Sprite slotSprite)
    {
        slotIndex = index;

        if (image == null)
            image = GetComponent<Image>();

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (slotImg == null)
            slotImg = transform.Find("slotImg")?.gameObject;

        if (slotImg != null)
            slotImg.SetActive(false);

        if (image != null && slotSprite != null)
            image.sprite = slotSprite;

        Lock();
    }

    public int SlotIndex => slotIndex;
    public bool IsUnlocked => isUnlocked;

    public void Unlock()
    {
        isUnlocked = true;
        image.color = Color.white;
        canvasGroup.blocksRaycasts = true;
    }

    public void Lock()
    {
        isUnlocked = false;
        image.color = new Color(0.4f, 0.4f, 0.4f, 1f);
        canvasGroup.blocksRaycasts = false;
    }

    public void SetItem(Sprite itemSprite)
    {
        if (slotImg != null)
        {
            Image slotImgImage = slotImg.GetComponent<Image>();
            if (slotImgImage != null && itemSprite != null)
                slotImgImage.sprite = itemSprite;
            slotImg.SetActive(true);
        }

        canvasGroup.blocksRaycasts = false;
    }

    public void ClearItem()
    {
        if (slotImg != null)
            slotImg.SetActive(false);
        canvasGroup.blocksRaycasts = true;
    }

    public bool IsOccupied => slotImg != null && slotImg.activeSelf;
}
