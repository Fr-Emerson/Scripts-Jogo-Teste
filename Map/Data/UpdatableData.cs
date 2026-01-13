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
                UnityEditor.EditorApplication.update += NotifyOfUpdate;
            }
        }

        public void NotifyOfUpdate()
        {
            UnityEditor.EditorApplication.update -= NotifyOfUpdate;
            if (OnValuesUpdated != null)
            {
                OnValuesUpdated();
            }
        }
    }
}