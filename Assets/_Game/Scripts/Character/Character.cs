using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : GameUnit
{
    [Header("Movement")]
    [SerializeField] public Transform skin;
    [SerializeField] public float moveSpeed = 5;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animation")]
    [SerializeField] protected Animator anim;

    [Header("Stats")]
    public float size = 1f;
    public float range = 7f;

    [Header("Growth Settings")]
    [SerializeField] private float baseSize = 1f;
    [SerializeField] private float baseRange = 7f;
    [SerializeField] private float sizeGrowthPerKill = 0.2f;
    [SerializeField] private float rangeGrowthPerKill = 1f;

    [Header("Attack Settings")]
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected float attackCooldown = 2f;
    [SerializeField] protected Transform weaponHold;
    [SerializeField] protected PoolType weaponType = PoolType.Axe;

    protected string currentAnimName;
    protected int killCount = 0;
    protected bool isDead = false;
    protected List<WeaponProjectile> activeWeapons = new List<WeaponProjectile>();
    protected float lastAttackTime;

    public virtual void OnInit()
    {
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }

        isDead = false;
        killCount = 0;
        size = baseSize;
        range = baseRange;
        transform.localScale = Vector3.one * size;
        activeWeapons.Clear();
        lastAttackTime = -attackCooldown;

        if (weaponHold != null)
        {
            weaponHold.gameObject.SetActive(true);
        }
    }

    public Vector3 CheckGround(Vector3 nextPoint)
    {
        RaycastHit hit;
        if (Physics.Raycast(nextPoint, Vector3.down, out hit, 2f, groundLayer))
        {
            return hit.point + Vector3.up * 1.1f;
        }
        return transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            //xu li weapon voi obstacle 
        }
    }

    public void OnKill()
    {
        killCount++;
        size = baseSize + (killCount * sizeGrowthPerKill);
        range = baseRange + (killCount * rangeGrowthPerKill);
        transform.localScale = Vector3.one * size;

        OnGrowthUpdated();
    }

    protected virtual void OnGrowthUpdated()
    {
    }
    protected Character FindNearestTarget()
    {
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, range);

        Character nearestTarget = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider col in targetsInRange)
        {
            if (col.GetComponent<WeaponProjectile>() != null)
                continue;

            Character character = col.GetComponent<Character>();

            if (character == null || character.IsDead() || character == this)
                continue;
            if (targetLayer != (targetLayer | (1 << col.gameObject.layer)))
                continue;

            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance > range)
                continue;

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTarget = character;
            }
        }

        return nearestTarget;
    }

    public bool CanAttack()
    {
        return !isDead && Time.time >= lastAttackTime + attackCooldown;
    }

    public virtual void Attack(Character target)
    {
        if (target == null || !CanAttack())
            return;

        lastAttackTime = Time.time;
        ChangeAnim("attack");
        AudioManager.Instance.PlaySFX(SoundType.ATTACK);
        RotateToward(target.transform.position);
        ThrowWeapon(target.transform.position);
    }

    protected void ThrowWeapon(Vector3 targetPosition)
    {
        if (weaponHold != null)
        {
            weaponHold.gameObject.SetActive(false);
        }

        Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
        WeaponProjectile projectile = SimplePool.Spawn<WeaponProjectile>(weaponType, spawnPos, Quaternion.identity);

        if (projectile != null)
        {
            projectile.OnInit(targetPosition, this);
            RegisterWeapon(projectile);
            StartCoroutine(ShowWeaponAfterDelay(0.5f));
        }
    }

    private IEnumerator ShowWeaponAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (weaponHold != null && !isDead)
        {
            weaponHold.gameObject.SetActive(true);
        }
    }

    protected void RotateToward(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Keep on horizontal plane

        if (direction != Vector3.zero && skin != null)
        {
            skin.forward = direction;
        }
    }

    protected void RotateInMoveDirection(Vector3 moveDirection)
    {
        moveDirection.y = 0;
        if (moveDirection != Vector3.zero && skin != null)
        {
            skin.forward = moveDirection;
        }
    }

    public void RegisterWeapon(WeaponProjectile weapon)
    {
        if (!activeWeapons.Contains(weapon))
        {
            activeWeapons.Add(weapon);
        }
    }

    public void UnregisterWeapon(WeaponProjectile weapon)
    {
        activeWeapons.Remove(weapon);
    }

    protected void DestroyAllActiveWeapons()
    {
        List<WeaponProjectile> weaponsToDestroy = new List<WeaponProjectile>(activeWeapons);
        foreach (WeaponProjectile weapon in weaponsToDestroy)
        {
            if (weapon != null)
            {
                SimplePool.Despawn(weapon);
            }
        }
        activeWeapons.Clear();
    }
    public bool IsDead()
    {
        return isDead;
    }

    public void ChangeAnim(string animName)
    {
        if (currentAnimName != animName)
        {
            anim.ResetTrigger(animName);
            currentAnimName = animName;
            anim.SetTrigger(currentAnimName);
        }
    }
    public virtual void OnDeath()
    {
        if (isDead)
            return;

        isDead = true;
        DestroyAllActiveWeapons();
        ChangeAnim("dead");
        AudioManager.Instance.PlaySFX(SoundType.DEATH);
    }

    public virtual void OnHit()
    {
        AudioManager.Instance.PlaySFX(SoundType.HIT);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}