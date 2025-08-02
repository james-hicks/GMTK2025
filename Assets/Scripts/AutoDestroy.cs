using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 2f; // How long before it destroys itself

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
