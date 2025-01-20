using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

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
        if (Input.GetMouseButtonDown(0)){
            
            // Create a ray from the camera to the mouse position
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform a raycast and check if it hits a collider
            if (Physics.Raycast(ray, out hit)){
                // Get the GameObject that was hit
                GameObject clickedObject = hit.collider.gameObject;

                // Set the active placeable
                SetActivePlaceable(clickedObject);
                
                // Perform your desired action with the clicked object
                Debug.Log("Clicked on: " + clickedObject.name);
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
}

public struct PlaceableStruct{
    public Transform transform;
    public PlaceableSO placeableSO;
}
