using UnityEngine;

namespace Map
{
    public class MapGenerator : MonoBehaviour
    {
        public int mapWidth;
        public int mapHeight;
        public float noiseScale;

        public bool autoUpdate;
        public void GenerateMap()
        {
             float[,] noiseMap =  Noise.GeneratedNoiseMap(mapWidth, mapHeight, noiseScale);

             MapDisplay display = FindFirstObjectByType<MapDisplay>();
              display.DrawnNoiseMap(noiseMap);
        }
    }
}