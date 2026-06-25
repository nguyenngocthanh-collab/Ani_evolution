using System.Collections;
using UnityEngine;

public class RabbitAI2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float changeDirectionInterval = 2f;

    [Header("Avoidance Settings")]
    public float detectionRange = 1.5f;
    public float rabbitRadius = 0.4f;
    public LayerMask obstacleLayer;        // BẮT BUỘC: Chọn Layer của Tường/Rìa Map và Layer của Thỏ khác

    [Header("Map Boundaries (Giữ thỏ trong Map)")]
    public bool useMapClamp = true;
    public Vector2 minMapBounds = new Vector2(0f, 0f);
    public Vector2 maxMapBounds = new Vector2(50f, 50f); // Khớp với mapSize của GrassSpawner2D

    private float directionTimer = 0f;
    private bool isEating = false;
    private bool isSpecialActing = false;

    void Start()
    {
        ChooseRandomDirection();
    }

    void Update()
    {
        if (isEating || isSpecialActing) return;

        // 1. Quét tia Raycast để check vật cản / rìa map phía trước và bẻ lái
        RaycastAvoidance2D();

        // SỬA ĐỒI: Di chuyển tịnh tiến chính xác theo hướng transform.up (hướng mặt cục bộ của thỏ)
        transform.position += transform.up * moveSpeed * Time.deltaTime;

        // Giới hạn cứng tọa độ không cho thỏ lọt khỏi map
        if (useMapClamp)
        {
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minMapBounds.x, maxMapBounds.x);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minMapBounds.y, maxMapBounds.y);

            // Nếu chạm rìa map cứng thì tự ép đổi hướng ngay
            if (transform.position.x <= minMapBounds.x || transform.position.x >= maxMapBounds.x ||
                transform.position.y <= minMapBounds.y || transform.position.y >= maxMapBounds.y)
            {
                transform.position = clampedPosition;
                ChooseRandomDirection();
            }
        }

        // 2. Đếm thời gian tự động đổi hướng ngẫu nhiên
        directionTimer += Time.deltaTime;
        if (directionTimer >= changeDirectionInterval)
        {
            ChooseRandomDirection();
        }
    }

    void ChooseRandomDirection()
    {
        directionTimer = 0f;
        float randomAngle = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0, 0, randomAngle);

        if (Random.value < 0.10f)
        {
            StartCoroutine(SpecialBehaviorRoutine());
        }
    }

    void RaycastAvoidance2D()
    {
        Vector2 forward = transform.up;
        Vector2 left = transform.up - transform.right * 0.4f;
        Vector2 right = transform.up + transform.right * 0.4f;

        Vector2 rayOrigin = (Vector2)transform.position + forward * rabbitRadius;

        RaycastHit2D hitForward = Physics2D.Raycast(rayOrigin, forward, detectionRange, obstacleLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(rayOrigin, left, detectionRange, obstacleLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(rayOrigin, right, detectionRange, obstacleLayer);

        // Vẽ tia đỏ kiểm tra hướng đi trực quan ngoài màn hình Scene
        Debug.DrawRay(rayOrigin, forward * detectionRange, Color.red);
        Debug.DrawRay(rayOrigin, left * detectionRange, Color.yellow);
        Debug.DrawRay(rayOrigin, right * detectionRange, Color.yellow);

        if (hitForward.collider != null || hitLeft.collider != null || hitRight.collider != null)
        {
            // Nếu thấy vật cản, quay ngoắt một góc lớn ra phía sau (150 đến 210 độ)
            float turnAngle = Random.Range(150f, 210f);
            transform.Rotate(0, 0, turnAngle);
            directionTimer = 0f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Rabbit") || collision.gameObject.CompareTag("Obstacle"))
        {
            ChooseRandomDirection();
        }
    }

    // Kiểm tra Trigger ăn cỏ
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem có đúng tag "Grass" không
        if (other.CompareTag("Grass") && !isEating)
        {
            StartCoroutine(EatGrassRoutine(other.gameObject));
        }
    }

    IEnumerator EatGrassRoutine(GameObject grass)
    {
        isEating = true;

        // Đợi 2.5 giây giả lập ăn cỏ
        yield return new WaitForSeconds(2.5f);

        if (grass != null)
        {
            Destroy(grass);
        }

        isEating = false;
        ChooseRandomDirection();
    }

    IEnumerator SpecialBehaviorRoutine()
    {
        isSpecialActing = true;
        Vector3 originalScale = transform.localScale;

        for (int i = 0; i < 2; i++)
        {
            transform.localScale = new Vector3(originalScale.x * 1.2f, originalScale.y * 0.8f, 1f);
            yield return new WaitForSeconds(0.15f);
            transform.localScale = new Vector3(originalScale.x * 0.8f, originalScale.y * 1.2f, 1f);
            yield return new WaitForSeconds(0.15f);
        }

        transform.localScale = originalScale;
        yield return new WaitForSeconds(0.2f);
        isSpecialActing = false;
    }
}