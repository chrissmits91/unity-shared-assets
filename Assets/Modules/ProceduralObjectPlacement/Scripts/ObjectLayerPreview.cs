using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectLayerPreview : MonoBehaviour
{
    public List<Vector2> points;

    Vector2 meshWorldSize;

    public GameObject[] trees;

    Mesh mesh;

    public void RenderProceduralObjects(float width, float height, HeightMap heightMap, Mesh mesh) {
        meshWorldSize = new Vector2(width, height);
        points = PoissonDiscSampling.GeneratePoints(20f, meshWorldSize, 10);
        this.mesh = mesh;

        RenderTrees(heightMap);
    }

    private void RenderTrees(HeightMap heightMap) {
        List<Vector3> allowedSpawnPoints = AllowedSpawnPoints(heightMap, new Vector2(6, 45));

        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        allowedSpawnPoints.ForEach(spawnPoint => {
            Vector3 correctedSpawnPoint = NearestVertexTo(spawnPoint);
            int tree = Random.Range(0, trees.Length);
            Instantiate(trees[tree], correctedSpawnPoint, Quaternion.identity, transform);
        });
    }

    private List<Vector3> AllowedSpawnPoints(HeightMap heightMap, Vector2 heightRange) {
        List<Vector3> allowedSpawnPoints = new List<Vector3>();

        points.ForEach(point => {
            int heightMapPointX = Mathf.CeilToInt(point.x);
            int heightMapPointY = Mathf.Abs(Mathf.CeilToInt(point.y) - (int)meshWorldSize.y);

            float heightAtPoint = heightMap.values[heightMapPointX, heightMapPointY];
            if (heightAtPoint >= heightRange.x && heightAtPoint <= heightRange.y) {
                allowedSpawnPoints.Add(new Vector3(point.x - meshWorldSize.x / 2, heightAtPoint, point.y - meshWorldSize.x / 2));
            }
        });

        return allowedSpawnPoints;
    }

    private Vector3 NearestVertexTo(Vector3 point)
    {
        // convert point to local space
        point = transform.InverseTransformPoint(point);
        
        float minDistanceSqr = Mathf.Infinity;
        Vector3 nearestVertex = Vector3.zero;
        // scan all vertices to find nearest
        foreach (Vector3 vertex in mesh.vertices)
        {
            Vector3 diff = point-vertex;
            float distSqr = diff.sqrMagnitude;
            if (distSqr < minDistanceSqr) {
                minDistanceSqr = distSqr;
                nearestVertex = vertex;
            }
        }

        // convert nearest vertex back to world space
        return transform.TransformPoint(nearestVertex);
    }
}
