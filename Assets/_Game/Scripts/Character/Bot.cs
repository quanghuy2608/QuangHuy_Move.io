using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Bot : Character
{
    [Header("AI Navigation")]
    public NavMeshAgent agent;

    private Vector3 destination;
    private IState<Bot> currentState;

    public bool IsDestination => Vector3.Distance(destination, transform.position) < 0.1f;

    public override void OnInit()
    {
        base.OnInit();

        if (agent != null)
        {
            agent.enabled = true;
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        UpdateRotationFromMovement();

        if (currentState != null)
        {
            currentState.OnExecute(this);
        }
    }

    #region Navigation
    public void SetDestination(Vector3 position)
    {
        destination = position;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(position);
        }
    }

    public void MoveStop()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    private void UpdateRotationFromMovement()
    {
        if (agent != null && agent.enabled && agent.velocity.magnitude > 0.1f)
        {
            RotateInMoveDirection(agent.velocity);
        }
    }
    #endregion

    #region State Machine
    public void ChangeState(IState<Bot> state)
    {
        if (currentState != null)
        {
            currentState.OnExit(this);
        }

        currentState = state;

        if (currentState != null)
        {
            currentState.OnEnter(this);
        }
    }
    #endregion

    #region Attack (Public methods for States)
    public Character GetNearestTarget()
    {
        return FindNearestTarget();
    }

    public bool TryAttack(Character target)
    {
        if (CanAttack() && target != null)
        {
            Attack(target);
            return true;
        }
        return false;
    }

    public bool IsInAttackRange(Character target)
    {
        if (target == null || target.IsDead()) return false;
        float distance = Vector3.Distance(transform.position, target.transform.position);
        return distance <= range;
    }
    #endregion

    #region Death
    public override void OnDeath()
    {
        if (isDead)
            return;

        base.OnDeath();

        // Disable NavMeshAgent
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Stop state machine
        ChangeState(null);

        // Despawn after death animation
        StartCoroutine(DespawnAfterDeathAnimation(1.5f));
    }

    private IEnumerator DespawnAfterDeathAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Add coin reward
        DataManager.Instance.AddCoin(1);

        // Notify level manager BEFORE despawn
        LevelManager.Instance.OnBotDeath(this);

        // Return to pool
        SimplePool.Despawn(this);
    }
    #endregion
}