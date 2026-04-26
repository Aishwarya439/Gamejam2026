using System.Collections;
using UnityEngine;

public class GameSceneController : MonoBehaviour
{
    [SerializeField] private GameObject bottomLayer;
    [SerializeField] private GameObject componentLayerPrefab;
    [SerializeField] private GameObject componentLayerContainerPrefab;
    [SerializeField] private GameObject bottomLayerContainerPrefab;
    [SerializeField] private GameObject dummyMainBgPrefab;

    public GameObject BottomLayerContainerPrefab => bottomLayerContainerPrefab;
    public GameObject DummyMainBgPrefab => dummyMainBgPrefab;

    private componentContainer leftComponentContainer;
    private componentContainer rightComponentContainer;
    private VariantConfig pendingConfig;
    private int pendingConfigIndex;
    private IGameplayLogic activeLogic;

    private void Awake()
    {
        if (bottomLayer == null)
            bottomLayer = GameObject.Find("bottomLayer");

        Canvas canvas = FindSceneCanvas();
        if (canvas == null) return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Canvas.ForceUpdateCanvases();

        leftComponentContainer = CreateContainer("leftComponentContainer", canvasRect, isLeft: true);
        rightComponentContainer = CreateContainer("rightComponentContainer", canvasRect, isLeft: false);
    }

    private void Start()
    {
        if (pendingConfig != null)
            ApplyVariantInternal(pendingConfig, pendingConfigIndex);
    }

    private componentContainer CreateContainer(string containerName, RectTransform canvasRect, bool isLeft)
    {
        GameObject go = Instantiate(componentLayerPrefab, canvasRect);
        go.name = containerName;

        RectTransform rect = go.GetComponent<RectTransform>();

        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        float containerWidth = canvasWidth * 0.20f;
        float containerHeight = canvasHeight * 0.60f;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(containerWidth, containerHeight);

        float padding = 10f;
        float xPos = isLeft
            ? -canvasWidth / 2f + containerWidth / 2f + padding
            : canvasWidth / 2f - containerWidth / 2f - padding;
        rect.anchoredPosition = new Vector2(xPos, rect.anchoredPosition.y);

        return go.GetComponent<componentContainer>();
    }

    public void ApplyVariant(VariantConfig config, int configIndex)
    {
        if (leftComponentContainer == null || rightComponentContainer == null)
        {
            pendingConfig = config;
            pendingConfigIndex = configIndex;
            return;
        }
        ApplyVariantInternal(config, configIndex);
    }

    private void ApplyVariantInternal(VariantConfig config, int configIndex)
    {
        if (bottomLayer == null || leftComponentContainer == null || rightComponentContainer == null)
        {
            Debug.LogError("GameSceneController: missing scene references!");
            return;
        }

        bottomLayer.SetActive(config.VisibilityState == VisibilityState.WithBottomLayer);

        GameObject dummyMainBg = GameObject.Find("dummyMainBg");
        if (dummyMainBg != null)
            dummyMainBg.SetActive(config.gameType == "variant_1");

        if (config.VisibilityState == VisibilityState.WithoutBottomLayer)
            ApplyFullHeightLayout();

        SetGameBg(config.asset_id);
        SetMainBgSprite(config.asset_id);

        activeLogic = CreateLogic(config);
        activeLogic.Init(this, configIndex, config.asset_id, config.components);

        SplitAndPopulate(config.components, configIndex, config.asset_id);

        Debug.Log($"[GameScene] GameType: {config.gameType} | Visibility: {config.VisibilityState} | Logic: {config.EvaluationLogic}");
    }

    private void SetMainBgSprite(int assetId)
    {
        GameObject mainBgGo = GameObject.Find("mainBg");
        if (mainBgGo == null) return;

        string path = $"Art/level_{assetId}/start";
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
        {
            mainBgGo.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
            Debug.Log($"[GameScene] mainBg set to: Resources/{path}");
        }
        else
            Debug.LogWarning($"[GameScene] mainBg sprite not found at: Resources/{path}");
    }

    private void SetGameBg(int assetId)
    {
        GameObject gameBgGo = GameObject.Find("gameBg");
        if (gameBgGo == null) return;

        string path = $"Art/level_{assetId}/bg";
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
            gameBgGo.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
        else
            Debug.LogWarning($"[GameScene] gameBg sprite not found at: Resources/{path}");
    }

    private Canvas FindSceneCanvas()
    {
        foreach (Canvas c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (c.gameObject.name != "TransitionCanvas")
                return c;
        }
        return null;
    }

    private void ApplyFullHeightLayout()
    {
        Canvas canvas = FindSceneCanvas();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        float targetHeight = canvasHeight * 0.9f;

        GameObject mainBgGo = GameObject.Find("mainBg");
        if (mainBgGo != null)
        {
            RectTransform rect = mainBgGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(rect.anchorMin.x, 0.5f);
            rect.anchorMax = new Vector2(rect.anchorMax.x, 0.5f);
            rect.pivot = new Vector2(rect.pivot.x, 0.5f);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f);
        }

        ResizeContainers(targetHeight);
    }

    private void ResizeContainers(float height)
    {
        SetContainerHeight(leftComponentContainer, height);
        SetContainerHeight(rightComponentContainer, height);
    }

    private void SetContainerHeight(componentContainer container, float height)
    {
        if (container == null) return;
        RectTransform rect = container.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f);
    }

    private void SplitAndPopulate(int totalCount, int configIndex, int assetId)
    {
        if (totalCount <= 0) return;

        int[] indices = new int[totalCount];
        for (int i = 0; i < totalCount; i++) indices[i] = i + 1;
        Shuffle(indices);

        int leftCount = Mathf.CeilToInt(totalCount / 2f);
        int[] leftIndices = new int[leftCount];
        int[] rightIndices = new int[totalCount - leftCount];
        System.Array.Copy(indices, 0, leftIndices, 0, leftCount);
        System.Array.Copy(indices, leftCount, rightIndices, 0, rightIndices.Length);

        leftComponentContainer.Populate(leftIndices, configIndex, assetId, this, componentLayerContainerPrefab);
        rightComponentContainer.Populate(rightIndices, configIndex, assetId, this, componentLayerContainerPrefab);
    }

    private void Shuffle(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    public bool TryDrop(int index, GameObject droppedOn, DraggableItem draggable)
    {
        if (activeLogic == null) return false;
        return activeLogic.OnDrop(index, droppedOn, draggable);
    }

    public void NotifyLevelComplete()
    {
        StartCoroutine(DelayedSceneEnd());
    }

    private IEnumerator DelayedSceneEnd()
    {
        yield return new WaitForSeconds(2f);
        MainController.Instance.OnSceneEnd();
    }

    public void ShakeAndReset(System.Action onComplete)
    {
        StartCoroutine(ShakeAndResetCoroutine(onComplete));
    }

    private IEnumerator ShakeAndResetCoroutine(System.Action onComplete)
    {
        if (bottomLayer != null)
        {
            Vector3 originalPos = bottomLayer.transform.localPosition;
            float elapsed = 0f;
            float duration = 1.5f;
            float magnitude = 10f;
            float frequency = 30f;

            while (elapsed < duration)
            {
                float x = Mathf.Sin(elapsed * frequency) * magnitude * (1f - elapsed / duration);
                bottomLayer.transform.localPosition = originalPos + new Vector3(x, 0f, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            bottomLayer.transform.localPosition = originalPos;
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }

        onComplete?.Invoke();
    }

    private IGameplayLogic CreateLogic(VariantConfig config)
    {
        return config.gameType switch
        {
            "variant_1" => new GameplayLogicA(config.slots),
            "variant_2" => new GameplayLogicB(),
            "variant_3" => new GameplayLogicC(config.rows, config.columns),
            "variant_4" => new GameplayLogicD(config.slots),
            _ => new GameplayLogicA(config.slots)
        };
    }
}
