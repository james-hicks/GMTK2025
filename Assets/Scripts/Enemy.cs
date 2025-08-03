using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Android;

public class Enemy : MonoBehaviour, IHitable
{
    [Header("Enemy Stats")]
    public float HP = 10f;
    public float viewRange = 20f;
    public float attackRange = 7f;

    public GameObject HurtBox;

    protected bool dead = false;
    protected Animator animator;
    protected NavMeshAgent agent;
    protected Transform player;

    protected float hitCooldown;
    protected Coroutine currentState;
    public string CurrentStateName;

    public AudioClip attackSound;

    public System.Action OnDeath;

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        player = FindFirstObjectByType<PlayerController>().transform;
        agent = GetComponent<NavMeshAgent>();

        SwitchState(IdleState());
    }

    public void SwitchState(IEnumerator newState)
    {
        if (currentState != null)
            StopCoroutine(currentState);

        currentState = StartCoroutine(newState);
    }

    // ----------------------
    // CORE STATES
    // ----------------------

    public virtual IEnumerator IdleState()
    {
        CurrentStateName = "Idle";
        animator.SetBool("Move", false);

        while (true)
        {
            if (Vector3.Distance(transform.position, player.position) < viewRange)
                SwitchState(ChaseState());

            yield return null;
        }
    }

    public virtual IEnumerator ChaseState()
    {
        CurrentStateName = "Chase";
        animator.SetBool("Spin", false);
        animator.SetBool("Move", true);

        while (true)
        {
            agent.isStopped = false;
            agent.speed = 7f;
            agent.SetDestination(player.position);

            if (Vector3.Distance(transform.position, player.position) < attackRange)
            {
                SwitchState(AttackState());
                yield break;
            }


            yield return null;
        }
    }

    public virtual IEnumerator AttackState() // <-- This can be overridden
    {
        CurrentStateName = "Attack";
        agent.isStopped = true;
        animator.SetBool("Spin", true);
        animator.SetBool("Move", false);

        if(HurtBox != null)
        {
            GetComponentInChildren<AudioSource>().PlayOneShot(attackSound, 0.5f);
        }

        while (true)
        {
            agent.isStopped = false;
            agent.speed = 12f;

            yield return new WaitForSeconds(2f);
            SwitchState(ChaseState());
            yield break;
        }


    }

    public virtual IEnumerator HitState()
    {
        CurrentStateName = "Hit";
        animator.SetTrigger("Hit");

        agent.ResetPath();
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        animator.SetBool("Spin", false);
        animator.SetBool("Move", false);

        while (hitCooldown > 0)
        {
            hitCooldown -= Time.deltaTime;
            yield return null;
        }

        SwitchState(IdleState());
    }

    // ----------------------
    // DAMAGE / DEATH
    // ----------------------
    public virtual void GetHit(int damage)
    {
        if (dead || hitCooldown > 0) return;

        if(HurtBox != null) HurtBox.SetActive(false);

        HP -= damage;
        hitCooldown = 0.5f;

        if (HP <= 0f)
            Die();
        else
            SwitchState(HitState());
    }

    protected virtual void Die()
    {
        gameObject.layer = LayerMask.NameToLayer("EnemyImmune");
        GetComponentInChildren<AudioSource>().Play();
        StopAllCoroutines();
        OnDeath?.Invoke();


        agent.isStopped = true;
        dead = true;
        animator.SetBool("Death", true);


        Destroy(gameObject, 5f);
    }

    protected virtual void Update()
    {
        if (hitCooldown > 0)
            hitCooldown -= Time.deltaTime;
    }
}
