using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class RabbitAI2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.2f;

    [Header("Idle Time")]
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 3f;

    [Header("Move Time")]
    [SerializeField] private float minMoveTime = 1f;
    [SerializeField] private float maxMoveTime = 3f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private Vector2 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Game top-down
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        StartCoroutine(AIBehaviour());
    }

    private IEnumerator AIBehaviour()
    {
        while (true)
        {
            //==========================
            // IDLE
            //==========================

            rb.linearVelocity = Vector2.zero;

            if (animator != null)
                animator.SetFloat("Speed", 0f);

            yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));

            //==========================
            // MOVE
            //==========================

            moveDirection = Random.insideUnitCircle.normalized;

            float moveTime = Random.Range(minMoveTime, maxMoveTime);
            float timer = 0f;

            if (animator != null)
                animator.SetFloat("Speed", 1f);

            while (timer < moveTime)
            {
                rb.linearVelocity = moveDirection * moveSpeed;

                // Sprite gốc nhìn sang TRÁI
                if (moveDirection.x > 0.05f)
                {
                    // Đi sang phải
                    spriteRenderer.flipX = true;
                }
                else if (moveDirection.x < -0.05f)
                {
                    // Đi sang trái
                    spriteRenderer.flipX = false;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.001f)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
    }
}