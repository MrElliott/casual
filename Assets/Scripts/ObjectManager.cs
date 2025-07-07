using ScriptableObjectS;
using UnityEngine;

public class ObjectManager : MonoBehaviour{
    
    private Transform baseTransform;

    private Transform buildingTransform;
    
    private Transform propsTransform;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        baseTransform = transform.Find("Base");
        if (baseTransform == null){
            baseTransform = new GameObject("Base").transform;
            baseTransform.parent = transform;
        }

        buildingTransform = transform.Find("Building");
        if (buildingTransform == null){
            buildingTransform = new GameObject("Building").transform;
            buildingTransform.parent = transform;
        }
        
        propsTransform = transform.Find("Props");
        if (propsTransform == null){
            propsTransform = new GameObject("Props").transform;
            propsTransform.parent = transform;
        }
    }

    public Transform CreatePlaceable(PlaceableSO prefab, Vector3 position, Quaternion rotation){
        Transform parent = prefab.Type == PlaceableType.Base ? baseTransform : prefab.Type == PlaceableType.Building ? buildingTransform : propsTransform;
        
        Transform t = Instantiate(prefab.Prefab, position, rotation, parent);

        return t;
    }
}
