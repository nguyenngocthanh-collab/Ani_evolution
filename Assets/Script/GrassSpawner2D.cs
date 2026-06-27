using System.Collections.Generic;
using UnityEngine;

public class GrassSpawner2D : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject grassPrefab;

    [Header("Map Settings")]
    public Vector2 mapSize = new Vector2(50, 50);

    [Header("Cluster Settings")]
    public float minDistanceBetweenClusters = 4f;
    public float clusterSpread = 1.5f;
    public int maxGrassPerCluster = 4;
    public int rejectionSamples = 30;

    [Header("Perlin Noise Settings")]
    public float noiseScale = 0.1f;
    public float noiseThreshold = 0.2f;

    [Header("Scale Settings")]
    public float minScale = 0.05f;
    public float maxScale = 0.15f;

    [Header("Rotation Settings (0 = đứng thẳng)")]
    public float maxTiltAngle = 0f;

    private List<Vector2> clusterCenters = new List<Vector2>();
    private Vector2 regionOffset;

    void Start()
    {
        regionOffset = (Vector2)transform.position - mapSize * 0.5f;
        GenerateGrassNetwork();
    }

    void GenerateGrassNetwork()
    {
        clusterCenters = PoissonDiscSampling2D(minDistanceBetweenClusters, mapSize, rejectionSamples);

        foreach (Vector2 center in clusterCenters)
        {
            float noiseValue = Mathf.PerlinNoise(center.x * noiseScale, center.y * noiseScale);

            if (noiseValue < noiseThreshold) continue;

            int grassCount = Mathf.CeilToInt(noiseValue * maxGrassPerCluster);
            grassCount = Mathf.Clamp(grassCount, 1, maxGrassPerCluster);

            for (int i = 0; i < grassCount; i++)
            {
                Vector2 offset = new Vector2(
                    Random.Range(-clusterSpread, clusterSpread),
                    Random.Range(-clusterSpread, clusterSpread)
                );
                Vector2 spawnPos = regionOffset + center + offset;

                GameObject grass = Instantiate(grassPrefab, spawnPos, Quaternion.identity, transform);

                float tilt = Random.Range(-maxTiltAngle, maxTiltAngle);
                grass.transform.rotation = Quaternion.Euler(0, 0, tilt);

                float dynamicScale = Mathf.Lerp(minScale, maxScale, noiseValue) * Random.Range(0.9f, 1.1f);
                grass.transform.localScale = new Vector3(dynamicScale, dynamicScale, 1f);
            }
        }
    }

    List<Vector2> PoissonDiscSampling2D(float radius, Vector2 regionSize, int maxSamples)
    {
        float cellSize = radius / Mathf.Sqrt(2);

        int gridWidth = Mathf.CeilToInt(regionSize.x / cellSize);
        int gridHeight = Mathf.CeilToInt(regionSize.y / cellSize);
        int[,] grid = new int[gridWidth, gridHeight];

        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        Vector2 firstPoint = new Vector2(regionSize.x / 2, regionSize.y / 2);
        points.Add(firstPoint);
        spawnPoints.Add(firstPoint);

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

                if (candidate.x >= 0 && candidate.x <= regionSize.x && candidate.y >= 0 && candidate.y <= regionSize.y)
                {
                    int cellX = (int)(candidate.x / cellSize);
                    int cellY = (int)(candidate.y / cellSize);

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
                spawnPoints.RemoveAt(spawnIndex);
        }
        return points;
    }
}
