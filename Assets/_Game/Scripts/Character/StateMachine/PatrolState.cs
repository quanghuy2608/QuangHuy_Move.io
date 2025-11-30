using UnityEngine;
using UnityEngine.AI;

public class PatrolState : IState<Bot>
{
    private enum BotState { Idling, Moving, PreparingAttack }
    private BotState currentState;

    private float stateTimer = 0f;
    private float stateDuration = 0f;

    public void OnEnter(Bot bot)
    {
        StartIdling(bot);
    }

    public void OnExecute(Bot bot)
    {
        stateTimer += Time.deltaTime;

        // Kiem tra co target trong tam khong (dung method tu base class)
        Character nearbyTarget = bot.GetNearestTarget();

        if (nearbyTarget != null && bot.IsInAttackRange(nearbyTarget))
        {
            // Co target trong tam
            if (currentState != BotState.PreparingAttack)
            {
                StartPreparingAttack(bot, nearbyTarget);
            }
            else
            {
                // Dang chuan bi tan cong
                if (stateTimer >= stateDuration)
                {
                    bot.ChangeState(new AttackState());
                }
            }
            return;
        }

        // Khong co target -> xu ly idle/move binh thuong
        if (currentState == BotState.Idling)
        {
            if (stateTimer >= stateDuration)
            {
                StartMoving(bot);
            }
        }
        else if (currentState == BotState.Moving)
        {
            if (stateTimer >= stateDuration || bot.IsDestination)
            {
                StartIdling(bot);
            }
        }
    }

    public void OnExit(Bot bot)
    {
    }

    #region State Transitions
    private void StartIdling(Bot bot)
    {
        currentState = BotState.Idling;
        stateTimer = 0f;
        stateDuration = Random.Range(1f, 3f);

        bot.ChangeAnim("idle");
        bot.MoveStop();
    }

    private void StartMoving(Bot bot)
    {
        currentState = BotState.Moving;
        stateTimer = 0f;
        stateDuration = Random.Range(2f, 5f);

        bot.ChangeAnim("run");
        SeekTarget(bot);
    }

    private void StartPreparingAttack(Bot bot, Character target)
    {
        currentState = BotState.PreparingAttack;
        stateTimer = 0f;
        stateDuration = Random.Range(1f, 2f);

        // 50% tiep tuc chay, 50% dung lai
        if (Random.value < 0.5f)
        {
            bot.ChangeAnim("run");
            bot.SetDestination(target.transform.position);
        }
        else
        {
            bot.ChangeAnim("idle");
            bot.MoveStop();
        }
    }
    #endregion

    #region Target Finding
    private void SeekTarget(Bot bot)
    {
        Character nearestCharacter = FindNearestCharacter(bot);

        if (nearestCharacter != null)
        {
            bot.SetDestination(nearestCharacter.transform.position);
        }
        else
        {
            MoveToRandomPoint(bot);
        }
    }

    private Character FindNearestCharacter(Bot bot)
    {
        Character nearestCharacter = null;
        float minDistance = Mathf.Infinity;

        Character[] allCharacters = Object.FindObjectsOfType<Character>();

        foreach (Character character in allCharacters)
        {
            if (character == bot || character.IsDead() || !character.gameObject.activeSelf)
                continue;

            float distance = Vector3.Distance(bot.transform.position, character.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestCharacter = character;
            }
        }

        return nearestCharacter;
    }

    private void MoveToRandomPoint(Bot bot)
    {
        Vector3 randomDirection = Random.insideUnitSphere * Random.Range(5f, 15f);
        randomDirection += bot.transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 15f, NavMesh.AllAreas))
        {
            bot.SetDestination(hit.position);
        }
    }
    #endregion
}