using UnityEngine;
using System.Collections;

/// <summary>
/// 기본 엔티티 클래스. 물리적인 속성을 담당.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseEntity : MonoBehaviour
{
    [Header("Entity Containers")]
    [SerializeField] protected VisualContainer visualContainer;
    [SerializeField] protected PhysicsContainer physicsContainer;
    public VisualContainer Visual => visualContainer;
    public PhysicsContainer Physics => physicsContainer;

    [Header("Entity Properties")]
    public string entityID;

    [Header("Physics Components (Root)")]
    protected Rigidbody2D entityRigidbody;
    public Rigidbody2D EntityRigidbody => entityRigidbody;

    protected bool facingRight = true;
    public bool FacingRight => facingRight;

    protected virtual void Awake()
    {
        InitializeComponents();
        InitializeContainers();
        Initialize();
    }

    protected virtual void InitializeComponents()
    {
        entityRigidbody = GetComponent<Rigidbody2D>();
        if (entityRigidbody == null)
        {
            Debug.LogError($"{gameObject.name}: Rigidbody2D component is required on Entity root!");
        }
    }

    protected virtual void InitializeContainers()
    {
        if (visualContainer == null)
            visualContainer = GetComponentInChildren<VisualContainer>();

        if (physicsContainer == null)
            physicsContainer = GetComponentInChildren<PhysicsContainer>();

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

    protected virtual void Initialize(){}

    // Physics Utility Methods
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

    public virtual void IgnorePlatform()
    {
        if (entityRigidbody == null) return;
        
        if (IsGrounded && IsOnPlatform)
        {
            StartCoroutine(TemporaryIgnorePlatform());
        }
    }

    private IEnumerator TemporaryIgnorePlatform()
    {
        Collider2D entityCollider = physicsContainer.MainCollider;
        entityCollider.enabled = false;

        yield return new WaitForSeconds(0.2f);

        entityCollider.enabled = true;
    }
    
    // Visual Utility Methods
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

    public void Flip()
    {
        facingRight = !facingRight;

        if (visualContainer != null)
        {
            Vector3 scale = visualContainer.transform.localScale;
            scale.x *= -1;
            visualContainer.transform.localScale = scale;
        }
    }

    // Collision Detection Methods
    public bool IsGrounded => physicsContainer?.IsGrounded() ?? false;
    public bool IsOnPlatform => physicsContainer?.IsOnPlatform() ?? false;
    public bool IsWallLeft => physicsContainer?.IsWallLeft() ?? false;
    public bool IsWallRight => physicsContainer?.IsWallRight() ?? false;
    public bool IsWall(float direction) => physicsContainer?.IsWall(direction) ?? false; //TODO 왜 얘만 direction 필요함?
    public bool IsCeiling => physicsContainer?.IsCeiling() ?? false;

}
