using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI")]
    public GameObject overlay;
    public CanvasGroup canvasGroup;
    public TMP_Text tutorialText;
    public Button okButton;
    public RectTransform highlightBox;

    int step = 0;

    const string KEY = "TUTORIAL_DONE";

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        okButton.onClick.AddListener(NextStep);

        if (PlayerPrefs.GetInt(KEY, 0) == 1)
        {
            overlay.SetActive(false);
            return;
        }

        StartTutorial();
    }

    void StartTutorial()
    {
        overlay.SetActive(true);
        Time.timeScale = 0f;
        step = 0;
        ShowStep();
    }

    void ShowStep()
    {
        switch (step)
        {
            case 0:
                tutorialText.text = "close ads using the X button.";
                HighlightAnyAd();
                break;

            case 1:
                tutorialText.text = "this is a bomb ad. clicking it ends your game.";
                HighlightBombAd();
                break;

            case 2:
                tutorialText.text = "this is a cascade ad. it spawns more ads.";
                HighlightCascadeAd();
                break;

            case 3:
                tutorialText.text = "watch this bar. if it fills, you lose.";
                HighlightTempBar();
                break;

            case 4:
                tutorialText.text = "press SPACE to collect powerups.";
                HighlightPowerupBar();
                break;

            case 5:
                EndTutorial();
                break;
        }
    }

    void NextStep()
    {
        step++;
        ShowStep();
    }

    void EndTutorial()
    {
        PlayerPrefs.SetInt(KEY, 1);
        PlayerPrefs.Save();

        overlay.SetActive(false);
        Time.timeScale = 1f;
    }

    // ───── HIGHLIGHTS ─────

    void HighlightRect(RectTransform target)
    {
        if (target == null) return;

        highlightBox.position = target.position;
        highlightBox.sizeDelta = target.sizeDelta + new Vector2(40, 40);
    }

    void HighlightAnyAd()
    {
        var ad = FindObjectOfType<AdPopup>();
        if (ad != null)
            HighlightRect(ad.GetComponent<RectTransform>());
    }

    void HighlightBombAd()
    {
        foreach (var ad in FindObjectsOfType<AdPopup>())
        {
            if (ad.adType == AdPopup.AdType.Bomb)
            {
                HighlightRect(ad.GetComponent<RectTransform>());
                return;
            }
        }
    }

    void HighlightCascadeAd()
    {
        foreach (var ad in FindObjectsOfType<AdPopup>())
        {
            if (ad.adType == AdPopup.AdType.Cascade)
            {
                HighlightRect(ad.GetComponent<RectTransform>());
                return;
            }
        }
    }

    void HighlightTempBar()
    {
        var obj = GameObject.Find("TemperatureBar");
        if (obj != null)
            HighlightRect(obj.GetComponent<RectTransform>());
    }

    void HighlightPowerupBar()
    {
        var obj = GameObject.Find("PowerupBar");
        if (obj != null)
            HighlightRect(obj.GetComponent<RectTransform>());
    }
}