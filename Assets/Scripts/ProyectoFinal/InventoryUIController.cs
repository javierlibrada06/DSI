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

        private ItemDatabase _db;

        public void Init(VisualElement root, ItemDatabase db)
        {
            _root = root;
            _invGrid = root.Q("inv-grid");
            _db = db;

            CreateDragGhost();

            _root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        }

        public void Refresh()
        {
            var data = GameSession.Instance.Data;
            data.EnsureInventorySize(); 
            var inv = data.inventory;

            var slots = _invGrid.Query(className: "inv-slot").ToList();

            ClearManipulators(slots);

            for (int i = 0; i < slots.Count; i++)
            {
                int idx = i;
                var slot = slots[i];

                slot.Clear();
                slot.RemoveFromClassList("inv-slot--empty");

                InventoryItemData item = null;

                if (i < inv.Count)
                    item = inv[i];

                if (item != null)
                {
                    var def = _db.Get(item.itemId);

                    if (def != null)
                    {
                        var icon = new VisualElement();
                        icon.style.backgroundImage = new StyleBackground(def.icon);
                        icon.style.width = 48;
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

                var manip = new InventorySlotManipulator(
                    idx,
                    StartDrag,
                    DropOnSlot
                );

                slot.AddManipulator(manip);
                _manips.Add(manip);
            }
        }

        private void StartDrag(int index, Vector2 pos)
        {
            var inv = GameSession.Instance.Data.inventory;
            if (inv[index] == null) return;

            _dragSourceIndex = index;

            SetVisible(_dragGhost, true);
            MoveGhost(pos);
        }

        private void DropOnSlot(int targetIndex)
        {
            SetVisible(_dragGhost, false);

            var inv = GameSession.Instance.Data.inventory;

            if (_dragSourceIndex < 0 || targetIndex < 0 ||
                _dragSourceIndex >= inv.Count || targetIndex >= inv.Count)
                return;

            var source = inv[_dragSourceIndex];
            var target = inv[targetIndex];

            if (source == null) return;

            var def = _db.Get(source.itemId);

            // 🔥 merge
            if (target != null && target.itemId == source.itemId)
            {
                int space = def.maxStack - target.count;
                int move = Mathf.Min(space, source.count);

                target.count += move;
                source.count -= move;

                if (source.count <= 0)
                    inv[_dragSourceIndex] = null;
            }
            else
            {
                // 🔁 swap
                inv[_dragSourceIndex] = target;
                inv[targetIndex] = source;
            }

            GameSession.Instance.Save();
            _dragSourceIndex = -1;
            Refresh();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_dragSourceIndex >= 0)
                MoveGhost(evt.position);
        }

        private void MoveGhost(Vector2 pos)
        {
            _dragGhost.style.left = pos.x - 32;
            _dragGhost.style.top = pos.y - 32;
        }

        private void CreateDragGhost()
        {
            _dragGhost = new VisualElement();
            _dragGhost.AddToClassList("inv-drag-ghost");
            _dragGhost.style.display = DisplayStyle.None;

            _root.Add(_dragGhost);
        }

        private void ClearManipulators(List<VisualElement> slots)
        {
            for (int i = 0; i < _manips.Count && i < slots.Count; i++)
                slots[i].RemoveManipulator(_manips[i]);

            _manips.Clear();
        }

        private void SetVisible(VisualElement el, bool v)
        {
            el.style.display = v ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}