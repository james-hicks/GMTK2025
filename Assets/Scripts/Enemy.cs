using UnityEngine;
using UnityEngine.Rendering.UI;

public class Enemy : MonoBehaviour, IHitable
{
    public float HP;

    private bool dead = false;  
    private Animator animator;

    private float hitCooldown;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void GetHit(int damage)
    {
        if (dead || hitCooldown > 0) return;

        hitCooldown = 0.5f;
        HP -= damage;

        if (HP <= 0f)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Hit");
        }
    }

    private void Update()
    {
        if (hitCooldown > 0f)
        {
            hitCooldown -= Time.deltaTime;
        }
    }

    private void Die()
    {
        dead = true;
        animator.SetBool("Death", true);
        Destroy(gameObject, 5f);
    }
}
