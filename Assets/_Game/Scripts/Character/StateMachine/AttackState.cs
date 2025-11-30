using System.Collections;
using UnityEngine;

public class AttackState : IState<Bot>
{
    private float checkTargetTimer = 0f;
    private float checkTargetInterval = 0.3f;

    private bool hasAttacked = false;
    private float postAttackTimer = 0f;
    private float postAttackDuration = 0f;

    public void OnEnter(Bot bot)
    {
        bot.MoveStop();
        hasAttacked = false;
        postAttackTimer = 0f;
        checkTargetTimer = 0f;
    }

    public void OnExecute(Bot bot)
    {
        if (hasAttacked)
        {
            postAttackTimer += Time.deltaTime;

            if (postAttackTimer >= postAttackDuration)
            {
                bot.ChangeState(new PatrolState());
            }
            return;
        }

        checkTargetTimer += Time.deltaTime;
        if (checkTargetTimer >= checkTargetInterval)
        {
            checkTargetTimer = 0f;

            Character target = bot.GetNearestTarget();

            if (target == null || !bot.IsInAttackRange(target))
            {
                bot.ChangeState(new PatrolState());
                return;
            }

            if (bot.TryAttack(target))
            {
                hasAttacked = true;
                postAttackTimer = 0f;

                if (Random.value < 0.5f)
                {
                    postAttackDuration = Random.Range(1f, 2f);

                }
                else
                {
                    postAttackDuration = 0f;
                }
            }
        }
    }

    public void OnExit(Bot bot)
    {
        if (bot.agent != null && bot.agent.enabled)
        {
            bot.agent.isStopped = false;
        }
    }
}