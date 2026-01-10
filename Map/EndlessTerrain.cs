using UnityEngine;
using System.Collections.Generic;

namespace Map
{
    public class EndlessTerrain : MonoBehaviour
    {
        public const float MaxViewDistance = 450;
        public Transform viewer;
        
        public static Vector2 ViewerPosition;
        
        int _chunkSize;
        int _chunksVisibleInViewDistance;
        Dictionary<Vector2, TerrainChunk> _terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
        List<TerrainChunk> _terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
        private void Start()
        {
            _chunkSize = MapGenerator.MapChunkSize - 1;
            _chunksVisibleInViewDistance = Mathf.RoundToInt(MaxViewDistance / _chunkSize);
        }

        private void Update()
        {
            ViewerPosition = new Vector2(viewer.position.x, viewer.position.z);
            UpdateVisibleChunks();
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
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, _chunkSize, transform);
                        _terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                    }
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector3 position;
        Bounds bounds;
        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size/10f;
            meshObject.transform.parent = parent;
            SetVisible(false);
        }
        public void UpdateTerrainChunk()
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(EndlessTerrain.ViewerPosition));
            bool visible = viewerDistanceFromNearestEdge <= EndlessTerrain.MaxViewDistance;
            SetVisible(visible);
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
}