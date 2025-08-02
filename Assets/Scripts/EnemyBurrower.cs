using UnityEngine;
using System.Collections;

public class EnemyBurrower : Enemy
{

    public bool isBurrowed = false;

    public GameObject diveFXPrefab;
    public Transform diveFXSpawnPoint;

    public void PlayDiveFX()
    {
        if (diveFXPrefab && diveFXSpawnPoint)
        {
            Instantiate(diveFXPrefab, diveFXSpawnPoint.position, diveFXSpawnPoint.rotation);
        }
    }

    public void EnableBurrowImmunity()
    {
        gameObject.layer = LayerMask.NameToLayer("EnemyImmune");
    }

    public void DisableBurrowImmunity()
    {
        gameObject.layer = LayerMask.NameToLayer("Enemy");
    }

    public override IEnumerator ChaseState()
    {
        CurrentStateName = "Chase";
        animator.SetBool("Burrow", true);
        animator.SetBool("Move", true);
        isBurrowed = true;

        while (true)
        {
            agent.isStopped = false;
            agent.speed = 5f;
            agent.SetDestination(player.position);

            if (Vector3.Distance(transform.position, player.position) < attackRange)
            {
                SwitchState(AttackState());
                yield break;
            }

            yield return null;
        }
    }

    public override IEnumerator AttackState()
    {
        CurrentStateName = "Attack";
        agent.isStopped = true;
        animator.SetBool("Burrow", false);
        animator.SetBool("Move", false);
        isBurrowed = false;

        yield return new WaitForSeconds(1.8f);

        SwitchState(BurrowRetreatState());
    }

    private IEnumerator BurrowRetreatState()
    {
        CurrentStateName = "Retreat";

        // Start burrow
        animator.SetBool("Burrow", true);
        animator.SetBool("Move", true);
        isBurrowed = true;

        // Move to a random position away from player
        Vector3 retreatDirection = (transform.position - player.position).normalized;
        Vector3 retreatTarget = transform.position + retreatDirection * 10f; // move 10 units away

        agent.isStopped = false;
        agent.speed = 6f;
        agent.SetDestination(retreatTarget);

        // Wait until it reaches near destination
        while (Vector3.Distance(transform.position, retreatTarget) > 1f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // delay before resuming

        SwitchState(ChaseState());
    }

    public override void GetHit(int damage)
    {
        if (dead || hitCooldown > 0 || isBurrowed) return;

        HP -= damage;
        hitCooldown = 0.5f;

        if (HP <= 0f)
            Die();
        else
            SwitchState(HitState());
    }
}