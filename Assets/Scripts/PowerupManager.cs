using UnityEngine;
using Random = UnityEngine.Random;

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager Instance;

    [Header("UI")]
    public RectTransform powerupBar;   // the bar across the bottom
    public RectTransform hitZone;     // the central box
    public Transform moverParent;     // parent for moving icon (PowerupMover)

    [Header("Prefabs")]
    public GameObject powerFreeze;
    public GameObject powerCool;
    public GameObject powerClear;

    [Header("Spawn Chances")]
    [Range(0f, 1f)] public float chanceFreeze = 0.5f;
    [Range(0f, 1f)] public float chanceCool   = 0.35f;
    [Range(0f, 1f)] public float chanceClear  = 0.15f;

    [Header("Movement Speed")]
    public float minSpeed = 260f;
    public float maxSpeed = 620f;   // bigger range for more randomness

    [Header("Spawn Timing")]
    public float minDelay = 4f;
    public float maxDelay = 11f;    // more random between spawns

    [Header("Visuals")]
    public bool rotateIcon = true;
    public float rotateSpeed = 220f;

    [Tooltip("Fraction of the bar width at each edge used for fading in/out.")]
    [Range(0.05f, 0.45f)]
    public float edgeFadeFraction = 0.25f;

    GameObject currentIcon;
    RectTransform currentRT;
    CanvasGroup currentCG;

    float speed;
    bool active = false;

    float spawnTimer = 0f;
    float nextSpawnDelay = 7f; // will be randomized in Start()

    float startX;
    float endX;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // randomize the very first spawn too
        nextSpawnDelay = Random.Range(minDelay, maxDelay);
    }

    void Update()
    {
        if (!active)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= nextSpawnDelay)
                SpawnOne();
            return;
        }

        // move
        currentRT.anchoredPosition += Vector2.left * speed * Time.deltaTime;

        float x = currentRT.anchoredPosition.x;

        // rotate icon while it travels
        if (rotateIcon)
        {
            currentRT.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
        }

        // fade based on x position
        UpdateFadeForPosition(x);

        // missed & went off the left side -> end
        if (x <= endX)
        {
            EndIcon(false);
        }

        // catch attempt
        if (Input.GetKeyDown(KeyCode.Space))
            CheckCatch();
    }

    void SpawnOne()
    {
        spawnTimer = 0f;
        nextSpawnDelay = Random.Range(minDelay, maxDelay);

        currentIcon = Instantiate(SelectRandomPrefab(), moverParent);
        currentRT = currentIcon.GetComponent<RectTransform>();
        currentRT.localScale = Vector3.one;

        // compute edges based on bar width
        float halfBarWidth = powerupBar.rect.width / 2f;

        // start just off the right side, end just off the left
        startX = halfBarWidth + 60f;
        endX   = -halfBarWidth - 60f;

        currentRT.anchoredPosition = new Vector2(startX, 0f);

        speed = Random.Range(minSpeed, maxSpeed);

        // set up canvas group for fade
        currentCG = currentIcon.GetComponent<CanvasGroup>();
        if (currentCG == null)
            currentCG = currentIcon.AddComponent<CanvasGroup>();

        currentCG.alpha = 0f;  // start invisible

        active = true;
    }

    GameObject SelectRandomPrefab()
    {
        float r = Random.value;
        if (r < chanceClear) return powerClear;
        if (r < chanceClear + chanceCool) return powerCool;
        return powerFreeze;
    }

    void CheckCatch()
    {
        float x = currentRT.anchoredPosition.x;
        float zoneHalf = hitZone.rect.width / 2f;

        if (Mathf.Abs(x) <= zoneHalf)
            ApplyPowerup(currentIcon.name);

        EndIcon(true);
    }

    void EndIcon(bool triggered)
    {
        active = false;
        if (currentIcon != null)
            Destroy(currentIcon);

        currentIcon = null;
        currentRT = null;
        currentCG = null;
    }

    void ApplyPowerup(string name)
    {
        if (GameManager.Instance == null) return;

        // NOTE: these now ADD TO INVENTORY (GameManager handles actual use on key 1/2/3)
        if (name.Contains("Freeze"))
        {
            GameManager.Instance.AddPowerupFreeze(1);
        }
        else if (name.Contains("Cool"))
        {
            GameManager.Instance.AddPowerupCool(1);
        }
        else if (name.Contains("Clear"))
        {
            GameManager.Instance.AddPowerupClear(1);
        }
    }

    // ─────────────────────────────────────────────
    // FADE LOGIC – based purely on X position
    // fades in near right edge, full opacity around centre,
    // fades out as it exits left if you miss.
    // ─────────────────────────────────────────────
    void UpdateFadeForPosition(float x)
    {
        if (currentCG == null || powerupBar == null) return;

        float barWidth = powerupBar.rect.width;
        float halfBar  = barWidth / 2f;

        // plateau region where icon is fully visible
        // fade region is edgeFadeFraction * width from each side
        float plateauRight = halfBar - barWidth * edgeFadeFraction;
        float plateauLeft  = -plateauRight;

        float alpha = 1f;

        if (x > plateauRight)
        {
            // fade IN as it travels from offscreen (startX) to plateauRight
            alpha = Mathf.InverseLerp(startX, plateauRight, x);
        }
        else if (x < plateauLeft)
        {
            // fade OUT as it travels from plateauLeft to completely off (endX)
            alpha = Mathf.InverseLerp(endX, plateauLeft, x);
        }
        else
        {
            // centre zone → fully visible
            alpha = 1f;
        }

        currentCG.alpha = Mathf.Clamp01(alpha);
    }
}
