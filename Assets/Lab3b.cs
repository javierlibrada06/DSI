using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Lab3b : MonoBehaviour
{
    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        VisualElement izda = root.Q("Izquierda");
        VisualElement dcha = root.Q("Derecha");

        izda.AddManipulator(new Lab3Manipulator());
        dcha.AddManipulator(new Lab3Manipulator());

        List<VisualElement> lveizda = izda.Children().ToList();
        List<VisualElement> lvedcha = dcha.Children().ToList();

        lveizda.ForEach(elem => elem.AddManipulator(new Lab3Manipulator()));
        lvedcha.ForEach(elem => elem.AddManipulator(new Lab3Manipulator()));

        //lveizda.ForEach(elem => elem.AddManipulator(new ExampleDragger()));
        //lvedcha.ForEach(elem => elem.AddManipulator(new ExampleDragger()));


        izda.RegisterCallback<MouseDownEvent>(
            ev =>
            {
            Debug.Log("Contenedor Izquierda. Fase:" + ev.propagationPhase);

            Debug.Log("Contenedor Izquierda. Target:" + (ev.target as VisualElement).name);

            //(ev.target as VisualElement).style.backgroundColor = Color.green;
            }, TrickleDown.TrickleDown);

        dcha.RegisterCallback<MouseDownEvent>(ev =>
        {
            Debug.Log("Contenedor Derecha. Fase:" + ev.propagationPhase);

            Debug.Log("Contenedor Derecha. Target:" + (ev.target as VisualElement).name);

            //(ev.target as VisualElement).style.backgroundColor = Color.blue;
        }, TrickleDown.TrickleDown);
    }
}

