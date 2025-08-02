using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour {

	private void Update () 
    {
        transform.Translate(new Vector3(0.0f, 0.0f, 0.1f));
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerController cont))
        {
            cont.TakeDamage(1);
        }

        if(other.GetComponent<Enemy>() != null)
        {
            return;
        }

        Destroy(this.gameObject);
    }
}
