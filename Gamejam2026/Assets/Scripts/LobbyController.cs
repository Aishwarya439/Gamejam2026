using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;
    [SerializeField] private float revealDelay = 0.75f;
    [SerializeField] private GameObject startGameObject;
    [SerializeField] private GameObject bookGameObject;
    [SerializeField] private GameObject startButton;
    
    private static List<int> panelOrder = new () { 1, 5, 4, 6, 11, 2, 7, 8, 9, 10, 3, 12 };
    
    void Start()
    {
        bool isFirstVisit = MainController.Instance.CompletionIndex == 0;
        startGameObject.SetActive(isFirstVisit);
        bookGameObject.SetActive(!isFirstVisit);

        if (isFirstVisit)
        {
            startButton.transform.DOScale(1.08f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
        else
        {
            for (int i = 0; i < panels.Length; i++)
            {
                int index = i;
                Button btn = panels[i].GetComponent<Button>();
                if (btn != null)
                    btn.onClick.AddListener(() => OnPanelClicked(index));
            }

            int count = MainController.Instance.CompletionIndex + 1;
            StartCoroutine(RevealPanels(count));
        }
    }
    
    public void OnStartClick()
    {
        startButton.transform.DOKill();
        startButton.transform.localScale = Vector3.one;
        startGameObject.SetActive(false);
        bookGameObject.SetActive(true);
        
        for (int i = 0; i < panels.Length; i++)
        {
            int index = i;
            Button btn = panels[i].GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnPanelClicked(index));
        }

        int count = MainController.Instance.CompletionIndex + 1;
        StartCoroutine(RevealPanels(count));
    }

    private void OnPanelClicked(int index)
    {
        Debug.Log($"Panel clicked: index {index}");
        MainController.Instance.SetConfigIndex(index);
        MainController.Instance.LoadGameScene();
    }
    
    private IEnumerator RevealPanels(int count)
    {
        // Show all previously completed panels instantly
        for (int i = 0; i < count - 1 && i < panelOrder.Count; i++)
        {
            int panelIndex = panelOrder[i] - 1;
            if (panelIndex >= 0 && panelIndex < panels.Length)
            {
                GameObject panel = panels[panelIndex];
                panel.SetActive(true);
                panel.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
            }
        }

        // Animate only the next unlocked panel
        int nextIndex = count - 1;
        if (nextIndex >= 0 && nextIndex < panelOrder.Count)
        {
            int panelIndex = panelOrder[nextIndex] - 1;
            if (panelIndex >= 0 && panelIndex < panels.Length)
            {
                GameObject panel = panels[panelIndex];
                panel.SetActive(true);

                Image img = panel.GetComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0f);
                img.DOFade(1f, 1f).SetEase(Ease.OutCubic);

                yield return new WaitForSeconds(revealDelay);

                Button btn = panel.GetComponent<Button>();
                if (btn != null)
                    btn.interactable = true;

                panel.transform.DOScale(1.08f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }
        }
        else
        {
            MainController.Instance.LoadCreditsScene();
        }

        yield break;
    }
}
