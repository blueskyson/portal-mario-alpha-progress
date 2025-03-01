using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 5f;
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Events")]
    public UnityEvent onDeath;

    private float currentHealth;
    private Vector2 startPosition;
    private bool movingRight = true;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        startPosition = transform.position;
        if (transform.Find("HeadTrigger") == null)
        {
            GameObject headTrigger = new GameObject("HeadTrigger");
            headTrigger.transform.SetParent(transform);
            headTrigger.transform.localPosition = new Vector3(0, 1, 0);
            headTrigger.AddComponent<BoxCollider2D>().isTrigger = true;
            headTrigger.AddComponent<HeadTrigger>();
        }
    }

    protected virtual void Update()
    {
        if (!IsGrounded())
        {
            // Don't move if we're not on ground
            return;
        }

        // Handle patrol movement
        Patrol();
    }

    private void Patrol()
    {
        // Calculate patrol boundaries
        float leftBoundary = startPosition.x - patrolDistance;
        float rightBoundary = startPosition.x + patrolDistance;

        // Check if we need to turn around
        if (movingRight && transform.position.x >= rightBoundary)
        {
            movingRight = false;
            FlipSprite();
        }
        else if (!movingRight && transform.position.x <= leftBoundary)
        {
            movingRight = true;
            FlipSprite();
        }

        // Move in current direction
        float moveDirection = movingRight ? 1 : -1;
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
    }

    private void FlipSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );
        return hit.collider != null;
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // Visual feedback
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected System.Collections.IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    protected void Die()
    {
        onDeath?.Invoke();
        
        // Add death animation or particle effects here
        
        // Destroy the enemy
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize patrol area
        Gizmos.color = Color.yellow;
        Vector2 center = Application.isPlaying ? startPosition : (Vector2)transform.position;
        Gizmos.DrawWireCube(center + Vector2.up, new Vector3(patrolDistance * 2, 0.1f, 0));

        // Visualize ground check
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}