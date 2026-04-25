using UnityEngine;
using UnityEngine.SceneManagement;

public class MainController : MonoBehaviour
{
    public static MainController Instance { get; private set; }

    [SerializeField] private string[] variantConfigFiles;

    private int selectedConfigIndex = 0;
    private bool hasLoadedGame = false;

    public int CompletionIndex { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SetConfigIndex(int index)
    {
        selectedConfigIndex = index;
    }

    public void LoadGameScene()
    {
        if (hasLoadedGame) return;
        hasLoadedGame = true;
        SceneManager.LoadScene("GameScene");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
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
        controller.ApplyVariant(configs[index]);
    }
}
