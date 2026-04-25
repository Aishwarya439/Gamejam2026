using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;
    [SerializeField] private float revealDelay = 0.75f;
    
    private static List<int> panelOrder = new () { 1, 5, 4, 6, 11, 2, 7, 8, 9, 10, 3, 12 };
    
    void Start()
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

    private void OnPanelClicked(int index)
    {
        // do index+1
        Debug.Log($"Panel clicked: index {index+1}");
        MainController.Instance.SetConfigIndex(index+1);
    }
    
    private IEnumerator RevealPanels(int count)
    {
        for (int i = 0; i < count && i < panelOrder.Count; i++)
        {
            int panelIndex = panelOrder[i] - 1;
            if (panelIndex >= 0 && panelIndex < panels.Length)
            {
                GameObject panel = panels[panelIndex];
                panel.SetActive(true);
                panel.GetComponent<Image>().DOFade(1, 1).SetEase(Ease.OutCubic);

                if (i == count - 1)
                {
                    Button btn = panel.GetComponent<Button>();
                    if (btn != null)
                        btn.interactable = true;

                    panel.transform.DOScale(1.08f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                }
            }
            yield return new WaitForSeconds(revealDelay);
        }
    }
}
