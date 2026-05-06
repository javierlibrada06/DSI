using UnityEngine;
using UnityEngine.UIElements;

namespace RootsOfLife
{
    /// <summary>
    /// Manipulator para cada slot del inventario.
    /// Detecta drag-start y, al soltar, busca el slot real bajo el cursor
    /// usando WorldBoundingBox — no depende del índice del origen.
    /// </summary>
    public class InventorySlotManipulator : PointerManipulator
    {
        private readonly int _index;
        private readonly System.Action<int, Vector2> _onDragStart;
        private readonly System.Action<int, int> _onDrop;   // (source, target)
        private readonly System.Collections.Generic.List<VisualElement> _allSlots;

        private bool   _dragging;
        private Vector2 _startPos;

        private const float DRAG_THRESHOLD = 8f;

        public InventorySlotManipulator(
            int index,
            System.Action<int, Vector2> onDragStart,
            System.Action<int, int> onDrop,
            System.Collections.Generic.List<VisualElement> allSlots)
        {
            _index       = index;
            _onDragStart = onDragStart;
            _onDrop      = onDrop;
            _allSlots    = allSlots;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnDown, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerMoveEvent>(OnMove, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerUpEvent>  (OnUp,   TrickleDown.TrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnDown, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerMoveEvent>(OnMove, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerUpEvent>  (OnUp,   TrickleDown.TrickleDown);
        }

        private void OnDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
            _dragging = false;
            _startPos = evt.position;
            target.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnMove(PointerMoveEvent evt)
        {
            if (!target.HasPointerCapture(evt.pointerId)) return;

            if (!_dragging && Vector2.Distance(evt.position, _startPos) > DRAG_THRESHOLD)
            {
                _dragging = true;
                _onDragStart?.Invoke(_index, evt.position);
            }
            evt.StopPropagation();
        }

        private void OnUp(PointerUpEvent evt)
        {
            if (!target.HasPointerCapture(evt.pointerId)) return;
            target.ReleasePointer(evt.pointerId);

            if (!_dragging) { _dragging = false; return; }
            _dragging = false;

            int targetIdx = HitTestSlot(evt.position);
            _onDrop?.Invoke(_index, targetIdx);
            evt.StopPropagation();
        }

        private int HitTestSlot(Vector2 screenPos)
        {
            for (int i = 0; i < _allSlots.Count; i++)
            {
                var s = _allSlots[i];
                if (s != null && s.worldBound.Contains(screenPos))
                    return i;
            }
            return -1;
        }
    }
}
