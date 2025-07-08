using System.Collections.Generic;
using ScriptableObjectS;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class BuildingManager : MonoBehaviour
{
    [SerializeField]
    private ObjectManager objectManager;
    
    [SerializeField]
    private PlaceableSO basePlaceableSO;
    
    [SerializeField]
    private CinemachineOrbitController orbitController;

    private Camera _mainCamera;

    private PlaceableStruct basePlaceable;
    
    private List<PlaceableStruct> placeables = new List<PlaceableStruct>();
    
    private PlaceableStruct activePlaceable;


    public LayerMask collisionLayer; // Set this to specify which layers the raycast should check for collisions

    public bool inBuildMode = false; 
    [FormerlySerializedAs("buildingGhostPrefab")] [SerializeField]
    private GameObject buildingGhost;
    [SerializeField]
    private Material ghostMaterial;
    [SerializeField]
    private PlaceableSO activeBuildable;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start(){
        _mainCamera = Camera.main;
        
        basePlaceable = new PlaceableStruct(){
            transform = objectManager.CreatePlaceable(basePlaceableSO, Vector3.zero, quaternion.identity),
            placeableSO = basePlaceableSO
        };
        
        orbitController.SetTarget(basePlaceable.transform);
    }

    // Update is called once per frame
    void Update()
    {
        if(inBuildMode)
            UpdateGhostLocation();
        
        if (Input.GetMouseButtonDown(0)){
            
            // Create a ray from the camera to the mouse position
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform a raycast and check if it hits a collider
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, collisionLayer)){
                // Get the GameObject that was hit
                GameObject clickedObject = hit.collider.gameObject;

                if (inBuildMode)
                {
                    Transform t = Instantiate(activeBuildable.Prefab, hit.point, Quaternion.identity);
                    t.SetParent(clickedObject.transform);
                }
                else
                {
                    // Set the active placeable
                    SetActivePlaceable(clickedObject);
                
                    // Perform your desired action with the clicked object
                    Debug.Log("Clicked on: " + clickedObject.name);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R)){
            if (activePlaceable.placeableSO == null)
                return;
            
            if (activePlaceable.placeableSO.Rotatable){
                if (activePlaceable.placeableSO.RotationStepping){
                    activePlaceable.transform.Rotate(0f, activePlaceable.placeableSO.RotationStep, 0f);
                }
            }
        }
    }

    private void SetActivePlaceable(GameObject go){
        if (go.transform.name == basePlaceable.transform.name){
            activePlaceable = basePlaceable;
        }

        foreach (PlaceableStruct placeableStruct in placeables){
            if (placeableStruct.transform.name == go.transform.name){
                activePlaceable = placeableStruct;
            }
        }
        
        Debug.Log("Set Active Placeable: " + activePlaceable.transform.name);
    }

    public void SetPlaceable(PlaceableSO placeable)
    {
        if (placeable == null || activePlaceable.placeableSO == placeable)
        {
            inBuildMode = false;
            return;
        }

        activeBuildable = placeable;
        inBuildMode = true;
        UpdateGhost();
    }
    
    private void UpdateGhostLocation()
    {
        if(!buildingGhost.activeSelf)
            buildingGhost.SetActive(true);
        
        // Get the mouse position in screen space
        Vector3 mousePosition = Input.mousePosition;

        // Convert screen space to world space
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        
        // Set the z-coordinate to the object's current z to avoid altering its depth in 3D
        mousePosition.z = transform.position.z;

        // Cast a ray from the camera to the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Check if the ray hits anything
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, collisionLayer))
        {
            // Move the object to the hit point
            buildingGhost.transform.position = hit.point;
        }
        else
        {
            // If no object is hit, move the object to the mouse position
            buildingGhost.transform.position = mousePosition;
        }
    }

    private void UpdateGhost()
    {
        foreach (Transform child in buildingGhost.transform)
        {
            Destroy(child.gameObject); // Destroy the child GameObject
        }

        Transform ghostTransform = Instantiate(activeBuildable.Prefab, buildingGhost.transform);
        ghostTransform.GameObject().layer = LayerMask.NameToLayer("Ghost");
        Renderer ghostRenderer = ghostTransform.GetComponent<Renderer>();
        ghostRenderer.material = ghostMaterial;
    }

}

public struct PlaceableStruct{
    public Transform transform;
    public PlaceableSO placeableSO;
}
