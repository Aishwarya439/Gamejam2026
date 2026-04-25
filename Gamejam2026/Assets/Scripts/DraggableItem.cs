using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private int itemIndex;
    private GameSceneController gameSceneController;

    private Canvas rootCanvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private Vector2 originalPosition;
    private bool restoredByLogic = false;

    public int ItemIndex => itemIndex;
    public Transform OriginalParent => originalParent;
    public Vector2 OriginalPosition => originalPosition;

    public void Setup(int index, GameSceneController controller)
    {
        itemIndex = index;
        gameSceneController = controller;

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
    }

    public void RefreshOriginalPosition()
    {
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        restoredByLogic = false;

        GameObject droppedOn = eventData.pointerCurrentRaycast.gameObject;
        bool success = gameSceneController.TryDrop(itemIndex, droppedOn, this);

        if (success && !restoredByLogic)
            Hide();
        else if (!success)
            SnapBack();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Restore()
    {
        restoredByLogic = true;
        gameObject.SetActive(true);
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = originalPosition;
    }

    public void SnapBack()
    {
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = originalPosition;
    }
}
