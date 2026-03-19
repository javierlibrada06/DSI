using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using DSI.Lab5;
using System.Linq;
using System;
using Lab6_namespace;

public class Lab5 : MonoBehaviour
{
    public List<Sprite> misFotos;

    public VisualTreeAsset tarjetaTemplate;

    List<VisualElement> tarjetaVisual = new List<VisualElement>();
    List<Tarjeta> logicaTarjeta = new List<Tarjeta>();
    List<Individuo> usuario = new List<Individuo>();
    TextField inputNombre;
    TextField inputApellido;
    Button create;
    Button change;
    VisualElement derecha;
    int current = 0;
    string currentName;
    string currentSurname;
    Sprite currentPhoto;

    private void OnEnable()
    {
        CargarJSON();
        var root = GetComponent<UIDocument>().rootVisualElement;


        derecha = root.Q("Right");
        inputNombre = root.Q<TextField>("inputName");
        inputApellido = root.Q<TextField>("inputSurname");
        create = root.Q<Button>("create");
        change = root.Q<Button>("change");

        for (int i = 0; i < usuario.Count; i++)
        {
            CrearTarjeta(usuario[i]);
        }

        // 3. Galería
        VisualElement contenedorGaleria = root.Q("PhotoContainer");
        if (contenedorGaleria != null)
        {
            List<VisualElement> opcionesFoto = contenedorGaleria.Children().ToList();
            for (int i = 0; i < opcionesFoto.Count; i++)
            {
                if (i < misFotos.Count)
                {
                    opcionesFoto[i].userData = misFotos[i];
                    opcionesFoto[i].style.backgroundImage = new StyleBackground(misFotos[i]);
                    opcionesFoto[i].RegisterCallback<PointerDownEvent>(AlPulsarFotoGaleria);
                }
            }
        }

        // 4. Eventos de Texto (FUERA del bucle de tarjetas)
        inputNombre.RegisterCallback<ChangeEvent<string>>(e =>
        {
           currentName = e.newValue;
            usuario[current].nombre = e.newValue;
            logicaTarjeta[current].Actualizar(usuario[current]);
        });

        inputApellido.RegisterCallback<ChangeEvent<string>>(e =>
        {
            currentSurname = e.newValue;
            usuario[current].apellido = e.newValue;
            logicaTarjeta[current].Actualizar(usuario[current]);

        });

        create.RegisterCallback<ClickEvent>(NuevaTarjeta);
        change.RegisterCallback<ClickEvent>(CambiarTarjeta);
    }

    private void AlPulsarFotoGaleria(PointerDownEvent evt)
    {
        VisualElement elementoPulsado = evt.currentTarget as VisualElement;
        if (elementoPulsado != null && elementoPulsado.userData is Sprite fotoSeleccionada)
        {
            currentPhoto = fotoSeleccionada;
        }
    }

    private void CrearTarjeta(Individuo datos) 
    {
        VisualTreeAsset plantilla = Resources.Load<VisualTreeAsset>("Tarjeta");
        VisualElement nuevaTarjeta = plantilla.Instantiate();
        
        derecha.Add(nuevaTarjeta);

        tarjetaVisual.Add(nuevaTarjeta);

        int i = tarjetaVisual.Count - 1;

        logicaTarjeta.Add(new Tarjeta(nuevaTarjeta));

        usuario[i] = datos;

        logicaTarjeta[i].Actualizar(usuario[i]);

        nuevaTarjeta.userData = i;

        nuevaTarjeta.RegisterCallback<PointerDownEvent>(ev =>
        {
            current = (int)nuevaTarjeta.userData;

            inputNombre.SetValueWithoutNotify(usuario[current].nombre);
            inputApellido.SetValueWithoutNotify(usuario[current].apellido);
        });
    }

    private void NuevaTarjeta(ClickEvent evt)
    {
        // 1. Crear la tarjeta visual desde plantilla
        VisualTreeAsset plantilla = Resources.Load<VisualTreeAsset>("Tarjeta");
        VisualElement nuevaTarjeta = plantilla.Instantiate();

        // 2. Añadirla al contenedor derecha
        derecha.Add(nuevaTarjeta);
        Debug.Log(derecha.childCount);

        // 3. Añadir a listas
        tarjetaVisual.Add(nuevaTarjeta);

        int i = tarjetaVisual.Count - 1;

        logicaTarjeta.Add(new Tarjeta(nuevaTarjeta));
        usuario.Add(new Individuo(currentName, currentSurname, currentPhoto));

        // 4. Asignar índice
        nuevaTarjeta.userData = i;

        // 5. Actualizar visual
        logicaTarjeta[i].Actualizar(usuario[i]);

        // 6. Evento de click
        nuevaTarjeta.RegisterCallback<PointerDownEvent>(ev =>
        {
            current = (int)nuevaTarjeta.userData;

            inputNombre.SetValueWithoutNotify(usuario[current].nombre);
            inputApellido.SetValueWithoutNotify(usuario[current].apellido);

        });

        GuardarJSON();
    }

    private void CambiarTarjeta(ClickEvent evt)
    {
        GuardarJSON();
    }

    void GuardarJSON()
    {
        string json = JsonHelperIndividuo.ToJson(usuario, true);

        string path = Application.persistentDataPath + "/individuos.json";
        System.IO.File.WriteAllText(path, json);

        Debug.Log("Guardado en: " + path);
    }

    void CargarJSON()
    {
        string path = Application.persistentDataPath + "/individuos.json";

        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            usuario = JsonHelperIndividuo.FromJson<Individuo>(json);
        }
        else
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("individuos");

            if (jsonFile != null)
            {
                usuario = JsonHelperIndividuo.FromJson<Individuo>(jsonFile.text);
            }
            else
            {
                usuario = new List<Individuo>();
            }
        }
    }
}