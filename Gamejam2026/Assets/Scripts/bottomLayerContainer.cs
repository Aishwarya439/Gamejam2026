using UnityEngine;
using UnityEngine.UI;

public class bottomLayerContainer : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private GameObject slotImg;
    [SerializeField] private Image frameImage;

    private Image slotImgImage;
    private int slotIndex;
    private bool isUnlocked = false;
    private CanvasGroup canvasGroup;

    public void Setup(int index, Sprite bgSprite, Sprite frameSprite, float slotWidth = 0f, float slotHeight = 0f)
    {
        slotIndex = index;

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (slotImg != null)
        {
            slotImgImage = slotImg.GetComponent<Image>();
            slotImg.SetActive(false);
        }

        if (image != null && bgSprite != null)
        {
            image.sprite = bgSprite;
            image.preserveAspect = true;
        }

        if (frameImage != null && frameSprite != null)
        {
            frameImage.sprite = frameSprite;
            frameImage.preserveAspect = true;
            frameImage.color = Color.white;
        }

        SizeSlotImgToFrameHole(frameSprite, slotWidth, slotHeight);

        Lock();
    }

    private void SizeSlotImgToFrameHole(Sprite frameSprite, float rectW, float rectH)
    {
        if (slotImg == null || frameSprite == null) return;

        RectTransform slotImgRect = slotImg.GetComponent<RectTransform>();
        if (slotImgRect == null) return;

        if (rectW <= 0 || rectH <= 0)
        {
            RectTransform slotRect = GetComponent<RectTransform>();
            rectW = slotRect.rect.width;
            rectH = slotRect.rect.height;
            if (rectW <= 0 || rectH <= 0) return;
        }

        // Compute rendered size of frame sprite inside slot rect (preserveAspect letterboxing)
        float spritePixelW = frameSprite.rect.width;
        float spritePixelH = frameSprite.rect.height;
        float spriteAspect = spritePixelW / spritePixelH;
        float rectAspect = rectW / rectH;

        float renderedW, renderedH;
        if (spriteAspect > rectAspect)
        {
            renderedW = rectW;
            renderedH = rectW / spriteAspect;
        }
        else
        {
            renderedH = rectH;
            renderedW = rectH * spriteAspect;
        }

        // Inner hole size = rendered frame size * hole ratio (measured from frame pixels)
        // hole is 182/258 wide and 182/246 tall in the source texture
        float holeW = renderedW * (frameSprite.rect.width > 0 ? MeasureHoleRatio(frameSprite, horizontal: true) : 0.705f);
        float holeH = renderedH * (frameSprite.rect.height > 0 ? MeasureHoleRatio(frameSprite, horizontal: false) : 0.740f);

        slotImgRect.anchorMin = new Vector2(0.5f, 0.5f);
        slotImgRect.anchorMax = new Vector2(0.5f, 0.5f);
        slotImgRect.pivot = new Vector2(0.5f, 0.5f);
        slotImgRect.anchoredPosition = Vector2.zero;
        slotImgRect.sizeDelta = new Vector2(holeW, holeH);
    }

    private float MeasureHoleRatio(Sprite sprite, bool horizontal)
    {
        Texture2D tex = sprite.texture;
        if (!tex.isReadable)
            return horizontal ? 0.705f : 0.740f;

        int texW = tex.width;
        int texH = tex.height;
        int cx = texW / 2;
        int cy = texH / 2;
        int maxR = horizontal ? Mathf.Min(cx, cy) : Mathf.Min(cx, cy);

        for (int r = 0; r < maxR; r++)
        {
            float a = horizontal
                ? Mathf.Max(tex.GetPixel(cx - r, cy).a, tex.GetPixel(cx + r, cy).a)
                : Mathf.Max(tex.GetPixel(cx, cy - r).a, tex.GetPixel(cx, cy + r).a);

            if (a > 0.5f)
            {
                float holeDiameter = r * 2f;
                return horizontal ? holeDiameter / texW : holeDiameter / texH;
            }
        }

        return horizontal ? 0.705f : 0.740f;
    }

    public int SlotIndex => slotIndex;
    public bool IsUnlocked => isUnlocked;

    public void Unlock()
    {
        isUnlocked = true;
        image.color = Color.white;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void Lock()
    {
        isUnlocked = false;
        image.color = Color.white;
        canvasGroup.alpha = 0.4f;
        canvasGroup.blocksRaycasts = false;
    }

    public void SetItem(Sprite itemSprite)
    {
        if (slotImg != null && itemSprite != null)
        {
            if (slotImgImage == null) slotImgImage = slotImg.GetComponent<Image>();
            if (slotImgImage != null)
            {
                slotImgImage.sprite = itemSprite;
                slotImgImage.color = Color.white;
                slotImgImage.preserveAspect = false;
            }
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
