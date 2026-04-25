using System.Collections.Generic;
using UnityEngine;

public class bottomLayer : MonoBehaviour
{
    private const float GAP = 12f;

    private RectTransform rectTransform;
    private List<bottomLayerContainer> slots = new List<bottomLayerContainer>();

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Populate(int slotCount, GameObject slotPrefab, Sprite slotSprite)
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
            container.Setup(i + 1, slotSprite);
            slots.Add(container);
        }

        // Unlock first slot
        if (slots.Count > 0)
            slots[0].Unlock();
    }

    public void PopulateAllUnlocked(int slotCount, GameObject slotPrefab, Sprite slotSprite)
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
            container.Setup(i + 1, slotSprite);
            container.Unlock();
            slots.Add(container);
        }
    }

    public void UnlockNext(int currentIndex)
    {
        int nextIndex = currentIndex; // currentIndex is 1-based, slots list is 0-based
        if (nextIndex < slots.Count)
            slots[nextIndex].Unlock();
    }

    public List<bottomLayerContainer> Slots => slots;
}
