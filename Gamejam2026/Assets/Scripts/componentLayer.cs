using System.Collections.Generic;
using UnityEngine;

public class componentContainer : MonoBehaviour
{
    private const float GAP = 16f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Populate(int[] indices, int configIndex, GameSceneController controller, GameObject prefab)
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        if (indices == null || indices.Length == 0) return;

        List<RectTransform> spawnedItems = new List<RectTransform>();

        foreach (int index in indices)
        {
            GameObject instance = Instantiate(prefab, transform);
            instance.name = $"Item_{index}";
            instance.GetComponent<componentLayerContainer>().Setup(index, configIndex, controller);
            spawnedItems.Add(instance.GetComponent<RectTransform>());
        }

        Canvas.ForceUpdateCanvases();
        LayoutItems(spawnedItems);

        // Update originalPosition on each DraggableItem after layout
        foreach (var rt in spawnedItems)
        {
            DraggableItem d = rt.GetComponent<DraggableItem>();
            if (d != null) d.RefreshOriginalPosition();
        }
    }

    public void RefreshLayout()
    {
        Canvas.ForceUpdateCanvases();

        List<RectTransform> activeItems = new List<RectTransform>();
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
                activeItems.Add(child.GetComponent<RectTransform>());
        }

        LayoutItems(activeItems);

        foreach (var rt in activeItems)
        {
            DraggableItem d = rt.GetComponent<DraggableItem>();
            if (d != null) d.RefreshOriginalPosition();
        }
    }

    private void LayoutItems(List<RectTransform> items)
    {
        if (items == null || items.Count == 0) return;

        float containerWidth = rectTransform.rect.width;
        float containerHeight = rectTransform.rect.height;
        int count = items.Count;

        float itemWidth = containerWidth * 0.9f;
        float itemHeight = (containerHeight - (count + 1) * GAP) / count;

        float startY = containerHeight / 2f - GAP - itemHeight / 2f;

        for (int i = 0; i < count; i++)
        {
            items[i].anchorMin = new Vector2(0.5f, 0.5f);
            items[i].anchorMax = new Vector2(0.5f, 0.5f);
            items[i].pivot = new Vector2(0.5f, 0.5f);
            items[i].sizeDelta = new Vector2(itemWidth, itemHeight);
            items[i].anchoredPosition = new Vector2(0f, startY - i * (itemHeight + GAP));
        }
    }
}
