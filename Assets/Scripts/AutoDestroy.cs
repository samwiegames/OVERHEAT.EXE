using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 0.6f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
