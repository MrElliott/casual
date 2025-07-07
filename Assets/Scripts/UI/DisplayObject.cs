using ScriptableObjectS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayObject : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _text;
    
    [SerializeField] private PlaceableSO placeableObject;

    private Transform _highlight;
    
    private void Awake()
    {
        //_image = GetComponent<Image>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetupImage();
        _highlight = transform.Find("highlight");
    }

    public void SetDisplay(PlaceableSO p)
    {
        placeableObject = p;
        SetupImage();
    }

    private void SetupImage()
    {
        Texture2D texture2D = RuntimePreviewGenerator.GenerateModelPreview(placeableObject.Prefab);
        Sprite disSprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.one);
        _image.sprite = disSprite;
        _text.text = placeableObject.DisplayName;
    }

    public void ShowHighlight(bool show)
    {
        //Debug.Log($"Highlight called: {show}");
        _highlight.gameObject.SetActive(show);
    }

    public PlaceableSO GetPlaceableObject()
        => placeableObject;
}
