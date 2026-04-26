using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bottomLayer : MonoBehaviour
{
    private const float GAP = 12f;

    private RectTransform rectTransform;
    private List<bottomLayerContainer> slots = new List<bottomLayerContainer>();

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Populate(int slotCount, GameObject slotPrefab, Sprite bgSprite, Sprite frameSprite)
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        slots.Clear();

        if (slotCount <= 0) return;

        Canvas.ForceUpdateCanvases();

        float containerWidth = rectTransform.rect.width;
        float containerHeight = rectTransform.rect.height;
        float slotWidth = (containerWidth - (slotCount + 1) * GAP) / slotCount;
        float slotHeight = containerHeight - 2 * GAP;
        float startX = -containerWidth / 2f + GAP + slotWidth / 2f;

        for (int i = 0; i < slotCount; i++)
        {
            GameObject instance = Instantiate(slotPrefab, transform);
            instance.name = $"Slot_{i + 1}";

            RectTransform rect = instance.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(slotWidth, slotHeight);
            rect.anchoredPosition = new Vector2(startX + i * (slotWidth + GAP), 0f);

            bottomLayerContainer container = instance.GetComponent<bottomLayerContainer>();
            container.Setup(i + 1, bgSprite, frameSprite, slotWidth, slotHeight);
            slots.Add(container);
        }

        // Unlock first slot
        if (slots.Count > 0)
            slots[0].Unlock();
    }

    public void PopulateAllUnlocked(int slotCount, GameObject slotPrefab, Sprite bgSprite, Sprite frameSprite)
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        slots.Clear();

        if (slotCount <= 0) return;

        Canvas.ForceUpdateCanvases();

        float containerWidth = rectTransform.rect.width;
        float containerHeight = rectTransform.rect.height;
        float slotWidth = (containerWidth - (slotCount + 1) * GAP) / slotCount;
        float slotHeight = containerHeight - 2 * GAP;
        float startX = -containerWidth / 2f + GAP + slotWidth / 2f;

        // Full-width background line — spawned first so slots render on top
        GameObject fullLine = new GameObject("TimelineLine", typeof(RectTransform), typeof(Image));
        fullLine.transform.SetParent(transform, false);
        RectTransform fullLineRect = fullLine.GetComponent<RectTransform>();
        fullLineRect.anchorMin = new Vector2(0.5f, 0.5f);
        fullLineRect.anchorMax = new Vector2(0.5f, 0.5f);
        fullLineRect.pivot = new Vector2(0.5f, 0.5f);
        fullLineRect.sizeDelta = new Vector2(containerWidth, 4f);
        fullLineRect.anchoredPosition = Vector2.zero;
        fullLine.GetComponent<Image>().color = Color.black;

        for (int i = 0; i < slotCount; i++)
        {
            GameObject instance = Instantiate(slotPrefab, transform);
            instance.name = $"Slot_{i + 1}";

            RectTransform rect = instance.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(slotWidth, slotHeight);
            rect.anchoredPosition = new Vector2(startX + i * (slotWidth + GAP), 0f);

            bottomLayerContainer container = instance.GetComponent<bottomLayerContainer>();
            container.Setup(i + 1, bgSprite, frameSprite, slotWidth, slotHeight);
            container.Unlock();
            slots.Add(container);
        }

        // Calculate actual rendered sprite width inside slot rect (preserveAspect shrinks it)
        float spriteRenderedWidth = slotWidth;
        if (bgSprite != null)
        {
            float spriteAspect = bgSprite.rect.width / bgSprite.rect.height;
            float rectAspect = slotWidth / slotHeight;
            if (spriteAspect < rectAspect)
                spriteRenderedWidth = slotHeight * spriteAspect;
        }

        // Spawn random dots in each gap (including before first and after last slot)
        for (int i = 0; i <= slotCount; i++)
        {
            float gapLeft = i == 0
                ? -containerWidth / 2f
                : startX + (i - 1) * (slotWidth + GAP) + spriteRenderedWidth / 2f;
            float gapRight = i == slotCount
                ? containerWidth / 2f
                : startX + i * (slotWidth + GAP) - spriteRenderedWidth / 2f;

            float gapWidth = gapRight - gapLeft;
            if (gapWidth <= 0) continue;

            int dotCount = Random.Range(1, 3);
            for (int d = 0; d < dotCount; d++)
            {
                float dotX = Random.Range(gapLeft + 6f, gapRight - 6f);

                GameObject dot = new GameObject($"Dot_{i}_{d}", typeof(RectTransform), typeof(Image));
                dot.transform.SetParent(transform, false);

                RectTransform dotRect = dot.GetComponent<RectTransform>();
                dotRect.anchorMin = new Vector2(0.5f, 0.5f);
                dotRect.anchorMax = new Vector2(0.5f, 0.5f);
                dotRect.pivot = new Vector2(0.5f, 0.5f);
                dotRect.sizeDelta = new Vector2(8f, 8f);
                dotRect.anchoredPosition = new Vector2(dotX, 0f);

                dot.GetComponent<Image>().color = Color.black;
            }
        }
    }

    public void UnlockNext(int currentIndex)
    {
        int nextIndex = currentIndex; // currentIndex is 1-based, slots list is 0-based
        if (nextIndex < slots.Count)
            slots[nextIndex].Unlock();
    }

    public void ShowInfoOnly(Sprite infoSprite)
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        slots.Clear();

        Image bg = GetComponent<Image>();
        if (bg == null) bg = gameObject.AddComponent<Image>();
        bg.sprite = infoSprite;
        bg.preserveAspect = true;
        bg.color = Color.white;

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public List<bottomLayerContainer> Slots => slots;
}
