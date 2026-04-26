using System.Collections.Generic;
using UnityEngine;

public class GameplayLogicD : IGameplayLogic
{
    private GameSceneController controller;
    private int configIndex;
    private int assetId;
    private int totalComponents;
    private readonly int slotCount;

    private bottomLayer bottomLayerComponent;
    private UnityEngine.UI.Image mainBgImage;

    // slot index → (item index, draggable reference)
    private Dictionary<int, (int itemIndex, DraggableItem draggable)> placedItems
        = new Dictionary<int, (int, DraggableItem)>();

    public GameplayLogicD(int slotCount)
    {
        this.slotCount = slotCount;
    }

    public void Init(GameSceneController controller, int configIndex, int assetId, int totalComponents)
    {
        this.controller = controller;
        this.configIndex = configIndex;
        this.assetId = assetId;
        this.totalComponents = totalComponents;

        GameObject bottomLayerGo = GameObject.Find("bottomLayer");
        if (bottomLayerGo == null)
        {
            Debug.LogError("[GameplayLogicD] bottomLayer not found!");
            return;
        }

        bottomLayerComponent = bottomLayerGo.GetComponent<bottomLayer>();
        Sprite bgSprite = Resources.Load<Sprite>("Art/blc_4_bg");
        Sprite frameSprite = Resources.Load<Sprite>("Art/blc_4_frame");
        bottomLayerComponent.PopulateAllUnlocked(slotCount, controller.BottomLayerContainerPrefab, bgSprite, frameSprite);

        GameObject mainBgGo = GameObject.Find("mainBg");
        if (mainBgGo != null)
            mainBgImage = mainBgGo.GetComponent<UnityEngine.UI.Image>();
    }

    public bool OnDrop(int index, GameObject droppedOn, DraggableItem draggable)
    {
        if (droppedOn == null) return false;

        bottomLayerContainer slot = droppedOn.GetComponent<bottomLayerContainer>()
            ?? droppedOn.GetComponentInParent<bottomLayerContainer>();
        if (slot == null || !slot.IsUnlocked || slot.IsOccupied) return false;

        string path = $"Art/level_{assetId}/components/step_{index}";
        Sprite itemSprite = Resources.Load<Sprite>(path);
        slot.SetItem(itemSprite);

        placedItems[slot.SlotIndex] = (index, draggable);

        if (placedItems.Count >= slotCount)
            Evaluate();

        return true;
    }

    private void Evaluate()
    {
        int correctUntil = 0;
        for (int i = 1; i <= slotCount; i++)
        {
            if (placedItems.TryGetValue(i, out var entry) && entry.itemIndex == i)
                correctUntil = i;
            else
                break;
        }

        if (correctUntil >= slotCount)
        {
            OnEnd();
            return;
        }

        // Show incorrect feedback
        Sprite incorrectSprite = Resources.Load<Sprite>($"Art/level_{assetId}/incorrectFeedback");
        if (incorrectSprite != null && mainBgImage != null)
            mainBgImage.sprite = incorrectSprite;

        List<int> toRevert = new List<int>();
        foreach (var kvp in placedItems)
        {
            if (kvp.Key > correctUntil)
                toRevert.Add(kvp.Key);
        }

        controller.ShakeAndReset(() =>
        {
            // Revert mainBg to start
            Sprite startSprite = Resources.Load<Sprite>($"Art/level_{assetId}/start");
            if (startSprite != null && mainBgImage != null)
                mainBgImage.sprite = startSprite;

            HashSet<componentContainer> containersToRefresh = new HashSet<componentContainer>();

            foreach (int slotIdx in toRevert)
            {
                DraggableItem draggable = placedItems[slotIdx].draggable;
                placedItems.Remove(slotIdx);

                bottomLayerContainer slot = bottomLayerComponent.Slots[slotIdx - 1];
                slot.ClearItem();

                draggable.Restore();

                componentContainer container = draggable.OriginalParent?.GetComponent<componentContainer>();
                if (container != null)
                    containersToRefresh.Add(container);
            }

            foreach (componentContainer container in containersToRefresh)
                container.RefreshLayout();
        });
    }

    public void OnEnd()
    {
        Sprite correctSprite = Resources.Load<Sprite>($"Art/level_{assetId}/correctFeedback");
        if (correctSprite != null && mainBgImage != null)
            mainBgImage.sprite = correctSprite;

        controller.NotifyLevelComplete();
    }
}
