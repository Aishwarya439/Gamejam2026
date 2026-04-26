using UnityEngine;
using UnityEngine.UI;

public class CreditsController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float scrollSpeed = 0.05f;

    private bool isDone = false;

    void Update()
    {
        if (isDone) return;

        float newPos = scrollRect.verticalNormalizedPosition - scrollSpeed * Time.deltaTime;

        if (newPos <= 0f)
        {
            newPos = 0f;
            isDone = true;
            OnCreditsComplete();
        }

        scrollRect.verticalNormalizedPosition = newPos;
    }

    private void OnCreditsComplete()
    {
        //MainController.Instance.LoadLobbyScene();
    }
}