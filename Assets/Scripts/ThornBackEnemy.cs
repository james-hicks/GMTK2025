using System.Collections;
using UnityEngine;

public class ThornBackEnemy : Enemy
{


    public override IEnumerator AttackState()
    {
        CurrentStateName = "Attack (Ranged)";
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        animator.SetBool("Spin", false);
        animator.SetBool("Move", false);


        while (true)
        {
            // Face the player
            Vector3 lookDir = (player.position - transform.position).normalized;
            lookDir.y = 0f;
            transform.forward = lookDir;

            animator.SetTrigger("Attack");

            // Wait between attacks
            yield return new WaitForSeconds(2f);

            // Exit if player moves out of range
            if (Vector3.Distance(transform.position, player.position) > attackRange + 2)
            {
                SwitchState(ChaseState());
                yield break;
            }


        }
    }
}
