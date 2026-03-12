using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using DSI.Lab5;
using System.Linq;
using System;

public class Lab5 : MonoBehaviour
{
    public List<Sprite> misFotos;

    List<VisualElement> tarjetaVisual = new List<VisualElement>();
    List<Tarjeta> logicaTarjeta = new List<Tarjeta>();
    List<Individuo> usuario = new List<Individuo>();
    TextField inputNombre;
    TextField inputApellido;
    int current = 0;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        VisualElement derecha = root.Q("Right");
        inputNombre = root.Q<TextField>("inputName");
        inputApellido = root.Q<TextField>("inputSurname");

        // 1. Obtener los elementos visuales
        tarjetaVisual = derecha.Children().ToList();

        // Limpiar listas por si acaso
        logicaTarjeta.Clear();
        usuario.Clear();

        // 2. Bucle correcto (sin el -1)
        for (int i = 0; i < tarjetaVisual.Count; i++)
        {
            // USAR .Add() en lugar de lista[i]
            logicaTarjeta.Add(new Tarjeta(tarjetaVisual[i]));
            usuario.Add(new Individuo("Name", "Surname", misFotos.Count > 0 ? misFotos[0] : null));

            // Guardamos el índice i en el userData de la tarjeta física
            tarjetaVisual[i].userData = i;

            logicaTarjeta[i].Actualizar(usuario[i]);

            // Al pulsar, recuperamos el índice desde el userData
            tarjetaVisual[i].RegisterCallback<PointerDownEvent>(ev => {
                VisualElement t = ev.currentTarget as VisualElement;
                current = (int)t.userData;

                // Actualizar inputs con los datos de LA tarjeta pulsada
                inputNombre.SetValueWithoutNotify(usuario[current].nombre);
                inputApellido.SetValueWithoutNotify(usuario[current].apellido);
                Debug.Log("Editando tarjeta: " + current);
            });
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
            usuario[current].nombre = e.newValue;
            logicaTarjeta[current].Actualizar(usuario[current]);
        });

        inputApellido.RegisterCallback<ChangeEvent<string>>(e =>
        {
            usuario[current].apellido = e.newValue;
            logicaTarjeta[current].Actualizar(usuario[current]);
        });
    }

    private void AlPulsarFotoGaleria(PointerDownEvent evt)
    {
        VisualElement elementoPulsado = evt.currentTarget as VisualElement;
        if (elementoPulsado != null && elementoPulsado.userData is Sprite fotoSeleccionada)
        {
            usuario[current].foto = fotoSeleccionada;
            logicaTarjeta[current].Actualizar(usuario[current]);
        }
    }
}