using UnityEngine;
using UnityEngine.UI;

public class componentLayerContainer : MonoBehaviour
{
    [SerializeField] private Image image;

    public void Setup(int index, int assetId, GameSceneController controller)
    {
        string path = $"Art/level_{assetId}/components/step_{index}";
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
            image.sprite = sprite;
        else
            Debug.LogWarning($"[componentLayerContainer] Sprite not found at: Resources/{path}");

        DraggableItem draggable = gameObject.AddComponent<DraggableItem>();
        draggable.Setup(index, controller);
    }
}
