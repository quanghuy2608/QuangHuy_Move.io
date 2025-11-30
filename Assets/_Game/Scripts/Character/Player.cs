using System.Collections;
using UnityEngine;

public class Player : Character
{
    [Header("Range Circle Visual")]
    [SerializeField] private Color circleColor = Color.red;
    [SerializeField] private float circleWidth = 0.1f;
    [SerializeField] private int segmentCount = 100;

    private LineRenderer lineRenderer;
    private bool isMoving = false;
    private bool isAttackingTarget = false;

    void Start()
    {
        OnInit();
        SetupLineRenderer();
        DrawCircle();
    }

    void Update()
    {
        if (isDead)
            return;

        HandleInput();
        UpdateCirclePosition();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButton(0))
        {
            HandleMovement();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            HandleAttackOrIdle();
        }
        else
        {
            if (!isMoving && !isAttackingTarget)
            {
                ChangeAnim("idle");
            }
        }
    }

    private void HandleMovement()
    {
        isMoving = true;
        isAttackingTarget = false;

        Vector3 nextPoint = JoystickControl.direct * moveSpeed * Time.deltaTime + transform.position;
        Vector3 checkedPoint = CheckGround(nextPoint);
        transform.position = checkedPoint;

        if (JoystickControl.direct != Vector3.zero)
        {
            RotateInMoveDirection(JoystickControl.direct);
            ChangeAnim("run");
        }
    }

    private void HandleAttackOrIdle()
    {
        isMoving = false;

        if (CanAttack())
        {
            Character target = FindNearestTarget();
            if (target != null)
            {
                isAttackingTarget = true;
                Attack(target);
                StartCoroutine(ReturnToIdleAfterAttack(0.5f));
            }
            else
            {
                isAttackingTarget = false;
                ChangeAnim("idle");
            }
        }
        else
        {
            isAttackingTarget = false;
            ChangeAnim("idle");
        }
    }

    private IEnumerator ReturnToIdleAfterAttack(float attackAnimDuration)
    {
        yield return new WaitForSeconds(attackAnimDuration);

        if (!isMoving && !isDead)
        {
            isAttackingTarget = false;
            ChangeAnim("idle");
        }
    }

    private void SetupLineRenderer()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.startWidth = circleWidth;
        lineRenderer.endWidth = circleWidth;
        lineRenderer.positionCount = segmentCount + 1;
        lineRenderer.useWorldSpace = true; 
        lineRenderer.loop = true;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = circleColor;
        lineRenderer.endColor = circleColor;
    }

    private void DrawCircle()
    {
        float angle = 0f;
        float angleStep = 360f / segmentCount;

        Vector3 center = transform.position;
        center.y = 0.1f; 

        for (int i = 0; i <= segmentCount; i++)
        {
            float rad = Mathf.Deg2Rad * angle;
            float x = Mathf.Cos(rad) * range;
            float z = Mathf.Sin(rad) * range;

            lineRenderer.SetPosition(i, center + new Vector3(x, 0, z));
            angle += angleStep;
        }
    }

    private void UpdateCirclePosition()
    {

        Vector3 pos = transform.position;
        pos.y = 0.1f;
        transform.position = pos;

        DrawCircle();
    }

    protected override void OnGrowthUpdated()
    {
        base.OnGrowthUpdated();

        DrawCircle();
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = circleWidth * size;
            lineRenderer.endWidth = circleWidth * size;
        }
    }


    public override void OnDeath()
    {
        if (isDead)
            return;

        isMoving = false;
        isAttackingTarget = false;

        base.OnDeath();
        LevelManager.Instance.OnPlayerDeath();
    }
}