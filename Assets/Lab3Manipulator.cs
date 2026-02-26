using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class Lab3Manipulator : MouseManipulator
{
    private static List<Lab3Manipulator> allManipulators = new List<Lab3Manipulator>();

    private bool isSelected = false;

    public Lab3Manipulator()
    {
        // No necesitamos activator para seleccionar
        activators.Add(new ManipulatorActivationFilter
        {
            button = MouseButton.LeftMouse
        });
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        target.RegisterCallback<WheelEvent>(OnMouseWheel);

        if (!allManipulators.Contains(this))
            allManipulators.Add(this);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
        target.UnregisterCallback<WheelEvent>(OnMouseWheel);

        if (allManipulators.Contains(this))
            allManipulators.Remove(this);
    }

    private void OnPointerEnter(PointerEnterEvent evt)
    {
        Select();
    }

    private void OnMouseWheel(WheelEvent evt)
    {
        if (!isSelected)
            return;

        float delta = -evt.delta.y * 5f;

        float newWidth = target.resolvedStyle.width + delta;
        float newHeight = target.resolvedStyle.height + delta;

        if (newWidth < 40) newWidth = 40;
        if (newHeight < 40) newHeight = 40;

        target.style.width = newWidth;
        target.style.height = newHeight;

        evt.StopPropagation();
    }

    private void Select()
    {
        foreach (var manip in allManipulators)
        {
            manip.DeselectInternal();
        }

        isSelected = true;
        target.AddToClassList("selected");
    }

    private void DeselectInternal()
    {
        isSelected = false;
        target.RemoveFromClassList("selected");
    }
}