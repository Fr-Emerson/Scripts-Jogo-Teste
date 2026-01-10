using System;
using UnityEngine;

namespace Map
{
    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode
        {
            NoiseMap,
            ColorMap,
            Mesh
        }
        public DrawMode drawMode;
        public const int MapChunkSize = 241;
        [Range(0,6)]
        public int levelOfDetail;
        public float noiseScale;
        
        public int octaves;
        [Range(0,1)]
        public float persistence;
        public float lacunarity;
        
        public int seed;
        public Vector2 offset;
        public float heightMultiplier;
        public AnimationCurve meshHeightCurve;
        public bool autoUpdate;
        public TerrainType[] regions;
        public void GenerateMap()
        {
             float[,] noiseMap =  Noise.GeneratedNoiseMap(MapChunkSize, MapChunkSize, noiseScale,seed,octaves, persistence, lacunarity,offset);
             Color[] colourMap = new Color[MapChunkSize * MapChunkSize];
             for (int y = 0; y < MapChunkSize; y++)
             {
                 for (int x = 0; x < MapChunkSize; x++)
                 {
                     float currentHeight = noiseMap[x, y];
                     for (int i = 0; i < regions.Length; i++)
                     {
                         if (currentHeight<=regions[i].height)
                         {
                             colourMap[y * MapChunkSize + x] = regions[i].color;
                             break;
                         }
                     }
                 }
             }
             MapDisplay display = FindFirstObjectByType<MapDisplay>();
             if (drawMode == DrawMode.NoiseMap)
             {
                 display.DrawnTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
             }
             else if (drawMode == DrawMode.ColorMap)
             {
                 display.DrawnTexture(TextureGenerator.TextureFromColourMap(colourMap, MapChunkSize, MapChunkSize));
             }
             else if (drawMode == DrawMode.Mesh)
             {
                 display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap,heightMultiplier,meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, MapChunkSize, MapChunkSize));
             }
             
        }

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
        }
    }
}