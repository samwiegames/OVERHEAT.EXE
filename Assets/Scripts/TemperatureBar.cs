using UnityEngine;
using UnityEngine.UI;

public class TemperatureBar : MonoBehaviour
{
    [Header("Fill")]
    public RectTransform fillRect;   // child "Fill"
    public Image fillImage;          // Image on Fill

    [Header("Color Gradient")]
    public Gradient temperatureGradient; // blue -> orange -> red

    float currentValue = 0f; // 0..1

    /// <summary>
    /// called by GameManager with normalized temp 0..1
    /// </summary>
    public void SetValue01(float value01)
    {
        currentValue = Mathf.Clamp01(value01);

        // scale the fill vertically from the bottom
        if (fillRect != null)
        {
            Vector3 scale = fillRect.localScale;
            scale.y = Mathf.Max(0.0001f, currentValue); // avoid 0 scale
            fillRect.localScale = scale;
        }

        // color from gradient
        if (temperatureGradient != null && fillImage != null)
        {
            Color c = temperatureGradient.Evaluate(currentValue);
            fillImage.color = c;
        }
    }
}
