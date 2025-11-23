using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AdCloseBurstUI : MonoBehaviour
{
    [Header("Particle Settings")]
    public Image particleTemplate;
    public int particleCount = 18;
    public float speedMin = 200f;
    public float speedMax = 450f;
    public float gravity = -900f;
    public float lifetime = 0.7f;

    class Particle
    {
        public RectTransform rt;
        public Vector2 velocity;
        public float age;
        public Image img;
    }

    readonly List<Particle> particles = new List<Particle>();
    float timeAlive = 0f;

    void Start()
    {
        if (particleTemplate == null)
        {
            Debug.LogWarning("AdCloseBurstUI: no particleTemplate assigned.");
            Destroy(gameObject);
            return;
        }

        // hide template itself, we'll clone it
        particleTemplate.gameObject.SetActive(false);

        for (int i = 0; i < particleCount; i++)
        {
            Image img = Instantiate(particleTemplate, transform);
            img.gameObject.SetActive(true);

            RectTransform rt = img.rectTransform;
            rt.anchoredPosition = Vector2.zero;

            // start mostly upwards so they pop then fall
            float angle = Random.Range(-Mathf.PI, 0f); // -180° to 0° (upwards hemisphere)
            float speed = Random.Range(speedMin, speedMax);
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 vel = dir * speed;

            particles.Add(new Particle
            {
                rt = rt,
                velocity = vel,
                age = 0f,
                img = img
            });
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        timeAlive += dt;

        foreach (var p in particles)
        {
            p.age += dt;

            // apply gravity
            p.velocity.y += gravity * dt;

            // move
            p.rt.anchoredPosition += p.velocity * dt;

            // fade out over lifetime
            float t = Mathf.Clamp01(p.age / lifetime);
            Color c = p.img.color;
            c.a = 1f - t;
            p.img.color = c;
        }

        if (timeAlive >= lifetime)
        {
            // clean up spawned images and this effect root
            foreach (var p in particles)
            {
                if (p.rt != null)
                    Destroy(p.rt.gameObject);
            }

            Destroy(gameObject);
        }
    }
}
