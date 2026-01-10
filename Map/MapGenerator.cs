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
        public int mapWidth;
        public int mapHeight;
        public float noiseScale;
        
        public int octaves;
        [Range(0,1)]
        public float persistence;
        public float lacunarity;
        
        public int seed;
        public Vector2 offset;
        
        public float heightMultiplier;
        public bool autoUpdate;
        public TerrainType[] regions;
        public void GenerateMap()
        {
             float[,] noiseMap =  Noise.GeneratedNoiseMap(mapWidth, mapHeight, noiseScale,seed,octaves, persistence, lacunarity,offset);
             Color[] colourMap = new Color[mapWidth * mapHeight];
             for (int y = 0; y < mapHeight; y++)
             {
                 for (int x = 0; x < mapWidth; x++)
                 {
                     float currentHeight = noiseMap[x, y];
                     for (int i = 0; i < regions.Length; i++)
                     {
                         if (currentHeight<=regions[i].height)
                         {
                             colourMap[y * mapWidth + x] = regions[i].color;
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
                 display.DrawnTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
             }
             else if (drawMode == DrawMode.Mesh)
             {
                 display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap,heightMultiplier), TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
             }
             
        }

        private void OnValidate()
        {
            if ( mapWidth<1)
            {
                mapWidth = 1;
            }
            if (mapHeight < 1)
            {
                mapHeight = 1;
            }

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