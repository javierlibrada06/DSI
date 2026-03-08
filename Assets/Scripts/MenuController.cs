using UnityEngine;
using UnityEngine.UIElements;

public class MenuController : MonoBehaviour
{
    public Sprite cropSprites;
    private int plants = 0;
    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var cropImage = root.Q<VisualElement>("cropImage");

        Debug.Log("UI cargada correctamente");

        var header = root.Q<VisualElement>("header"); // Buscar con Q
        Debug.Log("Header encontrado: " + header);

        var titleLabel = header.Query<Label>().AtIndex(1);

        var body = root.Q<VisualElement>("body"); // Buscar con Q
        Debug.Log("body encontrado: " + body);

        foreach (var child in body.Children()) // Children
        {
            Debug.Log("Hijo del body: " + child.name);
            child.RegisterCallback<ClickEvent>(evt =>
             {
                 SetHighlight(child, body);
                 Debug.Log("Hightlight " + child.name);
             });
        }

        var left = body.Query<VisualElement>().AtIndex(0);
        var center = body.Query<VisualElement>().AtIndex(1);
        var right = body.Query<VisualElement>().AtIndex(2);


        var footer = root.Q<VisualElement>("footer");
        Debug.Log("footer encontrado: " + footer);

        foreach (var child in footer.Children()) // Children
        {
            child.RegisterCallback<ClickEvent>(evt =>
            {
                child.style.backgroundImage = new StyleBackground(cropSprites); //  Se cambia el sprite del crop
                plants++;
                titleLabel.text = "Plantas: " + plants;
                Debug.Log("Hightlight " + child.name);
            });
        }


    }

    void SetHighlight(VisualElement selected, VisualElement body)
    {
        foreach (var child in body.Children())
        {
            child.RemoveFromClassList("highlight");
        }

        selected.AddToClassList("highlight");
    }


}



