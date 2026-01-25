using System;
using UnityEngine;
// O Tutorial em que me baseio usa Built-in Render Pipeline, enquanto eu estou a usar URP, por isso tive de fazer algumas adaptações, entre elas, ao invés de usar uma lista dinâmica para as camadas de cor, uso um array com tamanho fixo de 8. Além disso, devido aos meus conhecimentos limitados de shaders, tive de adaptar o shader usado no tutorial para um shaderGraph que aceite as 8 camadas estáticas de cor e altura.
// Obs: O shaderGraph é baseado nos cálculos do vídeo, logo ele pode não estar na sua forma mais otimizada do mundo, btw só estou aprendendo.
namespace Map.Data
{
    [CreateAssetMenu]
    public class TextureData : UpdatableData
    {
        private const int MaxColorLayers = 8;

        private readonly string[] _colorLayersNames = { "_color1", "_color2", "_color3", "_color4", "_color5", "_color6", "_color7", "_color8" };

        private readonly string[] _baseHeightsNames =
        {
            "_baseHeight1", "_baseHeight2", "_baseHeight3", "_baseHeight4", "_baseHeight5", "_baseHeight6",
            "_baseHeight7", "_baseHeight8"
        };
        private readonly string[] _baseBlends = {"_BaseBlend1", "_BaseBlend2", "_BaseBlend3", "_BaseBlend4", "_BaseBlend5", "_BaseBlend6",
            "_BaseBlend7", "_BaseBlend8" };

        private float _savedMinHeight;
        private float _savedMaxHeight;
        
        public ColorLayer[] colorLayers;
         
        public void ApplyMaterial(Material material)
        {
            if (material == null)
            {
                Debug.LogError("Material é nulo!");
                return;
            }
            
            if (colorLayers == null || colorLayers.Length < MaxColorLayers)
            {
                Debug.LogWarning($"colorLayers deve ter {MaxColorLayers} elementos!");
            }
            
            UpdateMeshHeights(material, _savedMinHeight, _savedMaxHeight);
            
            int layerCount = Mathf.Min(MaxColorLayers, colorLayers?.Length ?? 0);
            for (int i = 0; i < layerCount; i++)
            {
                material.SetColor(_colorLayersNames[i], colorLayers[i].color);
                material.SetFloat(_baseHeightsNames[i], colorLayers[i].baseStartHeight);
                material.SetFloat(_baseBlends[i], colorLayers[i].baseBlend);
            }
        }
        
        public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
        {
            _savedMinHeight = minHeight;
            _savedMaxHeight = maxHeight;
            material.SetFloat("_minHeight", minHeight);
            material.SetFloat("_maxHeight", maxHeight);
            
        }
    }
    
    [Serializable]
    public class ColorLayer
    {
        [Header("Color Layer")]
        public Color color;
        [Range(0, 1)] public float baseStartHeight;
        [Range(0, 1)] public float baseBlend;
    }
}