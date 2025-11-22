using UnityEngine;
using UnityEngine.UI;

public class AdPopup : MonoBehaviour
{
    public Button closeButton;

    RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);

        // safety: snap inside popup area
        if (GameManager.Instance != null && rt != null)
            GameManager.Instance.SnapPopupInside(rt);
    }

    void OnCloseClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPopupClosed(this);

        Destroy(gameObject);
    }
}
