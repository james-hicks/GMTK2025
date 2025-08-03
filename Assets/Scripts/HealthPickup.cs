using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public GameObject Art;

    public bool pickedUp = false;
    public AudioClip clip;

    void Update()
    {
        Art.transform.Rotate(0,1,0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerController playerController) && !pickedUp)
        {
            pickedUp = true;
            playerController.Heal(1);
            playerController.GetComponentInChildren<AudioSource>().PlayOneShot(clip, 0.4f);
            Destroy(gameObject);
        }
    }
}
