using System.Collections;
using UnityEngine;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] private GameObject mainControllerPrefab;
    [SerializeField] private GameObject audioSourcePrefab;

    private static GameObject mainControllerInstance;
    private static GameObject audioSourceInstance;

    private void Awake()
    {
        if (mainControllerInstance == null)
        {
            if (mainControllerPrefab == null)
            {
                Debug.LogError("MainController prefab is not assigned in the inspector!");
            }
            else
            {
                mainControllerInstance = Instantiate(mainControllerPrefab);
                mainControllerInstance.name = mainControllerPrefab.name;
                DontDestroyOnLoad(mainControllerInstance);
            }
        }

        if (audioSourceInstance == null)
        {
            if (audioSourcePrefab == null)
            {
                Debug.LogError("AudioSource prefab is not assigned in the inspector!");
            }
            else
            {
                audioSourceInstance = Instantiate(audioSourcePrefab);
                audioSourceInstance.name = audioSourcePrefab.name;
                DontDestroyOnLoad(audioSourceInstance);
            }
        }
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
