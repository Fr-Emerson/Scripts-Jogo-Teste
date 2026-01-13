using System;
using UnityEngine;

namespace Map.Data
{
    [CreateAssetMenu]
    public class UpdatableData : ScriptableObject
    {
        public event Action OnValuesUpdated;
        public bool autoUpdate = true;

        protected virtual void OnValidate()
        {
            if (autoUpdate)
            {
                NotifyOfUpdate();
            }
        }

        public void NotifyOfUpdate()
        {
            if (OnValuesUpdated != null)
            {
                OnValuesUpdated();
            }
        }
    }
}