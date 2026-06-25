using System.Collections.Generic;
using UnityEngine;

public class GrassSpawner2D : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject grassPrefab; // Kéo Prefab Sprite Triangle (Cỏ 2D) vào đây

    [Header("Map Settings")]
    public Vector2 mapSize = new Vector2(50, 50);

    [Header("Poisson Disc Settings")]
    public float minDistanceBetweenClusters = 4f;
    public int rejectionSamples = 30;

    [Header("Perlin Noise Settings")]
    public float noiseScale = 0.1f;
    public int maxGrassPerCluster = 4;

    [Header("Scale Settings")]
    public float minScale = 0.8f;
    public float maxScale = 1.5f;

    private List<Vector2> clusterCenters = new List<Vector2>();

    void Start()
    {
        GenerateGrassNetwork();
    }

    void GenerateGrassNetwork()
    {
        // 1. Lấy mẫu phân bố các cụm cỏ bằng Poisson Disc 2D (Đã tối ưu hóa)
        clusterCenters = PoissonDiscSampling2D(minDistanceBetweenClusters, mapSize, rejectionSamples);

        foreach (Vector2 center in clusterCenters)
        {
            // 2. Perlin Noise tính mật độ (Dùng trực tiếp X và Y của 2D)
            float noiseValue = Mathf.PerlinNoise(center.x * noiseScale, center.y * noiseScale);

            if (noiseValue < 0.2f) continue;

            int grassCount = Mathf.CeilToInt(noiseValue * maxGrassPerCluster);
            grassCount = Mathf.Clamp(grassCount, 1, maxGrassPerCluster);

            for (int i = 0; i < grassCount; i++)
            {
                // Tạo độ lệch ngẫu nhiên xung quanh tâm cụm trong không gian 2D
                Vector2 offset = new Vector2(Random.Range(-0.8f, 0.8f), Random.Range(-0.8f, 0.8f));
                Vector2 spawnPos = center + offset;

                GameObject grass = Instantiate(grassPrefab, spawnPos, Quaternion.identity, transform);

                // 3. Xoay ngẫu nhiên quanh trục Z (Dành riêng cho 2D)
                float randomZRotation = Random.Range(0f, 360f);
                grass.transform.rotation = Quaternion.Euler(0, 0, randomZRotation);

                // Thay đổi kích thước theo tỉ lệ Perlin Noise
                float dynamicScale = Mathf.Lerp(minScale, maxScale, noiseValue) * Random.Range(0.9f, 1.1f);
                grass.transform.localScale = new Vector3(dynamicScale, dynamicScale, 1f);
            }
        }
    }

    // Hàm Poisson Disc Sampling hệ số O(N) đã được tối ưu hóa bằng Grid
    List<Vector2> PoissonDiscSampling2D(float radius, Vector2 regionSize, int maxSamples)
    {
        // Kích thước lý thuyết của mỗi ô lưới để đảm bảo mỗi ô chỉ chứa tối đa 1 điểm
        float cellSize = radius / Mathf.Sqrt(2);

        // Khởi tạo lưới bản đồ (Grid) để kiểm tra va chạm vùng lân cận nhanh hơn
        int gridWidth = Mathf.CeilToInt(regionSize.x / cellSize);
        int gridHeight = Mathf.CeilToInt(regionSize.y / cellSize);
        int[,] grid = new int[gridWidth, gridHeight];

        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        // Tạo điểm xuất phát đầu tiên ở chính giữa bản đồ
        Vector2 firstPoint = new Vector2(regionSize.x / 2, regionSize.y / 2);
        points.Add(firstPoint);
        spawnPoints.Add(firstPoint);

        // Lưu index vào lưới (Lưu points.Count tức là Index + 1 để tránh trùng với mặc định là 0)
        grid[(int)(firstPoint.x / cellSize), (int)(firstPoint.y / cellSize)] = points.Count;

        float radiusSq = radius * radius;

        while (spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCenter = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < maxSamples; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCenter + dir * Random.Range(radius, 2 * radius);

                // Kiểm tra xem điểm này có nằm trong giới hạn map không
                if (candidate.x >= 0 && candidate.x <= regionSize.x && candidate.y >= 0 && candidate.y <= regionSize.y)
                {
                    int cellX = (int)(candidate.x / cellSize);
                    int cellY = (int)(candidate.y / cellSize);

                    // Giới hạn vùng quét xung quanh ô hiện tại (chỉ quét các ô lân cận trong bán kính tìm kiếm)
                    int searchStartX = Mathf.Max(0, cellX - 2);
                    int searchEndX = Mathf.Min(gridWidth - 1, cellX + 2);
                    int searchStartY = Mathf.Max(0, cellY - 2);
                    int searchEndY = Mathf.Min(gridHeight - 1, cellY + 2);

                    bool isFarEnough = true;

                    for (int x = searchStartX; x <= searchEndX; x++)
                    {
                        for (int y = searchStartY; y <= searchEndY; y++)
                        {
                            int pointIndex = grid[x, y] - 1;
                            if (pointIndex != -1)
                            {
                                // Sử dụng bình phương khoảng cách (sqrMagnitude) để loại bỏ phép tính Căn Bậc Hai (Tối ưu CPU)
                                if ((candidate - points[pointIndex]).sqrMagnitude < radiusSq)
                                {
                                    isFarEnough = false;
                                    break;
                                }
                            }
                        }
                        if (!isFarEnough) break;
                    }

                    if (isFarEnough)
                    {
                        points.Add(candidate);
                        spawnPoints.Add(candidate);
                        grid[cellX, cellY] = points.Count;
                        candidateAccepted = true;
                        break;
                    }
                }
            }
            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }
        return points;
    }
}