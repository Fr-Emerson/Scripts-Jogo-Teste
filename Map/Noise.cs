using System;
using System.Collections;
using UnityEngine;
namespace Map
{
    public static class Noise
    {
        
        public enum NormalizeMode
        {
            Local,
            Global
        }
        public static float[,] GeneratedNoiseMap(int mapWidth, int mapHeight, float scale,int seed, int octaves, float persistence, float lacunarity, Vector2 offset, NormalizeMode normalizeMode = NormalizeMode.Global)
        {
            float [,] noiseMap = new float[mapWidth, mapHeight];
            float amplitude = 1;
            float frequency = 1;
            System.Random prng = new System.Random(seed);
            Vector2 [] octaveOffsets = new Vector2[octaves];
            float maxPossibleHeight = 0;
            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) - offset.x;
                float offsetY = prng.Next(-100000, 100000) - offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
                maxPossibleHeight += amplitude; 
                amplitude *= persistence;
            }
            if (scale <= 0)
            {
                scale = 0.0001f;
            }
            float maxLocalNoiseHeight = float.MinValue;
            float minLocalNoiseHeight = float.MaxValue;
            float halfWidth = mapWidth / 2f;
            float halfHeight = mapHeight / 2f;
            for (int y = 0; y < mapHeight; y++)
            {
                for(int x = 0; x < mapWidth; x++)
                {
                    amplitude = 1;
                    frequency = 1;
                    float noiseHeight = 0;
                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x * frequency;
                        float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y * frequency;
                     
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseMap[x, y] = perlinValue;
                        noiseHeight += perlinValue * amplitude;
                        
                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxLocalNoiseHeight)
                    {
                        maxLocalNoiseHeight = noiseHeight;
                    }
                    else if(noiseHeight < minLocalNoiseHeight)
                    {
                        minLocalNoiseHeight = noiseHeight;
                    }
                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    if (normalizeMode == NormalizeMode.Local)
                    {
                        noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                    }
                    else
                    {
                        float normalizedHeight = (noiseMap[x,y] +1)/(2f*maxPossibleHeight/1.75f);
                        noiseMap[x, y] = Mathf.Clamp(normalizedHeight,0,int.MaxValue);
                    }
                }
            }

            return noiseMap;
        }

    }
}
