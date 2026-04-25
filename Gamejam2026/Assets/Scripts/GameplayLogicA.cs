using UnityEngine;
using UnityEngine.UI;

public class GameplayLogicA : IGameplayLogic
{
    private GameSceneController controller;
    private int configIndex;
    private int totalComponents;
    private int nextExpectedIndex = 1;

    private GameObject mainBg;
    private Image mainBgImage;

    public void Init(GameSceneController controller, int configIndex, int totalComponents)
    {
        this.controller = controller;
        this.configIndex = configIndex;
        this.totalComponents = totalComponents;

        mainBg = GameObject.Find("mainBg");
        if (mainBg != null)
            mainBgImage = mainBg.GetComponent<Image>();
    }

    public bool OnDrop(int index, GameObject droppedOn, DraggableItem draggable)
    {
        if (droppedOn == null || droppedOn.name != "mainBg")
            return false;

        if (index != nextExpectedIndex)
            return false;

        string path = $"Art/level_{configIndex + 1}/steps/step_{index}";
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null && mainBgImage != null)
            mainBgImage.sprite = sprite;
        else
            Debug.LogWarning($"[GameplayLogicA] Sprite not found at: Resources/{path}");

        nextExpectedIndex++;

        if (nextExpectedIndex > totalComponents)
            OnEnd();

        return true;
    }

    public void OnEnd()
    {
        controller.NotifyLevelComplete();
    }
}
