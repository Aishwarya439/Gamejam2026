using UnityEngine;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] private GameObject mainControllerPrefab;
    
    private static GameObject mainControllerInstance;

    private void Awake()
    {
        // Prevent duplicates if the splash scene is somehow loaded again
        if (mainControllerInstance != null)
        {
            return;
        }

        if (mainControllerPrefab == null)
        {
            Debug.LogError("MainController prefab is not assigned in the inspector!");
            return;
        }

        // Instantiate the prefab
        mainControllerInstance = Instantiate(mainControllerPrefab);
        mainControllerInstance.name = mainControllerPrefab.name; // removes "(Clone)" suffix

        // Make it persist across scene loads
        DontDestroyOnLoad(mainControllerInstance);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
