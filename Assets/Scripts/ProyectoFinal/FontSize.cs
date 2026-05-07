using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class FontSizeManager : MonoBehaviour
{
    [Header("Configuración de UI Toolkit")]
    [SerializeField] private UIDocument uiDocument;

    private Dictionary<VisualElement, float> originalSizes = new Dictionary<VisualElement, float>();
    private VisualElement root;
    private bool isInitialized = false;

    void Start()
    {
        Invoke(nameof(InitializeUI), 0.1f);
    }

    void InitializeUI()
    {
        if (uiDocument == null)
        {
            return;
        }

        root = uiDocument.rootVisualElement;

        if (root != null)
        {
            Slider slider = root.Q<Slider>("slider-textsize");

            if (slider != null)
            {
                slider.RegisterValueChangedCallback(evt =>
                {
                    SetTextMultiplier(evt.newValue);
                });
            }
                root.Query<VisualElement>().ForEach(el =>
            {
                float size = el.resolvedStyle.fontSize;
                if (size > 0)
                {
                    originalSizes[el] = size;
                }
            });

            isInitialized = true;
        }
    }

    public void SetTextMultiplier(float multiplier)
    {
        if (!isInitialized) return;

        foreach (var entry in originalSizes)
        {
            VisualElement element = entry.Key;
            float baseSize = entry.Value;

            if (element != null)
            {
                element.style.fontSize = new StyleLength(new Length(baseSize * multiplier, LengthUnit.Pixel));
            }
        }
    }
}