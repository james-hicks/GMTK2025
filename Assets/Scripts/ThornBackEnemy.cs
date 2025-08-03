using System.Collections;
using UnityEngine;

public class ThornBackEnemy : Enemy
{
    [SerializeField] private LayerMask lineOfSightMask;

    public override IEnumerator AttackState()
    {
        CurrentStateName = "Attack (Ranged)";

        // Only reset once
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;

        animator.SetBool("Spin", false);
        animator.SetBool("Move", false);

        float attackCooldown = 2f;
        float cooldownTimer = 0f;

        while (true)
        {
            if (player == null)
            {
                SwitchState(IdleState());
                yield break;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > attackRange + 2f)
            {
                SwitchState(ChaseState());
                yield break;
            }

            // LOS check
            Vector3 origin = transform.position + Vector3.up * 1.2f;
            Vector3 direction = (player.position + Vector3.up * 1f - origin).normalized;

            Debug.DrawRay(origin, direction * attackRange, Color.red, 0.1f);

            bool hasLOS = false;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, attackRange + 2f, lineOfSightMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.GetComponent<PlayerController>() != null)
                    hasLOS = true;
            }

            if (!hasLOS)
            {
                SwitchState(ChaseState());
                yield break;
            }

            // Face player
            Vector3 lookDir = (player.position - transform.position).normalized;
            lookDir.y = 0f;
            transform.forward = lookDir;

            // Attack cooldown
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                animator.SetTrigger("Attack");
                cooldownTimer = attackCooldown;
            }

            yield return null;

            if (!hasLOS)
            {
                Debug.Log("LOS blocked — switching to Chase");
                SwitchState(ChaseState());
                yield break;
            }
            if (lineOfSightMask == 0)
            {
                Debug.LogWarning("Line of Sight Mask is not set on " + name);
            }
        }
    }

}
