using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RootsOfLife
{
    [RequireComponent(typeof(UIDocument))]
    public class GameUIController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private ItemDatabase itemDatabase;

        // ── UI ROOT ───────────────────────────────
        private VisualElement _root;

        // ── TABS ─────────────────────────────────
        private readonly string[] _tabNames = { "mapa", "inventario", "enciclopedia", "mejoras", "ajustes" };
        private Dictionary<string, Button> _tabs = new();
        private Dictionary<string, VisualElement> _contents = new();
        private string _activeTab = "mapa";

        // ── PANEL LATERAL ────────────────────────
        private Label _panelTitle;
        private Label _panelDesc;
        private VisualElement _upgInfo;
        private Button _panelBack;

        // ── MAPA ─────────────────────────────────
        private VisualElement _mapArea;
        private VisualElement _islandCentral;
        private VisualElement _islandOeste;
        private VisualElement _islandEste;

        private string _selectedIsland = "";

        // ── INVENTARIO (NUEVO SISTEMA) ───────────
        private InventoryUIController _inventoryUI;

        // ── MEJORAS ──────────────────────────────
        private string _selectedTool = "";

        private static readonly List<ToolDef> Tools = new()
        {
            new("regadera", "Regadera", "💧"),
            new("hacha",    "Hacha",    "🪓"),
            new("pico",     "Pico",     "⛏️"),
            new("azada",    "Azada",    "🔨"),
        };

        private static readonly Dictionary<string, IslandDef> Islands = new()
        {
            ["central"] = new("Isla Central", "La isla principal del archipiélago."),
            ["oeste"] = new("Isla Oeste", "Bosques densos y misteriosos."),
            ["este"] = new("Isla Este", "Rica en minerales."),
        };

        // ════════════════════════════════════════
        // UNITY
        // ════════════════════════════════════════

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            BindElements();
            RegisterCallbacks();

            // 🔥 INVENTARIO (nuevo)
            _inventoryUI = new InventoryUIController();
            _inventoryUI.Init(_root, itemDatabase);

            SwitchTab("mapa");
        }

        // ════════════════════════════════════════
        // BINDING
        // ════════════════════════════════════════

        private void BindElements()
        {
            foreach (var name in _tabNames)
            {
                _tabs[name] = _root.Q<Button>($"tab-{name}");
                _contents[name] = _root.Q($"content-{name}");
            }

            _panelTitle = _root.Q<Label>("panel-title");
            _panelDesc = _root.Q<Label>("panel-desc");
            _upgInfo = _root.Q("upg-info");
            _panelBack = _root.Q<Button>("panel-back");

            _mapArea = _root.Q("map-area");
            _islandCentral = _root.Q("island-central");
            _islandOeste = _root.Q("island-oeste");
            _islandEste = _root.Q("island-este");
        }

        private void RegisterCallbacks()
        {
            // Tabs
            foreach (var name in _tabNames)
            {
                string n = name;
                _tabs[n]?.RegisterCallback<ClickEvent>(_ => SwitchTab(n));
            }

            // Islas
            _islandCentral?.RegisterCallback<ClickEvent>(_ => SelectIsland("central"));
            _islandOeste?.RegisterCallback<ClickEvent>(_ => SelectIsland("oeste"));
            _islandEste?.RegisterCallback<ClickEvent>(_ => SelectIsland("este"));

            _panelBack?.RegisterCallback<ClickEvent>(_ => DeselectAll());
        }

        // ════════════════════════════════════════
        // TABS
        // ════════════════════════════════════════

        private void SwitchTab(string tab)
        {
            _activeTab = tab;
            _selectedIsland = "";
            _selectedTool = "";

            foreach (var name in _tabNames)
            {
                bool active = name == tab;
                _tabs[name]?.EnableInClassList("tab--active", active);
                SetVisible(_contents[name], active);
            }

            // 🔥 INVENTARIO NUEVO
            if (tab == "inventario")
                _inventoryUI.Refresh();

            var (title, desc) = tab switch
            {
                "mapa" => ("Selección", "Explora las islas."),
                "inventario" => ("Inventario", "Organiza tus recursos."),
                "enciclopedia" => ("Enciclopedia", "Descubre el mundo."),
                "mejoras" => ("Mejoras", "Mejora tus herramientas."),
                "ajustes" => ("Ajustes", "Configura el juego."),
                _ => ("", "")
            };

            SetPanelText(title, desc);
            SetVisible(_upgInfo, false);
            SetVisible(_panelBack, false);
        }

        // ════════════════════════════════════════
        // MAPA
        // ════════════════════════════════════════

        private void SelectIsland(string id)
        {
            _selectedIsland = id;

            if (Islands.TryGetValue(id, out var island))
                SetPanelText(island.Name, island.Desc);

            SetVisible(_panelBack, true);
        }

        private void DeselectAll()
        {
            _selectedIsland = "";
            SwitchTab(_activeTab);
        }

        // ════════════════════════════════════════
        // HELPERS
        // ════════════════════════════════════════

        private void SetPanelText(string title, string desc)
        {
            if (_panelTitle != null) _panelTitle.text = title;
            if (_panelDesc != null) _panelDesc.text = desc;
        }

        private static void SetVisible(VisualElement el, bool visible)
        {
            if (el == null) return;
            el.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    // ════════════════════════════════════════
    // AUX
    // ════════════════════════════════════════

    internal sealed class ToolDef
    {
        public string Id;
        public string Name;
        public string Icon;

        public ToolDef(string id, string name, string icon)
        {
            Id = id;
            Name = name;
            Icon = icon;
        }
    }

    internal sealed class IslandDef
    {
        public string Name;
        public string Desc;

        public IslandDef(string name, string desc)
        {
            Name = name;
            Desc = desc;
        }
    }
}