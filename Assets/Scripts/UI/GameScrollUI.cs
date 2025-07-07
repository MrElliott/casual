using System.Collections.Generic;
using ScriptableObjectS;
using UnityEngine;
using UnityEngine.UI;

public class GameScrollUI : MonoBehaviour
{
    [SerializeField] private Transform selectablePrefab;

    [SerializeField] private Transform scrollContentTransform;

    [SerializeField] private ScriptableObjectDatabase<PlaceableSO> placeableObjects;

    [SerializeField] private float yPadding = 10f;
    
    BuildingManager buildingManager;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buildingManager = FindObjectOfType<BuildingManager>();
        selectablePrefab.GetComponent<RectTransform>();

        GenerateScrollUI();
    }

    private void GenerateScrollUI()
    {
        List<PlaceableSO> placeables = placeableObjects.GetAllObjects();
        
        foreach (PlaceableSO placeable in placeables)
        {
            Transform c = Instantiate(selectablePrefab, scrollContentTransform);
            c.GetComponent<DisplayObject>().SetDisplay(placeable);
            c.GetComponent<Button>().onClick.AddListener(() =>
            {
                buildingManager.SetPlaceable(placeable);
            });
            //c.localPosition = new Vector3(-(rectTransform.rect.width/2), -(rectTransform.rect.height * i) + (rectTransform.rect.height + yPadding), 0);
        }
    }
}
