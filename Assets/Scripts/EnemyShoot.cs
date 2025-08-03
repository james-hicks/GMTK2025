using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;

    public AudioClip shootSound;
    public void Shoot()
    {
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        GetComponentInChildren<AudioSource>().PlayOneShot(shootSound);
        Destroy(bullet, 10f);
    }
}
