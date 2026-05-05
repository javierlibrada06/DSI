using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;
using UnityEngine;

public class InventorySlotManipulator : PointerManipulator
{
    private readonly int _index;
    private readonly System.Action<int, Vector2> _onDragStart;
    private readonly System.Action<int> _onDrop;

    private bool _down;
    private Vector2 _start;

    private const float DRAG_THRESHOLD = 8f;

    public InventorySlotManipulator(
        int index,
        System.Action<int, Vector2> onDragStart,
        System.Action<int> onDrop)
    {
        _index = index;
        _onDragStart = onDragStart;
        _onDrop = onDrop;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnDown);
        target.RegisterCallback<PointerMoveEvent>(OnMove);
        target.RegisterCallback<PointerUpEvent>(OnUp);
    }

    private void OnDown(PointerDownEvent evt)
    {
        if (evt.button != 0) return;
        _down = true;
        _start = evt.position;
    }

    private void OnMove(PointerMoveEvent evt)
    {
        if (!_down) return;

        if (Vector2.Distance(evt.position, _start) > DRAG_THRESHOLD)
            _onDragStart?.Invoke(_index, evt.position);
    }

    private void OnUp(PointerUpEvent evt)
    {
        if (!_down) return;
        _down = false;
        _onDrop?.Invoke(_index);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnDown);
        target.UnregisterCallback<PointerMoveEvent>(OnMove);
        target.UnregisterCallback<PointerUpEvent>(OnUp);
    }
}