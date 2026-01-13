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
        
        public float minHeight => uniformScale*heightMultiplier*meshHeightCurve.Evaluate(0);

        public float maxHeight => uniformScale*heightMultiplier*meshHeightCurve.Evaluate(1);
    }
}