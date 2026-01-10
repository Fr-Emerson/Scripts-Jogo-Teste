using System;
using UnityEngine;
using System.Collections.Generic;

namespace Map
{
    public class EndlessTerrain : MonoBehaviour
    {
        const float viewerMoveThresholdForChunkUpdate = 25f;
        const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
        
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
        List<TerrainChunk> _terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
        private void Start()
        {
            MaxViewDistance = details[details.Length - 1].visibleDistanceThreshold;
            mapGenerator = FindFirstObjectByType<MapGenerator>();
            _chunkSize = MapGenerator.MapChunkSize - 1;
            _chunksVisibleInViewDistance = Mathf.RoundToInt(MaxViewDistance / _chunkSize);
            UpdateVisibleChunks();
        }

        private void Update()
        {
            ViewerPosition = new Vector2(viewer.position.x, viewer.position.z);
            if ((_viewerPositionOld  - ViewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
            {
                _viewerPositionOld = ViewerPosition;
                UpdateVisibleChunks();
            }
        }
        
        void UpdateVisibleChunks()
        {
            for (int i = 0; i < _terrainChunksVisibleLastUpdate.Count; i++)
            {
                _terrainChunksVisibleLastUpdate[i].SetVisible(false);
            }
            _terrainChunksVisibleLastUpdate.Clear();
            int currentChunkCoordX = Mathf.RoundToInt(viewer.position.x / _chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewer.position.z / _chunkSize);

            for (int yOffset = -_chunksVisibleInViewDistance ; yOffset <= _chunksVisibleInViewDistance; yOffset++)
            {
                for (int xOffset = -_chunksVisibleInViewDistance ; xOffset <= _chunksVisibleInViewDistance; xOffset++)
                {
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                    if (_terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        _terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                        if (_terrainChunkDictionary[viewedChunkCoord].IsVisible())
                        {
                            _terrainChunksVisibleLastUpdate.Add(_terrainChunkDictionary[viewedChunkCoord]);
                        }
                    }
                    else
                    {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, _chunkSize, details,transform, mapMaterial);
                        _terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                    }
                }
            }
        }

        #region ||-- Terrain Chunk Class --||
        [Serializable]
        public class TerrainChunk
        {
            GameObject meshObject;
            Vector3 position;
            Bounds bounds;
            
            MapData mapData;
            bool mapDataReceived;
            int previousLODIndex = -1;
            
            MeshRenderer meshRenderer;
            MeshFilter meshFilter;
            
            LODInfo[] detailLevels;
            LODMesh[] lodMeshes;
            
            public TerrainChunk(Vector2 coord, int size,LODInfo[] details,Transform parent, Material material )
            {
                this.detailLevels = details;
                position = coord * size;
                bounds = new Bounds(position, Vector2.one * size);
                Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            
                meshObject = new GameObject("Terrain Chunk"); 
                meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshFilter = meshObject.AddComponent<MeshFilter>();
                meshRenderer.material = material;
                
                meshObject.transform.position = positionV3;
                meshObject.transform.parent = parent;
                SetVisible(false);
                
                lodMeshes = new LODMesh[details.Length];
                for (int i = 0; i < detailLevels.Length; i++)
                {
                    lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
                }
                mapGenerator.RequestMapData(position,OnMapDataReceived);
            
            }
            void OnMapDataReceived(MapData mapData)
            {
                Debug.Log("Received map data for chunk at " + position);
                this.mapData = mapData;
                mapDataReceived = true;
                Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.ColourMap, MapGenerator.MapChunkSize, MapGenerator.MapChunkSize);
                meshRenderer.material.mainTexture = texture;
                UpdateTerrainChunk();
                // mapGenerator.RequestMeshData(mapData,OnMeshDataReceived);
            }
            // void OnMeshDataReceived(MeshData meshData)
            // {
            //     meshFilter.mesh = meshData.CreateMesh();
            // }
            
            
            public void UpdateTerrainChunk()
            {
                if (mapDataReceived)
                {
                    float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(ViewerPosition));
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
                                meshFilter.mesh = lodMesh.mesh;
                                previousLODIndex = lodIndex;
                            }
                            else if (!lodMesh.hasRequestedMesh)
                            {
                                lodMesh.RequestMesh(mapData);
                            }
                        }
                    }

                    SetVisible(visible);
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
            Action UpdateCallback;
            int lod;
           
            
            public LODMesh(int lod, Action UpdateCallback)
            {
                this.lod = lod;
                this.UpdateCallback = UpdateCallback;
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
            public int lod;
            public float visibleDistanceThreshold;
        }
        
    }
    
}