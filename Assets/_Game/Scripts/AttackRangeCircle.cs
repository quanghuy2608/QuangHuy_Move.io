using UnityEngine;

public class AttackRangeCircle : MonoBehaviour
{
    [Header("Attack Range Settings")]
    [SerializeField] private float attackRange = 7f;

    [Header("Circle Visual Settings")]
    [SerializeField] private Color circleColor = Color.red;
    [SerializeField] private float circleWidth = 0.1f;
    [SerializeField] private int segmentCount = 100; // Số đoạn thẳng tạo thành vòng tròn (càng nhiều càng mượt)

    [Header("Attack Settings")]
    [SerializeField] private LayerMask enemyLayer; // Layer của bot/enemy
    [SerializeField] private float attackCooldown = 1f; // Thời gian giữa các đợt tấn công

    private LineRenderer lineRenderer;
    private float lastAttackTime;
    private bool isMoving = false;

    void Start()
    {
        SetupLineRenderer();
        DrawCircle();
    }

    void Update()
    {
        // Cập nhật vị trí vòng tròn theo player
        UpdateCirclePosition();

        // Kiểm tra và tấn công nếu đủ điều kiện
        if (!isMoving && Time.time >= lastAttackTime + attackCooldown)
        {
            TryAttackNearestEnemy();
        }
    }

    private void SetupLineRenderer()
    {
        // Tạo LineRenderer component
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Cấu hình LineRenderer
        lineRenderer.startWidth = circleWidth;
        lineRenderer.endWidth = circleWidth;
        lineRenderer.positionCount = segmentCount + 1;
        lineRenderer.useWorldSpace = false; // Sử dụng local space để di chuyển theo player
        lineRenderer.loop = true;

        // Thiết lập material và màu
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = circleColor;
        lineRenderer.endColor = circleColor;
    }

    private void DrawCircle()
    {
        float angle = 0f;
        float angleStep = 360f / segmentCount;

        for (int i = 0; i <= segmentCount; i++)
        {
            float rad = Mathf.Deg2Rad * angle;

            // Vẽ trên mặt phẳng XZ (Y = 0)
            float x = Mathf.Cos(rad) * attackRange;
            float z = Mathf.Sin(rad) * attackRange;

            lineRenderer.SetPosition(i, new Vector3(x, 0, z));

            angle += angleStep;
        }
    }

    private void UpdateCirclePosition()
    {
        // Vòng tròn sẽ tự động theo player vì đang dùng local space
        // Nhưng đảm bảo Y luôn ở độ cao mong muốn (ví dụ: sát mặt đất)
        transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
    }

    private void TryAttackNearestEnemy()
    {
        // Tìm tất cả enemy trong phạm vi tấn công
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        if (enemiesInRange.Length > 0)
        {
            // Tìm enemy gần nhất
            Collider nearestEnemy = null;
            float minDistance = Mathf.Infinity;

            foreach (Collider enemy in enemiesInRange)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            if (nearestEnemy != null)
            {
                Attack(nearestEnemy.gameObject);
            }
        }
    }

    private void Attack(GameObject target)
    {
        lastAttackTime = Time.time;

        // TODO: Thực hiện logic tấn công của bạn ở đây
        Debug.Log($"Attacking: {target.name} at distance: {Vector3.Distance(transform.position, target.transform.position)}");

        // Ví dụ: Gây sát thương
        // Character targetChar = target.GetComponent<Character>();
        // if (targetChar != null)
        // {
        //     targetChar.TakeDamage(attackDamage);
        // }
    }

    // Hàm này nên được gọi từ script di chuyển của player
    public void SetMoving(bool moving)
    {
        isMoving = moving;
    }

    // Các hàm tiện ích để thay đổi settings runtime
    public void SetAttackRange(float range)
    {
        attackRange = range;
        DrawCircle();
    }

    public void SetCircleColor(Color color)
    {
        circleColor = color;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    public void SetCircleWidth(float width)
    {
        circleWidth = width;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
    }

    // Debug: Hiển thị phạm vi tấn công trong Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}