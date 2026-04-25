using System.Collections;
using UnityEngine;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] private GameObject mainControllerPrefab;

    private static GameObject mainControllerInstance;

    private void Awake()
    {
        if (mainControllerInstance != null) return;

        if (mainControllerPrefab == null)
        {
            Debug.LogError("MainController prefab is not assigned in the inspector!");
            return;
        }

        mainControllerInstance = Instantiate(mainControllerPrefab);
        mainControllerInstance.name = mainControllerPrefab.name;
        DontDestroyOnLoad(mainControllerInstance);
    }

    private void Start()
    {
        StartCoroutine(AdvanceToGame());
    }

    private IEnumerator AdvanceToGame()
    {
        yield return new WaitForSeconds(1f);

        if (mainControllerInstance != null)
            mainControllerInstance.GetComponent<MainController>().LoadGameScene();
    }
}
