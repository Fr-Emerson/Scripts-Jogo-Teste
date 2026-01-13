using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Map.Data;
using TerrainData = UnityEngine.TerrainData;

namespace Map
{
    public class MapGenerator : MonoBehaviour
    {
        #region ||-- Variables --||
        public enum DrawMode
        {
            NoiseMap,
            Mesh,
            FalloffMap
        }
        public DrawMode drawMode;
        
        public Data.TerrainData terrainData;
        public NoiseData noiseData;
        public TextureData textureData;
        public Material terrainMaterial;
        
        [Range(0,6)]
        public int editorLevelOfDetailPreview;
        
        public bool autoUpdate;
        float [,] falloffMap;
        Queue<MapThreadInfo<MapData>> _mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
        Queue<MapThreadInfo<MeshData>> _meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
        #endregion
        
        void OnValuesUpdated()
        {
            if (!Application.isPlaying)
            {
                DrawMapInEditor();
            }
        }
        void OnTextureValuesUpdated()
        {
            textureData.ApplyMaterial(terrainMaterial);
        }
        public int MapChunkSize
        {
            get
            {
                if (terrainData.useFlatShading)
                {
                    return 95;
                }
                else
                {
                    return 239;
                }
            }
        }
        private void Update()
        {
           
            if (_mapDataThreadInfoQueue.Count > 0)
            {
                for (int i = 0; i < _mapDataThreadInfoQueue.Count; i++)
                {
                    MapThreadInfo<MapData> threadInfo = _mapDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
            }
            

            if (_meshDataThreadInfoQueue.Count > 0)
            {
                MapThreadInfo<MeshData> threadInfo = _meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
            
        }

        #region ||-- Mesh Data --||

        public void RequestMeshData(MapData mapData,int lod, Action<MeshData> callback )
        {
            ThreadStart threadStart = delegate
            {
                MeshDataThread(mapData, lod ,callback);
            };
            new Thread(threadStart).Start();
        }
        void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
        {
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.HeightMap,terrainData.heightMultiplier,terrainData.meshHeightCurve, lod,terrainData.useFlatShading);
            lock (_meshDataThreadInfoQueue)
            {
                _meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
            }
        }

        #endregion
        #region ||-- Map Data --||
        public void RequestMapData(Vector2 centre,Action<MapData> callback)
        {
            ThreadStart threadStart = delegate
            {
                MapDataThread(centre,callback);
            };
            new Thread(threadStart).Start();
        }

        void MapDataThread(Vector2 centre,Action<MapData> callback)
        {
            MapData mapData = GenerateMapData(centre);
            lock (_mapDataThreadInfoQueue)
            {
                _mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            }
        }
       
        MapData GenerateMapData(Vector2 centre)
        {
             float[,] noiseMap =  Noise.GeneratedNoiseMap(MapChunkSize + 2, MapChunkSize + 2,noiseData.noiseScale,noiseData.seed,noiseData.octaves, noiseData.persistence, noiseData.lacunarity,centre + noiseData.offset, noiseData.normalizeMode);
             if (terrainData.useFalloff)
             {
                 if (falloffMap == null)
                 {
                     falloffMap = FalloffGenerator.GenerateFalloffMap(MapChunkSize + 2);
                 }
                 for (int y = 0; y < MapChunkSize+2; y++)
                 {
                     for (int x = 0; x < MapChunkSize+2; x++)
                     {
                         if (terrainData.useFalloff)
                         {
                             noiseMap[x,y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                         }
                     }
             }
             }

             return new MapData(noiseMap);
        }
        #endregion

        #region ||-- Editor --||
        public void DrawMapInEditor()
        {
            MapData mapData = GenerateMapData(Vector2.zero);
            float[,] noiseMap = mapData.HeightMap;
            MapDisplay display = FindFirstObjectByType<MapDisplay>();
            if (drawMode == DrawMode.NoiseMap)
            {
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
            }
            else if (drawMode == DrawMode.Mesh)
            {
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap,terrainData.heightMultiplier,terrainData.meshHeightCurve, editorLevelOfDetailPreview,terrainData.useFlatShading));
            }
            else if( drawMode == DrawMode.FalloffMap)
            {
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(MapChunkSize)));
            }
        }
        #endregion

        private void OnValidate()
        {
            if (terrainData!= null)
            {
                terrainData.OnValuesUpdated -= OnValuesUpdated;
                terrainData.OnValuesUpdated += OnValuesUpdated;
            }

            if (noiseData != null)
            {
                noiseData.OnValuesUpdated -= OnValuesUpdated;
                noiseData.OnValuesUpdated += OnValuesUpdated;
            }

            if (textureData != null)
            {
                textureData.OnValuesUpdated -= OnTextureValuesUpdated;
                textureData.OnValuesUpdated += OnTextureValuesUpdated;
            }
        }

        struct MapThreadInfo<T>
        {
            public readonly Action<T> callback;
            public readonly T parameter;

            public MapThreadInfo(Action<T> callback, T parameter)
            {
                this.callback = callback;
                this.parameter = parameter;
            }
        }
    }
    [Serializable]
    public struct MapData
    {
        public readonly float[,] HeightMap;
        public MapData(float[,] heightMap)
        {
            HeightMap = heightMap;
        }
    }
}