using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class FontSizeManager : MonoBehaviour
{
    [Header("Configuración de UI Toolkit")]
    [SerializeField] private UIDocument uiDocument;

    // Diccionario para almacenar el tamaño original de cada elemento
    private Dictionary<VisualElement, float> originalSizes = new Dictionary<VisualElement, float>();
    private VisualElement root;
    private bool isInitialized = false;

    void Start()
    {
        // Iniciamos con un pequeño retraso para asegurar que UI Builder cargó todo
        Invoke(nameof(InitializeUI), 0.1f);
    }

    void InitializeUI()
    {
        if (uiDocument == null)
        {
            Debug.LogError("FontSizeManager: No has asignado el UIDocument en el Inspector.");
            return;
        }

        root = uiDocument.rootVisualElement;

        if (root != null)
        {
            Slider slider = root.Q<Slider>("slider-textsize");

            if (slider != null)
            {
                // 2. Suscríbete al evento de cambio de valor
                slider.RegisterValueChangedCallback(evt =>
                {
                    SetTextMultiplier(evt.newValue);
                });
            }
                // Buscamos Labels, Buttons y TextField que tienen texto
                root.Query<VisualElement>().ForEach(el =>
            {
                // Solo guardamos elementos que tengan un tamaño de fuente definido
                float size = el.resolvedStyle.fontSize;
                if (size > 0)
                {
                    originalSizes[el] = size;
                }
            });

            isInitialized = true;
            Debug.Log($"FontSizeManager: Se han registrado {originalSizes.Count} elementos de texto.");
        }
    }

    /// <summary>
    /// Método para conectar al Slider. 
    /// El valor del slider debe ser el multiplicador (ej: de 0.5 a 2.0)
    /// </summary>
    public void SetTextMultiplier(float multiplier)
    {
        if (!isInitialized) return;

        foreach (var entry in originalSizes)
        {
            VisualElement element = entry.Key;
            float baseSize = entry.Value;

            if (element != null)
            {
                // Aplicamos el multiplicador al tamaño que guardamos al principio
                element.style.fontSize = new StyleLength(new Length(baseSize * multiplier, LengthUnit.Pixel));
            }
        }
    }
}