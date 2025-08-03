using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public GameObject Art;

    void Update()
    {
        Art.transform.Rotate(0,1,0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerController playerController))
        {
            playerController.Heal(1);
            Destroy(gameObject);
        }
    }
}
