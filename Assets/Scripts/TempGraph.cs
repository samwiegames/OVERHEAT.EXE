using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(RectTransform))]
public class TempGraph : MonoBehaviour
{
    [Header("Shape")]
    public int resolution = 64;
    public float baseAmplitude = 5f;
    public float maxAmplitude = 25f;

    public float baseFrequency = 0.8f;
    public float maxFrequency = 4f;

    [Header("Noise / Chaos")]
    public float maxNoiseAmplitude = 8f;
    public float minNoiseFrequency = 0.8f;
    public float maxNoiseFrequency = 4f;

    [Header("Animation")]
    public float scrollSpeed = 0.6f;

    float chaos01 = 0f; // 0 = calm, 1 = very chaotic

    LineRenderer lr;
    RectTransform rt;
    Vector3[] points;
    float phase;

    void Awake()
    {
        lr  = GetComponent<LineRenderer>();
        rt  = GetComponent<RectTransform>();

        if (resolution < 2) resolution = 2;

        points = new Vector3[resolution];
        lr.positionCount = resolution;
        lr.useWorldSpace = false;
    }

    /// <summary>
    /// called by GameManager with normalized temperature 0..1
    /// </summary>
    public void SetChaos01(float t)
    {
        chaos01 = Mathf.Clamp01(t);
    }

    void Update()
    {
        phase += scrollSpeed * Time.deltaTime;

        float width  = rt.rect.width;
        float height = rt.rect.height;
        float centerY = height * 0.5f;

        float amp       = Mathf.Lerp(baseAmplitude,   maxAmplitude,   chaos01);
        float freq      = Mathf.Lerp(baseFrequency,   maxFrequency,   chaos01);
        float noiseAmp  = Mathf.Lerp(0f,              maxNoiseAmplitude, chaos01);
        float noiseFreq = Mathf.Lerp(minNoiseFrequency, maxNoiseFrequency, chaos01);

        for (int i = 0; i < resolution; i++)
        {
            float xNorm = (float)i / (resolution - 1);
            float x = xNorm * width;

            float wave  = Mathf.Sin((xNorm * freq + phase) * Mathf.PI * 2f);
            float noise = (Mathf.PerlinNoise(xNorm * noiseFreq, Time.time * 1.5f) * 2f - 1f);

            float y = centerY + wave * amp + noise * noiseAmp;

            points[i] = new Vector3(x, y, 0f);
        }

        lr.SetPositions(points);
    }
}
