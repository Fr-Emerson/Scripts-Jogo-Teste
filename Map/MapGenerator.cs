using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
namespace Map
{
    public class MapGenerator : MonoBehaviour
    {
        #region ||-- Variables --||
        public enum DrawMode
        {
            NoiseMap,
            ColorMap,
            Mesh,
            FalloffMap
        }
        public DrawMode drawMode;
        public Noise.NormalizeMode normalizeMode;
        public bool useFlatShading;
        [Range(0,6)]
        public int editorLevelOfDetailPreview;
        public float noiseScale;
        
        public int octaves;
        [Range(0,1)]
        public float persistence;
        public float lacunarity;
        
        public int seed;
        public Vector2 offset;
        public bool useFalloff;
        public float heightMultiplier;
        public AnimationCurve meshHeightCurve;
        public bool autoUpdate;
        public TerrainType[] regions;
        float [,] falloffMap;
        static MapGenerator instance;
        Queue<MapThreadInfo<MapData>> _mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
        Queue<MapThreadInfo<MeshData>> _meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
        #endregion

        private void Awake()
        {
            falloffMap = FalloffGenerator.GenerateFalloffMap(MapChunkSize);
        }
        public static int MapChunkSize
        {
            get
            {
                if (instance== null)
                {
                    instance = FindFirstObjectByType<MapGenerator>();
                }
                if (instance.useFlatShading)
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
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.HeightMap,heightMultiplier,meshHeightCurve, lod,useFlatShading);
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
             float[,] noiseMap =  Noise.GeneratedNoiseMap(MapChunkSize + 2, MapChunkSize + 2, noiseScale,seed,octaves, persistence, lacunarity,centre + offset, normalizeMode);
             Color[] colourMap = new Color[MapChunkSize * MapChunkSize];
             for (int y = 0; y < MapChunkSize; y++)
             {
                 for (int x = 0; x < MapChunkSize; x++)
                 {
                     if (useFalloff)
                     {
                         noiseMap[x,y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                     }
                     float currentHeight = noiseMap[x, y];
                     for (int i = 0; i < regions.Length; i++)
                     {
                         if (currentHeight>=regions[i].height)
                         {
                             colourMap[y * MapChunkSize + x] = regions[i].color;
                         }
                         else
                         {
                             break;
                             
                         }
                     }
                 }
             }

             return new MapData(noiseMap, colourMap);
        }
        #endregion

        #region ||-- Editor --||
        public void DrawMapInEditor()
        {
            MapData mapData = GenerateMapData(Vector2.zero);
            float[,] noiseMap = mapData.HeightMap;
            Color[] colourMap = mapData.ColourMap;
            MapDisplay display = FindFirstObjectByType<MapDisplay>();
            if (drawMode == DrawMode.NoiseMap)
            {
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
            }
            else if (drawMode == DrawMode.ColorMap)
            {
                display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, MapChunkSize, MapChunkSize));
            }
            else if (drawMode == DrawMode.Mesh)
            {
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap,heightMultiplier,meshHeightCurve, editorLevelOfDetailPreview,useFlatShading), TextureGenerator.TextureFromColourMap(colourMap, MapChunkSize, MapChunkSize));
            }
            else if( drawMode == DrawMode.FalloffMap)
            {
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(MapChunkSize)));
            }
        }
        #endregion

        private void OnValidate()
        {
            if (lacunarity < 1)
            {
                lacunarity = 1;
            }
            if (octaves < 0)
            {
                octaves = 0;
            }

            if (heightMultiplier < 1)
            {
                heightMultiplier = 1f;
            }
            falloffMap = FalloffGenerator.GenerateFalloffMap(MapChunkSize);
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
        public readonly Color[] ColourMap;
        public MapData(float[,] heightMap, Color[] colourMap)
        {
            this.HeightMap = heightMap;
            this.ColourMap = colourMap;
        }
    }
    [Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }
    
}