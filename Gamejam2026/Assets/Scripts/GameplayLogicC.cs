using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayLogicC : IGameplayLogic
{
    private GameSceneController controller;
    private int configIndex;
    private int totalComponents;
    private readonly int slotCount;

    private bottomLayer bottomLayerComponent;
    private Dictionary<int, GameObject> dummyLayers = new Dictionary<int, GameObject>();

    public GameplayLogicC(int slotCount)
    {
        this.slotCount = slotCount;
    }

    public void Init(GameSceneController controller, int configIndex, int totalComponents)
    {
        this.controller = controller;
        this.configIndex = configIndex;
        this.totalComponents = totalComponents;

        SetupBottomLayer(controller);
        SetupDummyLayers(controller);
    }

    private void SetupBottomLayer(GameSceneController gsc)
    {
        GameObject bottomLayerGo = GameObject.Find("bottomLayer");
        if (bottomLayerGo == null)
        {
            Debug.LogError("[GameplayLogicC] bottomLayer not found in scene!");
            return;
        }

        bottomLayerComponent = bottomLayerGo.GetComponent<bottomLayer>();
        Sprite slotSprite = Resources.Load<Sprite>("Art/slot");
        bottomLayerComponent.Populate(slotCount, controller.BottomLayerContainerPrefab, slotSprite);
    }

    private void SetupDummyLayers(GameSceneController gsc)
    {
        GameObject mainBgGo = GameObject.Find("mainBg");
        if (mainBgGo == null) return;

        string startPath = $"Art/level_{configIndex + 1}/start";
        Sprite startSprite = Resources.Load<Sprite>(startPath);
        Image mainBgImage = mainBgGo.GetComponent<Image>();
        if (mainBgImage != null && startSprite != null)
            mainBgImage.sprite = startSprite;
        else
            Debug.LogWarning($"[GameplayLogicC] start sprite not found at: Resources/{startPath}");

        GameObject prefab = gsc.DummyMainBgPrefab;
        if (prefab == null)
        {
            Debug.LogError("[GameplayLogicC] dummyMainBgPrefab not assigned!");
            return;
        }

        // Register the existing scene dummyMainBg as layer 1
        GameObject existingDummy = GameObject.Find("dummyMainBg");
        if (existingDummy != null)
        {
            string path1 = $"Art/level_{configIndex + 1}/steps/step_1";
            Sprite sprite1 = Resources.Load<Sprite>(path1);
            Image img1 = existingDummy.GetComponent<Image>();
            if (img1 != null && sprite1 != null)
                img1.sprite = sprite1;
            dummyLayers[1] = existingDummy;
        }

        Canvas.ForceUpdateCanvases();

        for (int i = totalComponents; i >= 2; i--)
        {
            GameObject dummy = GameObject.Instantiate(prefab, mainBgGo.transform);
            dummy.name = $"dummyMainBg_{i}";

            // Stretch to fill mainBg fully
            RectTransform rect = dummy.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            string path = $"Art/level_{configIndex + 1}/steps/step_{i}";
            Sprite sprite = Resources.Load<Sprite>(path);
            Image img = dummy.GetComponent<Image>();
            if (img != null && sprite != null)
                img.sprite = sprite;
            else
                Debug.LogWarning($"[GameplayLogicC] Sprite not found at: Resources/{path}");

            dummyLayers[i] = dummy;
        }
    }

    public bool OnDrop(int index, GameObject droppedOn, DraggableItem draggable)
    {
        if (droppedOn == null) return false;

        bottomLayerContainer slot = droppedOn.GetComponent<bottomLayerContainer>();
        if (slot == null || !slot.IsUnlocked) return false;
        if (slot.SlotIndex != index) return false;

        string path = $"Art/level_{configIndex + 1}/components/step_{index}";
        Sprite itemSprite = Resources.Load<Sprite>(path);
        if (itemSprite != null)
            slot.SetItem(itemSprite);
        else
            Debug.LogWarning($"[GameplayLogicC] Sprite not found at: Resources/{path}");

        // Remove the corresponding dummy layer
        if (dummyLayers.TryGetValue(index, out GameObject dummy))
        {
            GameObject.Destroy(dummy);
            dummyLayers.Remove(index);
        }

        bottomLayerComponent.UnlockNext(index);

        if (index >= totalComponents)
            OnEnd();

        return true;
    }

    public void OnEnd()
    {
        controller.NotifyLevelComplete();
    }
}
