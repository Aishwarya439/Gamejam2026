using UnityEngine;
using UnityEngine.UI;

public class PuzzleSlot : MonoBehaviour
{
    public int slotIndex;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void RevealPiece(Sprite piece)
    {
        image.sprite = piece;
        image.color = Color.white;
    }
}
