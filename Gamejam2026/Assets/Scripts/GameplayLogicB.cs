using UnityEngine;
using UnityEngine.UI;

public class GameplayLogicB : IGameplayLogic
{
    private GameSceneController controller;
    private int configIndex;
    private int totalComponents;
    private readonly int rows;
    private readonly int columns;

    private int placedCount = 0;
    private Image mainBgImage;

    public GameplayLogicB(int rows, int columns)
    {
        this.rows = rows;
        this.columns = columns;
    }

    public void Init(GameSceneController controller, int configIndex, int totalComponents)
    {
        this.controller = controller;
        this.configIndex = configIndex;
        this.totalComponents = totalComponents;

        GameObject mainBgGo = GameObject.Find("mainBg");
        if (mainBgGo != null)
        {
            mainBgImage = mainBgGo.GetComponent<Image>();
            string startPath = $"Art/level_{configIndex + 1}/start";
            Sprite startSprite = Resources.Load<Sprite>(startPath);
            if (startSprite != null)
                mainBgImage.sprite = startSprite;
        }

        CreatePuzzleSlots();
    }

    public bool OnDrop(int index, GameObject droppedOn, DraggableItem draggable)
    {
        if (droppedOn == null) return false;

        PuzzleSlot slot = droppedOn.GetComponent<PuzzleSlot>();
        if (slot == null) return false;

        if (slot.slotIndex != index) return false;

        string path = $"Art/level_{configIndex + 1}/components/step_{index}";
        Sprite piece = Resources.Load<Sprite>(path);
        if (piece != null)
            slot.RevealPiece(piece);
        else
            Debug.LogWarning($"[GameplayLogicB] Sprite not found at: Resources/{path}");

        placedCount++;
        if (placedCount >= totalComponents)
            OnEnd();

        return true;
    }

    public void OnEnd()
    {
        // Destroy all puzzle slots so mainBg is fully visible
        GameObject mainBgGo = GameObject.Find("mainBg");
        if (mainBgGo != null)
        {
            foreach (Transform child in mainBgGo.transform)
                GameObject.Destroy(child.gameObject);
        }

        string finishPath = $"Art/level_{configIndex + 1}/finish";
        Sprite finishSprite = Resources.Load<Sprite>(finishPath);
        if (finishSprite != null && mainBgImage != null)
            mainBgImage.sprite = finishSprite;
        else
            Debug.LogWarning($"[GameplayLogicB] Finish sprite not found at: Resources/{finishPath}");

        controller.NotifyLevelComplete();
    }

    private void CreatePuzzleSlots()
    {
        GameObject mainBgGo = GameObject.Find("mainBg");
        if (mainBgGo == null) return;

        RectTransform mainBgRect = mainBgGo.GetComponent<RectTransform>();
        Canvas.ForceUpdateCanvases();

        float bgWidth = mainBgRect.rect.width;
        float bgHeight = mainBgRect.rect.height;
        float slotWidth = bgWidth / columns;
        float slotHeight = bgHeight / rows;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                int slotIndex = r * columns + c + 1;

                GameObject slotGo = new GameObject($"PuzzleSlot_{slotIndex}", typeof(RectTransform), typeof(Image), typeof(PuzzleSlot));
                slotGo.transform.SetParent(mainBgGo.transform, false);

                RectTransform rect = slotGo.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(slotWidth, slotHeight);
                rect.anchoredPosition = new Vector2(c * slotWidth, -r * slotHeight);

                Image img = slotGo.GetComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0.1f);

                PuzzleSlot slot = slotGo.GetComponent<PuzzleSlot>();
                slot.slotIndex = slotIndex;
            }
        }
    }
}
