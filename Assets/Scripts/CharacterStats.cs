using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterStat : VisualElement
{
    VisualElement root;
    VisualElement iconsContainer;
    Label text;

    int valor;
    string name;
    string icono;

    public int Valor
    {
        get => valor;
        set
        {
            valor = Mathf.Clamp(value, 0, 6);
            actualizarIconos();
        }
    }

    public string Icono
    {
        get => icono;
        set
        {
            icono = value;
            actualizarIconos();
        }
    }

    public string Name
    {
        get => name;
        set
        {
            name = value;
            actualizarTexto();
        }
    }

    public CharacterStat()
    {
        var template = Resources.Load<VisualTreeAsset>("StatsContainer");
        root = template.Instantiate();

        Add(root);

        iconsContainer = root.Q<VisualElement>("Icons");
        text = root.Q<Label>("Name");
    }

    void actualizarIconos()
    {
        var icons = iconsContainer.Children().ToList();

        for (int i = 0; i < icons.Count; i++)
        {
            var icon = icons[i];

            if (i < valor)
            {
                icon.style.display = DisplayStyle.Flex;

                icon.style.backgroundImage =
                    new StyleBackground(Resources.Load<Sprite>(icono));
                icon.style.unityBackgroundImageTintColor = new Color(1f, 1f, 1f, 1f);

            }
            else
            {
                icon.style.display = DisplayStyle.Flex;
                icon.style.backgroundImage =
                    new StyleBackground(Resources.Load<Sprite>(icono));
                icon.style.unityBackgroundImageTintColor = new Color(1f, 1f, 1f, 0.2f);
            }
        }
    }

    void actualizarTexto()
    {
        text.text = name + ": " + valor;
    }
}