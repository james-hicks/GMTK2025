using UnityEngine;

public class EnemyHurtBox : MonoBehaviour
{
    public int Damage = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController cont))
        {
            cont.TakeDamage(Damage);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.TryGetComponent(out PlayerController cont))
        {
            cont.TakeDamage(Damage);
        }
    }
}
