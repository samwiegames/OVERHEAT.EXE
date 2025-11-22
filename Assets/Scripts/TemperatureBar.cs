using UnityEngine;

public class TemperatureBar : MonoBehaviour
{
    public RectTransform fillRect;
    float fullHeight;

    void Start()
    {
        fullHeight = GetComponent<RectTransform>().rect.height - 8f; // minus padding
    }

    public void SetValue01(float t) // t = 0..1
    {
        t = Mathf.Clamp01(t);
        Vector2 size = fillRect.sizeDelta;
        size.y = fullHeight * t;
        fillRect.sizeDelta = size;
    }
}
