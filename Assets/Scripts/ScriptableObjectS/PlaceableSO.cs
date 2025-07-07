using UnityEngine;

namespace ScriptableObjectS
{
    [CreateAssetMenu(fileName = "PlaceableSO", menuName = "Scriptable Objects/PlaceableSO")]
    public class PlaceableSO : ScriptableObject{
        [Header("Basic Information")]
        public string DisplayName;
        public Transform Prefab;
        public PlaceableType Type;    
    
        [Header("Transformations")]
        public bool Scalable;
        public bool Rotatable;
        public bool Translatable;
    
        [Header("Transformation Data")]
        public bool RotationStepping = false;
        public float RotationStep;
    }

    public enum PlaceableType{
        Base,
        Building,
        Prop
    }
}