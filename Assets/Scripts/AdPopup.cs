using UnityEngine;
using UnityEngine.UI;

public class AdPopup : MonoBehaviour
{
    public enum AdType
    {
        Normal,
        Bomb,
        Cascade
    }

    [Header("Setup")]
    public Button closeButton;
    public AdType adType = AdType.Normal;

    [Header("Auto Despawn (for Bomb / Cascade)")]
    public bool autoDespawn = false;
    public float autoDespawnTime = 6f;   // seconds before it disappears on its own

    RectTransform rt;
    float lifeTimer = 0f;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);

        if (GameManager.Instance != null && rt != null)
            GameManager.Instance.SnapPopupInside(rt);
    }

    void Update()
    {
        if (!autoDespawn) return;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= autoDespawnTime)
        {
            AutoDespawn();
        }
    }

    void OnCloseClicked()
    {
        if (GameManager.Instance == null)
        {
            Destroy(gameObject);
            return;
        }

        switch (adType)
        {
            case AdType.Normal:
                GameManager.Instance.OnPopupClosed(this);
                break;

            case AdType.Bomb:
                GameManager.Instance.OnBombAdClicked(this);
                break;

            case AdType.Cascade:
                GameManager.Instance.OnCascadeAdClosed(this);
                break;
        }

        Destroy(gameObject);
    }

    void AutoDespawn()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnAdAutoDespawn(this);   // no temp bonus/penalty

        Destroy(gameObject);
    }
}
