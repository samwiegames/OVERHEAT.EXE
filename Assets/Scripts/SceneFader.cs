using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [Header("Fade Settings")]
    public float fadeDuration = 0.6f;   // time to fade in/out

    CanvasGroup cg;
    bool isFading = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        // start fully transparent
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
    }

    void OnEnable()
    {
        // subscribe to sceneLoaded so every new scene fades in automatically
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // when a scene loads, fade from black → clear
        StartCoroutine(FadeIn());
    }

    // ---------- PUBLIC API ----------

    public void FadeToScene(string sceneName)
    {
        if (isFading) return;
        StartCoroutine(FadeOutThenLoadScene(sceneName));
    }

    public void FadeRestartCurrent()
    {
        if (isFading) return;
        StartCoroutine(FadeOutThenLoadScene(SceneManager.GetActiveScene().name));
    }

    public void FadeToMenu(string menuSceneName)
    {
        if (isFading) return;
        StartCoroutine(FadeOutThenLoadScene(menuSceneName));
    }

    public void FadeAndQuit()
    {
        if (isFading) return;
        StartCoroutine(FadeOutThenQuit());
    }

    // ---------- COROUTINES ----------

    IEnumerator FadeIn()
    {
        isFading = true;
        cg.blocksRaycasts = true;

        float t = 0f;
        float start = cg.alpha;
        float end = 0f;   // fully clear

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);
            cg.alpha = Mathf.Lerp(start, end, k);
            yield return null;
        }

        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        isFading = false;
    }

    IEnumerator FadeOutThenLoadScene(string sceneName)
    {
        isFading = true;
        cg.blocksRaycasts = true;

        float t = 0f;
        float start = cg.alpha;
        float end = 1f;   // fully black

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);
            cg.alpha = Mathf.Lerp(start, end, k);
            yield return null;
        }

        cg.alpha = 1f;

        // make sure time is running normally in the new scene
        Time.timeScale = 1f;

        SceneManager.LoadScene(sceneName);
        // FadeIn will be called automatically by OnSceneLoaded

        // we leave isFading true until FadeIn finishes
    }

    IEnumerator FadeOutThenQuit()
    {
        isFading = true;
        cg.blocksRaycasts = true;

        float t = 0f;
        float start = cg.alpha;
        float end = 1f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);
            cg.alpha = Mathf.Lerp(start, end, k);
            yield return null;
        }

        cg.alpha = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
