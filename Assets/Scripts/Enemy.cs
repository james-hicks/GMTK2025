using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Android;
using UnityEngine.XR;

public class Enemy : MonoBehaviour, IHitable
{
    [Header("Enemy Stats")]
    public float HP;
    public float viewRange = 20f;
    public float attackRange = 7f;


    private bool dead = false;  
    private Animator animator;

    private float hitCooldown;

    private Coroutine currentState;
    [Space]
    public string CurrentStateName;

    private NavMeshAgent agent;
    private Transform player;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        player = FindFirstObjectByType<PlayerController>().transform;
        agent = GetComponent<NavMeshAgent>();

        SwitchState(IdleState());
    }

    public void SwitchState(IEnumerator newState)
    {
        if(currentState != null)
        {
            StopCoroutine(currentState);
        }
        //Debug.Log("Switching to new State" + newState);

        currentState = StartCoroutine(newState);
    }

    public IEnumerator IdleState()
    {
        CurrentStateName = "Idle";
        animator.SetBool("Move", false);

        while (true)
        {
            if(Vector3.Distance(transform.position, player.position) < viewRange)
            {
                SwitchState(ChaseState());
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator ChaseState()
    {
        CurrentStateName = "Chase";
        animator.SetBool("Spin", false);
        animator.SetBool("Move", true);
        while (true)
        {
            agent.speed = 4f;

            agent.isStopped = false;
            agent.SetDestination(player.position);

            if (Vector3.Distance(transform.position, player.position) < attackRange)
            {
                SwitchState(AttackState());
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator AttackState()
    {
        CurrentStateName = "Attack";
        agent.isStopped = true;
        animator.SetBool("Spin", true);
        animator.SetBool("Move", false);
        while (true)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            agent.speed = 3f;

            if (Vector3.Distance(transform.position, player.position) < 0.5f)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }

            if (Vector3.Distance(transform.position, player.position) > attackRange + 1)
            {
                SwitchState(ChaseState());
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator HitState()
    {
        CurrentStateName = "Hit State";
        animator.SetTrigger("Hit");

        agent.ResetPath();
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        animator.SetBool("Spin", false);
        animator.SetBool("Move", false);

        while (hitCooldown > 0)
        {
            yield return new WaitForEndOfFrame();
        }


        SwitchState(IdleState());
    }

    public void GetHit(int damage)
    {
        if (dead || hitCooldown > 0) return;


        HP -= damage;
        hitCooldown = 0.5f;
        if (HP <= 0f)
        {
            Die();
        }
        else
        {
            Debug.Log("Hit");
            SwitchState(HitState());
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
        StopAllCoroutines();
        agent.isStopped = true;
        dead = true;
        animator.SetBool("Death", true);
        Destroy(gameObject, 5f);
    }
}
