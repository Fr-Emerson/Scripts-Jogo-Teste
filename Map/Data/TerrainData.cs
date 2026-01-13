using System;
using UnityEngine;

namespace Map.Data
{
    [CreateAssetMenu]
    public class TerrainData : UpdatableData
    {
        public float uniformScale = 2f;
        
        public bool useFlatShading;
        public bool useFalloff;
        
        public float heightMultiplier;
        public AnimationCurve meshHeightCurve;

        protected override void OnValidate()
        {
            if (heightMultiplier < 1)
            {
                heightMultiplier = 1f;
            }
            base.OnValidate();
        }
    }
}