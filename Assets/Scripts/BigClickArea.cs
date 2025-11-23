using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Makes a button clickable even outside its normal bounds, by expanding the hitbox by 'extraSize' pixels.
/// Attach this to the same object that has the Button component.
/// </summary>
[RequireComponent(typeof(Button))]
public class BigClickArea : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("How many pixels beyond the button should still count as click?")]
    public float extraSize = 22f;

    private Button button;
    private RectTransform rt;
    private bool hovering = false;

    void Awake()
    {
        button = GetComponent<Button>();
        rt = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!button.interactable) return;

        // convert mouse pos to local point inside the rect
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt, eventData.position, eventData.pressEventCamera, out localPos
        );

        // expand clickable zone beyond the rect bounds
        float halfW = rt.rect.width * 0.5f + extraSize;
        float halfH = rt.rect.height * 0.5f + extraSize;

        if (Mathf.Abs(localPos.x) <= halfW && Mathf.Abs(localPos.y) <= halfH)
        {
            // treat it as a click on the button
            button.onClick.Invoke();
        }
    }
}
