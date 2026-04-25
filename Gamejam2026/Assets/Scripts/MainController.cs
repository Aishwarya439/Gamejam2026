using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    public static MainController Instance { get; private set; }

    [SerializeField] private string[] variantConfigFiles;
    [SerializeField] private float fadeDuration = 0.4f;

    private int selectedConfigIndex = 3;
    private bool hasLoadedGame = false;

    public int CompletionIndex { get; set; }

    private CanvasGroup overlay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildOverlay();
    }

    private void Start()
    {
        AudioManager.Instance.PlayBGM("MainTheme");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void BuildOverlay()
    {
        GameObject canvasGo = new GameObject("TransitionCanvas");
        canvasGo.transform.SetParent(transform);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject panelGo = new GameObject("FadePanel");
        panelGo.transform.SetParent(canvasGo.transform, false);

        RectTransform rect = panelGo.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = panelGo.AddComponent<Image>();
        img.color = Color.black;

        overlay = panelGo.AddComponent<CanvasGroup>();
        overlay.alpha = 0f;
        overlay.blocksRaycasts = false;
    }

    public void SetConfigIndex(int index)
    {
        selectedConfigIndex = index;
    }

    public void LoadGameScene()
    {
        if (hasLoadedGame) return;
        hasLoadedGame = true;
        FadeAndLoad("GameScene");
    }

    public void LoadLobbyScene()
    {
        FadeAndLoad("LobbyScene");
    }

    public void OnSceneEnd()
    {
        CompletionIndex++;
        hasLoadedGame = false;
        FadeAndLoad("LobbyScene");
    }

    private void FadeAndLoad(string sceneName)
    {
        overlay.DOKill();
        overlay.blocksRaycasts = true;
        DOTween.To(() => overlay.alpha, x => overlay.alpha = x, 1f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() => SceneManager.LoadScene(sceneName));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SplashScene") return;

        overlay.DOKill();
        DOTween.To(() => overlay.alpha, x => overlay.alpha = x, 0f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() => overlay.blocksRaycasts = false);

        if (scene.name != "GameScene") return;

        GameSceneController controller = FindFirstObjectByType<GameSceneController>();
        if (controller == null)
        {
            Debug.LogError("GameSceneController not found in GameScene!");
            return;
        }

        if (variantConfigFiles == null || variantConfigFiles.Length == 0)
        {
            Debug.LogError("No variant config files assigned to MainController!");
            return;
        }

        VariantConfig[] configs = VariantConfig.LoadAll(variantConfigFiles[0]);
        if (configs == null || configs.Length == 0) return;

        int index = Mathf.Clamp(selectedConfigIndex, 0, configs.Length - 1);
        controller.ApplyVariant(configs[index], index);
    }
}
