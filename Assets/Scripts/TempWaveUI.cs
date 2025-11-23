using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempWaveUI : MonoBehaviour
{
    [Header("Setup")]
    public RectTransform waveArea;   // usually RadioWaveBox
    public Image pointPrefab;        // tiny square Image prefab
    [Range(8, 128)] public int pointCount = 40;

    [Header("Wave Behaviour")]
    public float minAmplitude = 5f;
    public float maxAmplitude = 35f;
    public float minFrequency = 1f;
    public float maxFrequency = 7f;
    public float noiseAmount = 6f;         // extra chaos on high temp

    [Header("Colors (optional)")]
    public Color coolColor = new Color(0.4f, 0.8f, 1f, 1f);
    public Color hotColor  = new Color(1f, 0.3f, 0.2f, 1f);

    List<RectTransform> points = new List<RectTransform>();

    void Awake()
    {
        if (waveArea == null)
            waveArea = GetComponent<RectTransform>();

        CreatePoints();
    }

    void CreatePoints()
    {
        // clear old
        foreach (var p in points)
        {
            if (p != null) Destroy(p.gameObject);
        }
        points.Clear();

        if (pointPrefab == null || waveArea == null) return;

        for (int i = 0; i < pointCount; i++)
        {
            Image img = Instantiate(pointPrefab, waveArea);
            RectTransform rt = img.rectTransform;
            rt.localScale = Vector3.one;
            points.Add(rt);
        }
    }

    void Update()
    {
        if (waveArea == null || points.Count == 0) return;
        if (GameManager.Instance == null) return;

        // stop moving after game over so the screen feels "dead"
        if (GameManager.Instance.IsGameOver) return;

        float temp = GameManager.Instance.temp;
        float tempMax = Mathf.Max(1f, GameManager.Instance.tempMax);
        float heat01 = Mathf.Clamp01(temp / tempMax);

        // map temp to amplitude & frequency
        float amp = Mathf.Lerp(minAmplitude, maxAmplitude, heat01);
        float freq = Mathf.Lerp(minFrequency, maxFrequency, heat01);

        float width = waveArea.rect.width;
        float height = waveArea.rect.height;
        float halfW = width * 0.5f;

        float time = Time.time;

        for (int i = 0; i < points.Count; i++)
        {
            float t = (points.Count <= 1) ? 0f : (float)i / (points.Count - 1);
            float x = Mathf.Lerp(-halfW, halfW, t);

            // base sine
            float y = Mathf.Sin((t * freq * Mathf.PI * 2f) + time * freq);

            // add some random-ish jitter when hot
            float chaos = heat01 * noiseAmount;
            if (chaos > 0f)
            {
                // small hash-based noise so it doesn't change every frame crazily
                float n = Mathf.PerlinNoise(time * freq + i * 0.37f, i * 0.11f) * 2f - 1f;
                y += n * chaos * 0.05f;
            }

            y *= amp;

            RectTransform rt = points[i];
            rt.anchoredPosition = new Vector2(x, y);

            // color lerp along temp
            if (rt.TryGetComponent<Image>(out var img))
            {
                img.color = Color.Lerp(coolColor, hotColor, heat01);
            }
        }
    }

#if UNITY_EDITOR
    // if you change pointCount in inspector at edit time, rebuild points
    void OnValidate()
    {
        if (waveArea != null && pointPrefab != null && Application.isPlaying == false)
        {
            CreatePoints();
        }
    }
#endif
}
