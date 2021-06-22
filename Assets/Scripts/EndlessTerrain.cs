using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdBeforeChunkUpdate = 25f;
    const float sqrViewerMoveThresholdBeforeChunkUpdate = viewerMoveThresholdBeforeChunkUpdate * viewerMoveThresholdBeforeChunkUpdate;
    const float colliderGenerationDistanceThreshold = 5f;

    public int colliderLODIndex;
    public static float maxViewDistance;
    public LODInfo[] detailLevels;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    private Vector2 viewerPositionOld;
    private static MapGenerator mapGenerator;
    private int chunkSize;
    private int chuckVisibleInViewDistance;

    private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private static List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private void Start()
    {
        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = mapGenerator.mapChunkSize - 1;
        chuckVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.meshSettings.uniformScale;

        if (viewerPosition != viewerPositionOld)
        {
            foreach (TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdBeforeChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    private void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();

        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewer.position.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewer.position.z / chunkSize);

        for (int yOffset = -chuckVisibleInViewDistance; yOffset <= chuckVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chuckVisibleInViewDistance; xOffset <= chuckVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    continue;
                }

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, colliderLODIndex, transform, mapMaterial));
                }
            }
        }
    }

    public class TerrainChunk
    {
        public Vector2 coord;

        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        int colliderLODIndex;

        HeightMap mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;
        bool hasSetCollider;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Material material)
        {
            this.coord = coord;
            this.detailLevels = detailLevels;
            this.colliderLODIndex = colliderLODIndex;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0f, position.y);

            meshObject = new GameObject("Terrain Chunk");

            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * mapGenerator.meshSettings.uniformScale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * mapGenerator.meshSettings.uniformScale;
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod);
                lodMeshes[i].updateCallback += UpdateTerrainChunk;
                if (i == colliderLODIndex)
                {
                    lodMeshes[i].updateCallback += UpdateCollisionMesh;
                }
            }

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        private void OnMapDataReceived(HeightMap mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            // Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            // meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (!mapDataReceived) return;

            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

            bool wasVisible = IsVisible();
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

            if (visible)
            {
                int lodIndex = 0;
                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (lodIndex != previousLODIndex)
                {
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(mapData);
                    }
                }

                visibleTerrainChunks.Add(this);
            }

            if (wasVisible != visible)
            {
                if (visible)
                {
                    visibleTerrainChunks.Add(this);
                }
                else
                {
                    visibleTerrainChunks.Remove(this);
                }

                SetVisible(visible);
            }

        }

        public void UpdateCollisionMesh()
        {
            if (hasSetCollider) return;

            float sqrDistanceViewerToEdge = bounds.SqrDistance(viewerPosition);

            if (sqrDistanceViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDistanceThreshold)
            {
                if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
                {
                    lodMeshes[colliderLODIndex].RequestMesh(mapData);
                }
            }

            if (sqrDistanceViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
            {
                if (lodMeshes[colliderLODIndex].hasMesh)
                {
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasSetCollider = true;
                }
            }
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    public class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;

        int lod;
        public event System.Action updateCallback;

        public LODMesh(int lod)
        {
            this.lod = lod;
        }

        public void RequestMesh(HeightMap mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        [Range(0, MeshGenerator.numSupportedLODs - 1)]
        public int lod;
        public float visibleDistanceThreshold;
        public float sqrVisibleDistanceThreshold
        {
            get
            {
                return visibleDistanceThreshold * visibleDistanceThreshold;
            }
        }
    }
}
