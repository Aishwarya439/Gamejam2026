using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class DialogueEntry
{
    public string name;
    public string image;
    public string text;
}

[Serializable]
public class DialogueList
{
    public DialogueEntry[] dialogues;
}

public class DialogueManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private string rootFolder = "Dialogue";
    private string dataFileName = "data";
    private string imagesSubfolder = "image";

    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button nextButton;

    [Header("Typewriter Settings")]
    [SerializeField] private float typeSpeed = 0.03f;

    private List<DialogueEntry> dialogues = new List<DialogueEntry>();
    private int currentIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string currentFullText = "";

    private void Start()
    {
        LoadDialogues();

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextPressed);
        }

        if (dialogues.Count > 0)
        {
            ShowDialogue(0);
        }
    }

    private string CombinePath(params string[] parts)
    {
        List<string> validParts = new List<string>();
        foreach (string part in parts)
        {
            if (!string.IsNullOrEmpty(part))
            {
                validParts.Add(part.Trim('/'));
            }
        }
        return string.Join("/", validParts);
    }

    private void LoadDialogues()
    {
        string jsonPath = CombinePath(rootFolder, dataFileName);
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonPath);
        if (jsonFile == null)
        {
            Debug.LogError($"Could not load '{jsonPath}' from Resources folder.");
            return;
        }

        string wrappedJson = "{\"dialogues\":" + jsonFile.text + "}";
        DialogueList list = JsonUtility.FromJson<DialogueList>(wrappedJson);

        if (list != null && list.dialogues != null)
        {
            dialogues = new List<DialogueEntry>(list.dialogues);
        }
    }

    private void OnNextPressed()
    {
        currentIndex++;
        if (currentIndex < dialogues.Count)
        {
            ShowDialogue(currentIndex);
        }
        else
        {
            OnDialogueComplete();
        }
    }

    private void ShowDialogue(int index)
    {
        DialogueEntry entry = dialogues[index];
        currentFullText = entry.text;

        // Set name
        if (nameText != null)
        {
            nameText.text = entry.name ?? "";
        }

        // Load background from Resources
        if (backgroundImage != null && !string.IsNullOrEmpty(entry.image))
        {
            string bgPath = CombinePath(rootFolder, imagesSubfolder, entry.image);
            Sprite bgSprite = Resources.Load<Sprite>(bgPath);
            if (bgSprite != null)
            {
                backgroundImage.sprite = bgSprite;
                backgroundImage.enabled = true;
            }
            else
            {
                Debug.LogWarning($"Background image '{bgPath}' not found in Resources.");
            }
        }

        // Start typewriter
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(currentFullText));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        SetNextButtonActive(false);
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        SetNextButtonActive(true);
    }

    private void SetNextButtonActive(bool active)
    {
        if (nextButton == null) return;

        nextButton.gameObject.SetActive(active);

        if (active)
        {
            nextButton.transform.DOScale(1.08f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
        else
        {
            nextButton.transform.DOKill();
            nextButton.transform.localScale = Vector3.one;
        }
    }

    private void OnDialogueComplete()
    {
        SetNextButtonActive(false);
        MainController.Instance.LoadLobbyScene();
    }
}