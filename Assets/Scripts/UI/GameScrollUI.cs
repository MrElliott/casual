using UnityEngine;
using UnityEngine.UI;

public class GameScrollUI : MonoBehaviour
{
    [SerializeField] private Transform selectablePrefab;
    
    private RectTransform rectTransform;
    
    [SerializeField] private Transform scrollContentTransform;
    
    [SerializeField] private PlaceableSO placeableObject;

    [SerializeField] private float yPadding = 10f;
    private int objsToSpawn = 10;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rectTransform = selectablePrefab.GetComponent<RectTransform>();

        GenerateScrollUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateScrollUI()
    {
        for (int i = 0; i < objsToSpawn; i++)
        {
            Transform c = Instantiate(selectablePrefab, scrollContentTransform);
            c.GetComponent<DisplayObject>().SetDisplay(placeableObject);
            //c.localPosition = new Vector3(-(rectTransform.rect.width/2), -(rectTransform.rect.height * i) + (rectTransform.rect.height + yPadding), 0);
        }
    }
}
