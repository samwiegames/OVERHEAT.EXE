using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ---------- UI AREAS ----------
    [Header("UI Areas")]
    public RectTransform popupArea;   // assign PopupArea
    public RectTransform canvasRect;  // assign Canvas

    // ---------- AD PREFABS ----------
    [Header("Ad Prefabs")]
    public List<GameObject> adPrefabs = new List<GameObject>();  // drag all 5 here

    // ---------- TIMER UI ----------
    [Header("Timer UI")]
    public TMP_Text timerText;
    public TMP_Text bestText;

    // ---------- TEMPERATURE ----------
    [Header("Temperature (°C internal)")]
    public float temp = 30f;
    public float tempMax = 100f;
    public float baseTempIncreasePerSecond = 1.5f;
    public float tempPerOpenAd = 0.15f;
    public float tempOnAdClose = -1f;   // negative = cools when you close

    [Header("Temperature UI")]
    public TemperatureBar tempBar;      // our custom bar
    public TMP_Text tempValueText;      // shows °F

    // ---------- SPAWNING ----------
    [Header("Spawning")]
    public float baseSpawnInterval = 1.2f;
    public float minSpawnInterval = 0.6f;
    public float difficultyRampPerSecond = 0.004f;
    public float spawnPadding = 10f;

    // ---------- GAME OVER ----------
    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;

    // internal state
    List<AdPopup> activeAds = new List<AdPopup>();
    float elapsedTime = 0f;
    float spawnDifficultyTime = 0f;
    float spawnTimer = 0f;
    bool isGameOver = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (canvasRect == null)
        {
            Canvas c = FindObjectOfType<Canvas>();
            if (c != null) canvasRect = c.GetComponent<RectTransform>();
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        float best = PlayerPrefs.GetFloat("BestTime", 0f);
        if (bestText != null)
            bestText.text = "best: " + FormatTime(best);

        UpdateTempUI();
    }

    void Update()
    {
        if (isGameOver) return;

        float dt = Time.deltaTime;
        elapsedTime += dt;
        spawnDifficultyTime += dt;

        if (timerText != null)
            timerText.text = "time: " + FormatTime(elapsedTime);

        HandleSpawning(dt);
        HandleTemperature(dt);
    }

    // ========== SPAWNING ==========

    void HandleSpawning(float dt)
    {
        if (adPrefabs == null || adPrefabs.Count == 0 || popupArea == null) return;

        float currentInterval = Mathf.Max(
            minSpawnInterval,
            baseSpawnInterval - difficultyRampPerSecond * spawnDifficultyTime
        );

        spawnTimer += dt;
        if (spawnTimer >= currentInterval)
        {
            spawnTimer = 0f;
            SpawnPopup();
        }
    }

    void SpawnPopup()
    {
        if (adPrefabs.Count == 0) return;

        int index = Random.Range(0, adPrefabs.Count);
        GameObject prefab = adPrefabs[index];

        GameObject go = Instantiate(prefab, popupArea);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;

        Vector2 pos = RandomInsidePopupArea(rt);
        rt.anchoredPosition = pos;

        AdPopup popup = go.GetComponent<AdPopup>();
        if (popup != null)
            activeAds.Add(popup);
    }

    Vector2 RandomInsidePopupArea(RectTransform popupRt)
    {
        Rect areaRect = popupArea.rect;
        float parentWidth = areaRect.width;
        float parentHeight = areaRect.height;

        float halfW = popupRt.rect.width / 2f;
        float halfH = popupRt.rect.height / 2f;

        float maxX = parentWidth / 2f - halfW - spawnPadding;
        float maxY = parentHeight / 2f - halfH - spawnPadding;

        float x = Random.Range(-maxX, maxX);
        float y = Random.Range(-maxY, maxY);

        return new Vector2(x, y);
    }

    public void SnapPopupInside(RectTransform popupRt)
    {
        Rect areaRect = popupArea.rect;
        float parentWidth = areaRect.width;
        float parentHeight = areaRect.height;

        float halfW = popupRt.rect.width / 2f;
        float halfH = popupRt.rect.height / 2f;

        float maxX = parentWidth / 2f - halfW - spawnPadding;
        float maxY = parentHeight / 2f - halfH - spawnPadding;

        Vector2 pos = popupRt.anchoredPosition;
        pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
        pos.y = Mathf.Clamp(pos.y, -maxY, maxY);
        popupRt.anchoredPosition = pos;
    }

    // called by AdPopup
    public void OnPopupClosed(AdPopup popup)
    {
        if (activeAds.Contains(popup))
            activeAds.Remove(popup);

        temp += tempOnAdClose;
        temp = Mathf.Clamp(temp, 0f, tempMax);
        UpdateTempUI();
    }

    // ========== TEMPERATURE ==========

    void HandleTemperature(float dt)
    {
        // temperature rises over time
        float inc = baseTempIncreasePerSecond * dt;

        // plus extra heat per open ad
        inc += activeAds.Count * tempPerOpenAd * dt;

        temp += inc;
        temp = Mathf.Clamp(temp, 0f, tempMax);

        UpdateTempUI();

        if (temp >= tempMax)
        {
            TriggerGameOver("your pc overheated!");
        }
    }

    void UpdateTempUI()
    {
        float tNorm = Mathf.Clamp01(temp / tempMax);

        if (tempBar != null)
            tempBar.SetValue01(tNorm);

        if (tempValueText != null)
        {
            float f = temp * 9f / 5f + 32f;
            tempValueText.text = Mathf.RoundToInt(f) + "°F";
        }
    }

    // ========== GAME OVER ==========

    void TriggerGameOver(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;

        float best = PlayerPrefs.GetFloat("BestTime", 0f);
        if (elapsedTime > best)
        {
            best = elapsedTime;
            PlayerPrefs.SetFloat("BestTime", best);
        }

        if (bestText != null)
            bestText.text = "best: " + FormatTime(best);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
            gameOverText.text = reason + "\n\nsurvived: " + FormatTime(elapsedTime);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

        // --- POWERUP EFFECTS ---

    public void ChangeTemperature(float delta)
    {
        temp += delta;
        temp = Mathf.Clamp(temp, 0f, tempMax);
        UpdateTempUI();
    }

    public System.Collections.IEnumerator DoFreeze()
    {
        float original = baseTempIncreasePerSecond;
        baseTempIncreasePerSecond = 0f;
        yield return new WaitForSeconds(6f);
        baseTempIncreasePerSecond = original;
    }

    public void DoClearAds()
    {
        foreach (var p in activeAds)
            Destroy(p.gameObject);
        activeAds.Clear();

        // reset difficulty ramp too
        spawnDifficultyTime = 0f;
    }


    string FormatTime(float t)
    {
        int s = Mathf.FloorToInt(t);
        int m = s / 60;
        int sec = s % 60;
        return m.ToString("00") + ":" + sec.ToString("00");
    }
}


