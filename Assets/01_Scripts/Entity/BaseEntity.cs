// Assets/01_Scripts/Entity/BaseEntity.cs
using UnityEngine;

public abstract class BaseEntity : MonoBehaviour
{
    [Header("Core Entity Properties")]
    public string entityID;
    public bool isActive = true;
    
    [Header("Components")]
    protected Transform entityTransform;
    protected Rigidbody2D entityRigidbody;
    protected Collider2D entityCollider;
    protected SpriteRenderer spriteRenderer;
    
    protected virtual void Awake()
    {
        InitializeComponents();
    }
    
    protected virtual void InitializeComponents()
    {
        entityTransform = transform;
        entityRigidbody = GetComponent<Rigidbody2D>();
        entityCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public abstract void Initialize();
}
