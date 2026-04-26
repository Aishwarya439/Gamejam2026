using TMPro;
using UnityEngine;

public class Headerztext : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void Init(string text)
    {
        this.text.text = text;
    }
}
