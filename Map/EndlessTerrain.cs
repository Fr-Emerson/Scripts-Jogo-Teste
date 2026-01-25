using System;
using UnityEngine;
using System.Collections.Generic;

namespace Map
{
    public class EndlessTerrain : MonoBehaviour
    {
      
        const float viewerMoveThresholdForChunkUpdate = 25f;
        const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
        private const float colliderGenerationDistanceThreshold = 5f;

        public int colliderLODIndex;
        public LODInfo[] details;
        public Transform viewer;
        public Material mapMaterial;
        public static Vector2 ViewerPosition;
        Vector2 _viewerPositionOld;
        
        public static MapGenerator mapGenerator;
        
        public static float MaxViewDistance;
        int _chunkSize;
        int _chunksVisibleInViewDistance;
        Dictionary<Vector2, TerrainChunk> _terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
        static List<TerrainChunk> _visibleTerrainChunks = new List<TerrainChunk>();
        private void Start()
        {
            mapGenerator = FindFirstObjectByType<MapGenerator>();
            
            MaxViewDistance = details[details.Length - 1].visibleDistanceThreshold;
            _chunkSize = mapGenerator.MapChunkSize - 1;
            _chunksVisibleInViewDistance = Mathf.RoundToInt(MaxViewDistance / _chunkSize);
            UpdateVisibleChunks();
        }

        private void Update()
        {
            ViewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;
            if (ViewerPosition != _viewerPositionOld)
            {
                foreach (TerrainChunk terrainChunk in _visibleTerrainChunks)
                {
                    terrainChunk.UpdateCollisionMesh();
                }
            }
            if ((_viewerPositionOld  - ViewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
            {
                _viewerPositionOld = ViewerPosition;
                UpdateVisibleChunks();
            }
        }
        
        void UpdateVisibleChunks()
        {
            HashSet<Vector2> alreadyUpdatedChunkCoord = new HashSet<Vector2>();
            for (int i = _visibleTerrainChunks.Count -1; i >= 0; i--)
            {
                alreadyUpdatedChunkCoord.Add(_visibleTerrainChunks[i].coord);
                _visibleTerrainChunks[i].UpdateTerrainChunk();
            }
            int currentChunkCoordX = Mathf.RoundToInt(viewer.position.x / _chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewer.position.y / _chunkSize);

            for (int yOffset = -_chunksVisibleInViewDistance ; yOffset <= _chunksVisibleInViewDistance; yOffset++)
            {
                for (int xOffset = -_chunksVisibleInViewDistance ; xOffset <= _chunksVisibleInViewDistance; xOffset++)
                {
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                    if (!alreadyUpdatedChunkCoord.Contains(viewedChunkCoord))
                    {
                        if (_terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                        {
                            _terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                        }
                        else
                        {
                            TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, _chunkSize, details,colliderLODIndex,transform, mapMaterial);
                            _terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        }
                    }
                }
            }
        }

        #region ||-- Terrain Chunk Class --||
        [Serializable]
        public class TerrainChunk
        {

            public Vector2 coord;
            GameObject meshObject;
            Vector3 position;
            Bounds bounds;
            
            MapData mapData;
            bool mapDataReceived;
            int previousLODIndex = -1;
            
            MeshRenderer meshRenderer;
            MeshFilter meshFilter;
            MeshCollider meshCollider;

            int colliderLODIndex;
            LODInfo[] detailLevels;
            LODMesh[] lodMeshes;
            LODMesh collisionLODMesh;

            bool hasSetCollider;
            public TerrainChunk(Vector2 coord, int size,LODInfo[] details,int colliderLODIndex,Transform parent, Material material )
            {
                this.coord = coord;
                this.detailLevels = details;
                this.colliderLODIndex = colliderLODIndex;
                position = coord * size;
                bounds = new Bounds(position, Vector2.one * size);
                Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            
                meshObject = new GameObject("Terrain Chunk"); 
                meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshFilter = meshObject.AddComponent<MeshFilter>();
                meshCollider = meshObject.AddComponent<MeshCollider>();
                
                meshRenderer.material = material;
                
                meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
                meshObject.transform.parent = parent;
                meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
                SetVisible(false);
                
                lodMeshes = new LODMesh[details.Length];
                for (int i = 0; i < detailLevels.Length; i++)
                {
                    lodMeshes[i] = new LODMesh(detailLevels[i].lod);
                    lodMeshes[i].UpdateCallback += UpdateTerrainChunk;
                    if (i == colliderLODIndex)
                    {
                        lodMeshes[i].UpdateCallback += UpdateCollisionMesh;
                    }
                }
                mapGenerator.RequestMapData(position,OnMapDataReceived);
            
            }
            void OnMapDataReceived(MapData mapData)
            {
                Debug.Log("Received map data for chunk at " + position);
                this.mapData = mapData;
                mapDataReceived = true;
                
                UpdateTerrainChunk();
            }
            
            public void UpdateTerrainChunk()
            {
                if (mapDataReceived)
                {
                    float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(ViewerPosition));
                    bool wasVisible = IsVisible();
                    bool visible = viewerDistanceFromNearestEdge <= MaxViewDistance;
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
                                meshCollider.sharedMesh = lodMesh.mesh;
                            }
                            else if (!lodMesh.hasRequestedMesh)
                            {
                                lodMesh.RequestMesh(mapData);
                            }
                        }

                       
                        _visibleTerrainChunks.Add(this);
                    }

                    if (wasVisible != visible)
                    {
                        if (visible)
                        {
                            _visibleTerrainChunks.Add(this);
                        }
                        else
                        {
                            _visibleTerrainChunks.Remove(this);
                        }
                    }
                    SetVisible(visible);
                }
            }

            public void UpdateCollisionMesh()
            {
                if (!hasSetCollider)
                {
                    float sqrDistanceFromViewerToEdge = bounds.SqrDistance(ViewerPosition);

                    if (sqrDistanceFromViewerToEdge < detailLevels[colliderLODIndex].visibleDistanceThreshold)
                    {
                        if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
                        {
                            lodMeshes[colliderLODIndex].RequestMesh(mapData);
                            hasSetCollider = true;
                        }
                    }
                    if (sqrDistanceFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
                    {
                        if (lodMeshes[colliderLODIndex].hasMesh)
                        {
                            meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                        }
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
        #endregion
        #region ||-- LOD Mesh Class --||
        class LODMesh
        {
            public Mesh mesh;
            public bool hasRequestedMesh;
            public bool hasMesh;
            public event Action UpdateCallback;
            int lod;
           
            
            public LODMesh(int lod)
            {
                this.lod = lod;
            }
            void OnMeshDataReceived(MeshData meshData )
            {
                mesh = meshData.CreateMesh();
                hasMesh = true;
                UpdateCallback();
            }

            public void RequestMeshData(MapData mapData)
            {
                hasRequestedMesh = true;
                mapGenerator.RequestMeshData(mapData, lod,OnMeshDataReceived);
            }

            public void RequestMesh(MapData mapData)
            {
                hasRequestedMesh = true;
                mapGenerator.RequestMeshData(mapData, lod ,OnMeshDataReceived);
            }
            
        }
        #endregion
        [Serializable]
        public struct LODInfo
        {
            [Range(0,MeshGenerator.numSupportedLods-1)]
            public int lod;
            public float visibleDistanceThreshold;

            public float SqrVisibleDistanceThreshold => visibleDistanceThreshold * visibleDistanceThreshold;
        }
        
    }
    
}