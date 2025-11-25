using UnityEngine;
using UnityEngine.EventSystems;

public class ClickSfxManager : MonoBehaviour
{
    public static ClickSfxManager Instance { get; private set; }

    [Header("Audio")]
    public AudioSource sfxSource;           // one-shot AudioSource (no loop)
    public AudioClip clickClip;             // main click sound
    [Range(0f,1f)] public float clickVolume = 0.9f;

    [Header("Optional variations")]
    public AudioClip[] clickVariations;     // optional small variations (picked randomly)
    [Range(0f,1f)] public float variationVolume = 0.9f;
    public bool randomizePitch = true;
    public float pitchRange = 0.06f;        // +/- pitch

    // whether clicks on UI should also trigger (set true to hear clicks when pressing buttons)
    public bool playOnUI = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // if no AudioSource assigned, try to create one
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
    }

    void Update()
    {
        // left mouse click or touchscreen tap (first touch)
        if (Input.GetMouseButtonDown(0))
        {
            // don't play when clicking UI if configured to ignore UI clicks
            if (!playOnUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            PlayClick();
        }

        // touchscreen: alternative - first finger began (optional)
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                if (!playOnUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
                    return;

                PlayClick();
            }
        }
    }

    /// <summary>
    /// play a click sound (callable from UI Button onClick)
    /// </summary>
    public void PlayClick()
    {
        if (sfxSource == null) return;

        AudioClip clip = clickClip;
        float vol = clickVolume;

        if (clickVariations != null && clickVariations.Length > 0)
        {
            // occasionally use the variations set for richer feel
            clip = clickVariations[Random.Range(0, clickVariations.Length)];
            vol = variationVolume;
        }

        if (clip == null) return;

        float oldPitch = sfxSource.pitch;
        if (randomizePitch)
            sfxSource.pitch = 1f + Random.Range(-pitchRange, pitchRange);

        sfxSource.PlayOneShot(clip, vol);

        // restore pitch (PlayOneShot uses current pitch too so we set it back after)
        sfxSource.pitch = oldPitch;
    }
}
