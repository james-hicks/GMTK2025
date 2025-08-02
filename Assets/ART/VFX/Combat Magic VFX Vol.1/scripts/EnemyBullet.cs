using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public GameObject impactPrefab;  // Reference to the impact prefab

    private void Update()
    {
        transform.Translate(new Vector3(0.0f, 0.0f, 0.1f));
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the bullet hits the player
        if (other.TryGetComponent(out PlayerController cont))
        {
            cont.TakeDamage(1);  // Deal damage to the player
        }

        // Avoid impact if the bullet hits another enemy
        if (other.GetComponent<Enemy>() != null)
        {
            return;
        }

        // If we hit something, spawn the impact effect at the point of collision
        if (impactPrefab != null)
        {
            Instantiate(impactPrefab, transform.position, Quaternion.identity);  // Spawn the impact prefab
        }

        // Destroy the bullet
        Destroy(this.gameObject);
    }
}