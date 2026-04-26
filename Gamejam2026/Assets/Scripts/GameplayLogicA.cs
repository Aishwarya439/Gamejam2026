using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameplayLogicA : IGameplayLogic
{
    private GameSceneController controller;
    private int configIndex;
    private int assetId;
    private int totalComponents;
    private readonly int slotCount;

    private bottomLayer bottomLayerComponent;
    private Dictionary<int, Image> stepLayers = new Dictionary<int, Image>();

    public GameplayLogicA(int slotCount)
    {
        this.slotCount = slotCount;
    }

    public void Init(GameSceneController controller, int configIndex, int assetId, int totalComponents)
    {
        this.controller = controller;
        this.configIndex = configIndex;
        this.assetId = assetId;
        this.totalComponents = totalComponents;

        SetupBottomLayer();
        SetupDummyLayers();
    }

    private void SetupBottomLayer()
    {
        GameObject bottomLayerGo = GameObject.Find("bottomLayer");
        if (bottomLayerGo == null)
        {
            Debug.LogError("[GameplayLogicA] bottomLayer not found in scene!");
            return;
        }

        bottomLayerComponent = bottomLayerGo.GetComponent<bottomLayer>();
        Sprite bgSprite = Resources.Load<Sprite>("Art/blc_1_bg");
        Sprite frameSprite = Resources.Load<Sprite>("Art/blc_1_frame");
        bottomLayerComponent.Populate(slotCount, controller.BottomLayerContainerPrefab, bgSprite, frameSprite);
    }

    private void SetupDummyLayers()
    {
        GameObject mainBgGo = GameObject.Find("mainBg");
        if (mainBgGo == null) return;

        // Set mainBg image to start.png (bottom-most layer, never removed)
        string startPath = $"Art/level_{assetId}/start";
        Sprite startSprite = Resources.Load<Sprite>(startPath);
        Image mainBgImage = mainBgGo.GetComponent<Image>();
        if (mainBgImage != null && startSprite != null)
        {
            mainBgImage.sprite = startSprite;
            mainBgImage.color = Color.white;
            mainBgImage.preserveAspect = true;
        }
        else
            Debug.LogWarning($"[GameplayLogicA] start sprite not found at: Resources/{startPath}");

        GameObject prefab = controller.DummyMainBgPrefab;
        if (prefab == null)
        {
            Debug.LogError("[GameplayLogicA] dummyMainBgPrefab not assigned!");
            return;
        }

        // The existing dummyMainBg in the scene counts as one slot — use it for step_N (bottom of stack)
        // Then instantiate step_(N-1) ... step_1 on top, so step_1 is the topmost (last sibling)
        // Render order under mainBg: start(mainBg) -> step_N -> step_(N-1) -> ... -> step_1

        Canvas.ForceUpdateCanvases();

        GameObject existingDummy = GameObject.Find("dummyMainBg");

        // Assign step_N to the existing dummy (it will be the lowest step layer)
        if (existingDummy != null)
        {
            string path = $"Art/level_{assetId}/steps/step_{totalComponents}";
            Sprite sprite = Resources.Load<Sprite>(path);
            Image img = existingDummy.GetComponent<Image>();
            if (img != null && sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
            }
            else
                Debug.LogWarning($"[GameplayLogicA] Sprite not found at: Resources/{path}");

            existingDummy.transform.SetAsLastSibling();
            stepLayers[totalComponents] = img;
        }

        // Instantiate step_(N-1) down to step_1, each added as last sibling so step_1 ends up on top
        for (int i = totalComponents - 1; i >= 1; i--)
        {
            GameObject dummy = GameObject.Instantiate(prefab, mainBgGo.transform);
            dummy.name = $"dummyMainBg_{i}";

            RectTransform rect = dummy.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            string path = $"Art/level_{assetId}/steps/step_{i}";
            Sprite sprite = Resources.Load<Sprite>(path);
            Image img = dummy.GetComponent<Image>();
            if (img != null && sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
            }
            else
                Debug.LogWarning($"[GameplayLogicA] Sprite not found at: Resources/{path}");

            stepLayers[i] = img;
        }
    }

    public bool OnDrop(int index, GameObject droppedOn, DraggableItem draggable)
    {
        if (droppedOn == null) return false;

        bottomLayerContainer slot = droppedOn.GetComponent<bottomLayerContainer>()
            ?? droppedOn.GetComponentInParent<bottomLayerContainer>();
        if (slot == null || !slot.IsUnlocked) return false;
        if (slot.SlotIndex != index) return false;

        string path = $"Art/level_{assetId}/components/step_{index}";
        Sprite itemSprite = Resources.Load<Sprite>(path);
        if (itemSprite != null)
            slot.SetItem(itemSprite);
        else
            Debug.LogWarning($"[GameplayLogicA] Sprite not found at: Resources/{path}");

        // Fade out the corresponding step layer
        if (stepLayers.TryGetValue(index, out Image layerImage) && layerImage != null)
        {
            layerImage.DOFade(0f, 0.5f).OnComplete(() =>
            {
                if (layerImage != null)
                    GameObject.Destroy(layerImage.gameObject);
            });
            stepLayers.Remove(index);
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
