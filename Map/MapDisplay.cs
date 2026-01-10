using UnityEngine;

namespace Map
{
    public class MapDisplay : MonoBehaviour
    {
        public Renderer textureRenderer;

        public void DrawnTexture(Texture2D texture)
        {
            
            textureRenderer.sharedMaterial.mainTexture = texture;
            textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
        }
    }
    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }
}