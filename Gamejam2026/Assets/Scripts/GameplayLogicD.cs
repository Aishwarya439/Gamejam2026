using System.Collections.Generic;
using UnityEngine;

public class GameplayLogicD : IGameplayLogic
{
    private GameSceneController controller;
    private int configIndex;
    private int totalComponents;
    private readonly int slotCount;

    private bottomLayer bottomLayerComponent;

    // slot index → (item index, draggable reference)
    private Dictionary<int, (int itemIndex, DraggableItem draggable)> placedItems
        = new Dictionary<int, (int, DraggableItem)>();

    public GameplayLogicD(int slotCount)
    {
        this.slotCount = slotCount;
    }

    public void Init(GameSceneController controller, int configIndex, int totalComponents)
    {
        this.controller = controller;
        this.configIndex = configIndex;
        this.totalComponents = totalComponents;

        GameObject bottomLayerGo = GameObject.Find("bottomLayer");
        if (bottomLayerGo == null)
        {
            Debug.LogError("[GameplayLogicD] bottomLayer not found!");
            return;
        }

        bottomLayerComponent = bottomLayerGo.GetComponent<bottomLayer>();
        Sprite slotSprite = Resources.Load<Sprite>("Art/slot");
        bottomLayerComponent.PopulateAllUnlocked(slotCount, controller.BottomLayerContainerPrefab, slotSprite);
    }

    public bool OnDrop(int index, GameObject droppedOn, DraggableItem draggable)
    {
        if (droppedOn == null) return false;

        bottomLayerContainer slot = droppedOn.GetComponent<bottomLayerContainer>();
        if (slot == null || !slot.IsUnlocked || slot.IsOccupied) return false;

        string path = $"Art/level_{configIndex + 1}/components/step_{index}";
        Sprite itemSprite = Resources.Load<Sprite>(path);
        slot.SetItem(itemSprite);

        placedItems[slot.SlotIndex] = (index, draggable);

        // Evaluate once all slots are filled
        if (placedItems.Count >= slotCount)
            Evaluate();

        return true;
    }

    private void Evaluate()
    {
        // Find the last correct consecutive index from 1
        int correctUntil = 0;
        for (int i = 1; i <= slotCount; i++)
        {
            if (placedItems.TryGetValue(i, out var entry) && entry.itemIndex == i)
                correctUntil = i;
            else
                break;
        }

        // Revert everything after correctUntil
        List<int> toRevert = new List<int>();
        foreach (var kvp in placedItems)
        {
            if (kvp.Key > correctUntil)
                toRevert.Add(kvp.Key);
        }

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

        if (correctUntil >= slotCount)
            OnEnd();
    }

    public void OnEnd()
    {
        controller.NotifyLevelComplete();
    }
}
