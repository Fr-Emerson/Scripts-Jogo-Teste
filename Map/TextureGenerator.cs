using UnityEngine;

namespace Map
{
    public class TextureGenerator
    {
        public static Texture2D TextureFromColourMap(Color[] colourMap, int mapWidth, int mapHeight)
        {
            Texture2D texture = new Texture2D(mapWidth, mapHeight);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(colourMap);
            texture.Apply();
            return texture;
        }

        public static Texture2D TextureFromHeightMap(float[,] heightmap)
        {
            int width = heightmap.GetLength(0);
            int height = heightmap.GetLength(1);
            
            
            Color[] colourMap = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightmap[x, y]);
                }
            }

            return TextureFromColourMap(colourMap, width, height);
        }
    }
}