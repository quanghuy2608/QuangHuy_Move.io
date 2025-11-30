using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WeaponProjectile : GameUnit
{
    [SerializeField] private Transform TF;
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float rotateSpeed = 720f;
    [SerializeField] private float maxDistance = 20f;

    private Vector3 startPosition;
    private Vector3 direction;
    private bool isFlying = false;
    private Character owner; // Nguoi ban weapon nay

    public void OnInit(Vector3 targetPosition, Character owner = null)
    {
        this.owner = owner;
        startPosition = TF.position;

        // Chi tinh huong bay tren mat phang XZ (ngang), bo qua Y
        Vector3 flatStart = new Vector3(startPosition.x, 0, startPosition.z);
        Vector3 flatTarget = new Vector3(targetPosition.x, 0, targetPosition.z);
        direction = (flatTarget - flatStart).normalized;
        TF.rotation = Quaternion.Euler(90 ,0,0);
        // Dam bao weapon bay ngang (Y = 0)
        direction.y = 0;

        isFlying = true;
    }

    void Update()
    {
        if (!isFlying)
            return;

        // Bay thang theo huong ban dau
        TF.position += direction * moveSpeed * Time.deltaTime;

        // BO XOAY - chi bay thang
        TF.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime, Space.Self);

        // Kiem tra khoang cach da bay
        float distanceTraveled = Vector3.Distance(startPosition, TF.position);
        if (distanceTraveled >= maxDistance)
        {
            OnDespawn();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isFlying)
            return;

        // Kiem tra va cham voi Player
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            // Khong tu ban minh
            if (owner != null && owner.gameObject == player.gameObject)
                return;

            // Kiem tra player da chet chua
            if (player.IsDead())
                return;

            // Owner ghi nhan kill
            if (owner != null)
            {
                owner.OnKill();
            }

            player.OnDeath();
            OnDespawn();
            return;
        }

        // Kiem tra va cham voi Bot
        Bot bot = other.GetComponent<Bot>();
        if (bot != null)
        {
            // Khong tu ban minh
            if (owner != null && owner.gameObject == bot.gameObject)
                return;

            // Kiem tra bot da chet chua
            if (bot.IsDead())
                return;

            // Owner ghi nhan kill
            if (owner != null)
            {
                owner.OnKill();
            }

            bot.OnDeath();
            OnDespawn();
            return;
        }

        // Kiem tra va cham voi Obstacle
        if (other.CompareTag("Obstacle"))
        {
            
            OnDespawn();
            return;
        }
    }

    private void OnDespawn()
    {
        isFlying = false;

        // Unregister khoi owner
        if (owner != null)
        {
            owner.UnregisterWeapon(this);
        }

        owner = null;
        SimplePool.Despawn(this);
    }
}