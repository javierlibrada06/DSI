using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RootsOfLife
{
    public class InventoryUIController
    {
        private VisualElement _root;
        private VisualElement _invGrid;

        private VisualElement _dragGhost;
        private int _dragSourceIndex = -1;

        private readonly List<InventorySlotManipulator> _manips = new();
        private List<VisualElement> _slots = new();  // caché para hit-test

        private ItemDatabase _db;

        public void Init(VisualElement root, ItemDatabase db)
        {
            _root    = root;
            _invGrid = root.Q("inv-grid");
            _db      = db;

            CreateDragGhost();
            _root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        }

        // ── Refresh ────────────────────────────────────────────────────────────

        public void Refresh()
        {
            var data = GameSession.Instance.Data;
            data.EnsureInventorySize();
            var inv = data.inventory;

            _slots = _invGrid.Query(className: "inv-slot").ToList();

            ClearManipulators();

            for (int i = 0; i < _slots.Count; i++)
            {
                int idx  = i;
                var slot = _slots[i];

                slot.Clear();
                slot.RemoveFromClassList("inv-slot--empty");

                InventoryItemData item = (i < inv.Count) ? inv[i] : null;

                if (item != null)
                {
                    var def = _db.Get(item.itemId);
                    if (def != null)
                    {
                        var icon = new VisualElement();
                        icon.style.backgroundImage = new StyleBackground(def.icon);
                        icon.style.width  = 48;
                        icon.style.height = 48;
                        slot.Add(icon);
                    }

                    var count = new Label($"x{item.count}");
                    count.AddToClassList("item-count");
                    slot.Add(count);
                }
                else
                {
                    slot.AddToClassList("inv-slot--empty");
                }

                // Pasamos _slots como referencia para el hit-test en el drop
                var manip = new InventorySlotManipulator(idx, StartDrag, DropOnSlot, _slots);
                slot.AddManipulator(manip);
                _manips.Add(manip);
            }
        }

        // ── Drag ───────────────────────────────────────────────────────────────

        private void StartDrag(int index, Vector2 pos)
        {
            var inv = GameSession.Instance.Data.inventory;
            if (index < 0 || index >= inv.Count || inv[index] == null) return;

            _dragSourceIndex = index;
            SetVisible(_dragGhost, true);
            MoveGhost(pos);
        }

        // Firma actualizada: recibe (sourceIndex, targetIndex)
        private void DropOnSlot(int sourceIndex, int targetIndex)
        {
            SetVisible(_dragGhost, false);

            var inv = GameSession.Instance.Data.inventory;

            if (sourceIndex < 0 || targetIndex < 0 ||
                sourceIndex >= inv.Count || targetIndex >= inv.Count ||
                sourceIndex == targetIndex)
            {
                _dragSourceIndex = -1;
                return;
            }

            var source = inv[sourceIndex];
            var target = inv[targetIndex];

            if (source == null) { _dragSourceIndex = -1; return; }

            var def = _db.Get(source.itemId);

            if (target != null && target.itemId == source.itemId && def != null)
            {
                // Merge: rellenar stack destino
                int space = def.maxStack - target.count;
                int move  = Mathf.Min(space, source.count);
                target.count += move;
                source.count -= move;
                if (source.count <= 0)
                    inv[sourceIndex] = null;
            }
            else
            {
                // Swap
                inv[sourceIndex] = target;
                inv[targetIndex] = source;
            }

            GameSession.Instance.Save();
            _dragSourceIndex = -1;
            Refresh();
        }

        // ── Ghost ──────────────────────────────────────────────────────────────

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_dragSourceIndex >= 0)
                MoveGhost(evt.position);
        }

        private void MoveGhost(Vector2 pos)
        {
            _dragGhost.style.left = pos.x - 32;
            _dragGhost.style.top  = pos.y - 32;
        }

        private void CreateDragGhost()
        {
            _dragGhost = new VisualElement();
            _dragGhost.AddToClassList("inv-drag-ghost");
            _dragGhost.style.display = DisplayStyle.None;
            _dragGhost.pickingMode   = PickingMode.Ignore;  // no bloquea hit-test
            _root.Add(_dragGhost);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void ClearManipulators()
        {
            for (int i = 0; i < _manips.Count && i < _slots.Count; i++)
                _slots[i].RemoveManipulator(_manips[i]);
            _manips.Clear();
        }

        private static void SetVisible(VisualElement el, bool v)
        {
            if (el != null)
                el.style.display = v ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
