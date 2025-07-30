using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseEntity : MonoBehaviour
{
    [Header("Entity Containers")]
    [SerializeField] protected VisualContainer visualContainer;
    [SerializeField] protected PhysicsContainer physicsContainer;

    [Header("Entity Properties")]
    public string entityID;
    public bool isActive = true;

    [Header("Physics Components (Root)")]
    protected Rigidbody2D entityRigidbody;

    // 컨테이너 접근 프로퍼티
    public VisualContainer Visual => visualContainer;
    public PhysicsContainer Physics => physicsContainer;
    public Rigidbody2D EntityRigidbody => entityRigidbody;

    protected virtual void Awake()
    {
        InitializeComponents();
        InitializeContainers();
        Initialize();
    }

    protected virtual void InitializeComponents()
    {
        // Root에서 Rigidbody2D 가져오기
        entityRigidbody = GetComponent<Rigidbody2D>();

        if (entityRigidbody == null)
        {
            Debug.LogError($"{gameObject.name}: Rigidbody2D component is required on Entity root!");
        }
    }

    protected virtual void InitializeContainers()
    {
        // Inspector에서 할당된 것이 우선, null일 때만 찾거나 생성
        if (visualContainer == null)
            visualContainer = GetComponentInChildren<VisualContainer>();

        if (physicsContainer == null)
            physicsContainer = GetComponentInChildren<PhysicsContainer>();

        // 여전히 null이면 생성 (마지막 수단)
        if (visualContainer == null)
        {
            GameObject visualObj = new GameObject("VisualContainer");
            visualObj.transform.SetParent(transform);
            visualObj.transform.localPosition = Vector3.zero;
            visualContainer = visualObj.AddComponent<VisualContainer>();
        }

        if (physicsContainer == null)
        {
            GameObject physicsObj = new GameObject("PhysicsContainer");
            physicsObj.transform.SetParent(transform);
            physicsObj.transform.localPosition = Vector3.zero;
            physicsContainer = physicsObj.AddComponent<PhysicsContainer>();
        }
    }

    public abstract void Initialize();

    // Physics 관련 편의 메서드들 (Root의 Rigidbody2D 사용)
    public Vector2 GetVelocity()
    {
        return entityRigidbody?.linearVelocity ?? Vector2.zero;
    }

    public void SetVelocity(Vector2 velocity)
    {
        if (entityRigidbody != null)
            entityRigidbody.linearVelocity = velocity;
    }

    public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force)
    {
        if (entityRigidbody != null)
            entityRigidbody.AddForce(force, mode);
    }

    // Visual 관련 편의 메서드들
    public void SetSprite(Sprite sprite)
    {
        visualContainer?.SetSprite(sprite);
    }

    public void SetColor(Color color)
    {
        visualContainer?.SetColor(color);
    }

    public void ResetVisualToOriginal()
    {
        visualContainer?.ResetToOriginalValues();
    }

    // 충돌 검사 편의 메서드들
    public bool IsGrounded()
    {
        return physicsContainer?.IsGrounded() ?? false;
    }

    public bool IsWallLeft()
    {
        return physicsContainer?.IsWallLeft() ?? false;
    }

    public bool IsWallRight()
    {
        return physicsContainer?.IsWallRight() ?? false;
    }

    public bool IsWall(float direction)
    {
        return physicsContainer?.IsWall(direction) ?? false;
    }

    public bool IsCeiling()
    {
        return physicsContainer?.IsCeiling() ?? false;
    }
}
