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

        [Header("Iconos de herramientas")]
        [SerializeField] private ToolIconEntry[] toolIcons = new ToolIconEntry[]
        {
            new() { toolId = "regadera" },
            new() { toolId = "hacha"    },
            new() { toolId = "pico"     },
            new() { toolId = "azada"    },
        };

        private VisualElement _root;

        private readonly string[] _tabNames =
        {
            "mapa", "inventario", "enciclopedia", "mejoras", "ajustes"
        };

        private Dictionary<string, Button> _tabs = new();
        private Dictionary<string, VisualElement> _contents = new();
        private string _activeTab = "mapa";

        // panel lateral
        private Label _panelTitle;
        private Label _panelDesc;
        private VisualElement _upgInfo;
        private Button _panelBack;

        // mejoras UI
        private Label _upgLevel;
        private Label _upgNext;
        private Label _upgCapCurr;
        private Label _upgCapNext;
        private Button _btnUpgrade;
        private Label _upgMaxLabel;

        // mapa
        private VisualElement _mapArea;
        private VisualElement _islandCentral;
        private VisualElement _islandOeste;
        private VisualElement _islandEste;
        private string _selectedIsland = "";

        // inventario
        private InventoryUIController _inventoryUI;
        private Button _btnAddSeed;
        private Button _btnAddCrop;

        // ajustes
        private Slider _sliderMusic;
        private Slider _sliderSfx;
        private Slider _sliderTextSize;
        private Label  _valMusic;
        private Label  _valSfx;
        private Label  _valTextSize;

        private string _selectedTool = "";

        private static readonly List<ToolDef> Tools = new()
        {
            new("regadera", "Regadera", "💧"),
            new("hacha", "Hacha", "🪓"),
            new("pico", "Pico", "⛏️"),
            new("azada", "Azada", "🔨"),
        };

        private static readonly Dictionary<string, IslandDef> Islands = new()
        {
            ["central"] = new("Isla Central", "La isla principal del archipiélago."),
            ["oeste"] = new("Isla Oeste", "Bosques densos y misteriosos."),
            ["este"] = new("Isla Este", "Rica en minerales."),
        };

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            BindElements();
            RegisterCallbacks();

            _inventoryUI = new InventoryUIController();
            _inventoryUI.Init(_root, itemDatabase);

            SwitchTab("mapa");
        }

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

            _upgLevel = _root.Q<Label>("upg-level");
            _upgNext = _root.Q<Label>("upg-next");
            _upgCapCurr = _root.Q<Label>("upg-cap-curr");
            _upgCapNext = _root.Q<Label>("upg-cap-next");
            _btnUpgrade = _root.Q<Button>("btn-upgrade");
            _upgMaxLabel = _root.Q<Label>("upg-max-label");

            _mapArea = _root.Q("map-area");
            _islandCentral = _root.Q("island-central");
            _islandOeste = _root.Q("island-oeste");
            _islandEste = _root.Q("island-este");

            // Ajustes
            _sliderMusic    = _root.Q<Slider>("slider-music");
            _sliderSfx      = _root.Q<Slider>("slider-sfx");
            _sliderTextSize = _root.Q<Slider>("slider-textsize");
            _valMusic    = _root.Q<Label>("val-music");
            _valSfx      = _root.Q<Label>("val-sfx");
            _valTextSize = _root.Q<Label>("val-textsize");

            _btnAddSeed = _root.Q<Button>("btn-add-seed");
            _btnAddCrop = _root.Q<Button>("btn-add-crop");
        }

        private void RegisterCallbacks()
        {
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

            _btnUpgrade?.RegisterCallback<ClickEvent>(_ => OnUpgradeTool());

            // herramientas
            foreach (var tool in Tools)
            {
                string id = tool.Id;

                _root.Q<Button>($"tool-icon-{id}")
                     ?.RegisterCallback<ClickEvent>(_ => SelectTool(id));
            }

            // ajustes
            _sliderMusic?.RegisterValueChangedCallback(evt =>
            {
                if (_valMusic != null) _valMusic.text = Mathf.RoundToInt(evt.newValue).ToString();
                AudioManager.Instance?.SetMusicVolume(evt.newValue);
                var data = GameSession.Instance?.Data;
                if (data != null) { data.settings.musicVolume = evt.newValue; GameSession.Instance.Save(); }
            });

            _sliderSfx?.RegisterValueChangedCallback(evt =>
            {
                if (_valSfx != null) _valSfx.text = Mathf.RoundToInt(evt.newValue).ToString();
                AudioManager.Instance?.SetSfxVolume(evt.newValue);
                var data = GameSession.Instance?.Data;
                if (data != null) { data.settings.sfxVolume = evt.newValue; GameSession.Instance.Save(); }
            });

            _sliderTextSize?.RegisterValueChangedCallback(evt =>
            {
                if (_valTextSize != null) _valTextSize.text = Mathf.RoundToInt(evt.newValue).ToString();
                var data = GameSession.Instance?.Data;
                if (data != null) { data.settings.textSize = evt.newValue; GameSession.Instance.Save(); }
            });

            _btnAddSeed?.RegisterCallback<ClickEvent>(_ => TryAddItem("seed"));
            _btnAddCrop?.RegisterCallback<ClickEvent>(_ => TryAddItem("crop"));
        }
        private void TryAddItem(string itemId)
        {
            var data = GameSession.Instance?.Data;
            if (data == null || itemDatabase == null) return;

            var def = itemDatabase.Get(itemId);
            if (def == null)
            {
                Debug.LogWarning($"Item '{itemId}' no encontrado en la base de datos.");
                return;
            }

            var inv = data.inventory;
            data.EnsureInventorySize();

            // Buscar slot del mismo tipo con espacio
            for (int i = 0; i < inv.Count; i++)
            {
                if (inv[i] != null && inv[i].itemId == itemId && inv[i].count < def.maxStack)
                {
                    inv[i].count++;
                    GameSession.Instance.Save();
                    _inventoryUI.Refresh();
                    return;
                }
            }

            // Buscar primer slot vacio
            for (int i = 0; i < inv.Count; i++)
            {
                if (inv[i] == null)
                {
                    inv[i] = new InventoryItemData { itemId = itemId, count = 1 };
                    GameSession.Instance.Save();
                    _inventoryUI.Refresh();
                    return;
                }
            }
            Debug.Log($"Inventario lleno, no se pudo añadir '{itemId}'.");
        }
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

            var (title, desc) = tab switch
            {
                "mapa" => ("Mapa", "Explora las islas."),
                "inventario" => ("Inventario", "Gestiona tus objetos."),
                "enciclopedia" => ("Enciclopedia", "Descubre criaturas."),
                "mejoras" => ("Mejoras", "Mejora tus herramientas."),
                "ajustes" => ("Ajustes", "Configura el juego."),
                _ => ("", "")
            };

            SetPanelText(title, desc);

            SetVisible(_upgInfo, false);
            SetVisible(_panelBack, false);

            if (tab == "inventario")
                _inventoryUI.Refresh();

            if (tab == "mejoras")
            {
                RefreshUpgradesUI();
                if (!string.IsNullOrEmpty(_selectedTool))
                    ShowUpgradeInfo(GameSession.Instance.Data.GetToolLevel(_selectedTool));
                else
                    SetVisible(_upgInfo, false);
            }
        }
        private void SelectTool(string id)
        {
            _selectedTool = id;

            foreach (var t in Tools)
                _root.Q<Button>($"tool-icon-{t.Id}")
                     ?.RemoveFromClassList("tool-icon--selected");

            _root.Q<Button>($"tool-icon-{id}")
                 ?.AddToClassList("tool-icon--selected");

            var tool = Tools.Find(t => t.Id == id);
            if (tool == null) return;

            int lvl = GameSession.Instance.Data.GetToolLevel(id);

            SetPanelText(tool.Name, "Mejora tu herramienta para ser más eficiente.");
            ShowUpgradeInfo(lvl);
        }

        private void ShowUpgradeInfo(int lvl)
        {
            SetVisible(_upgInfo, true);

            _upgLevel.text = $"Nivel: Lvl.{lvl}";

            bool max = lvl >= 5;

            SetVisible(_upgNext, !max);
            SetVisible(_upgCapCurr, !max);
            SetVisible(_upgCapNext, !max);
            SetVisible(_btnUpgrade, !max);
            SetVisible(_upgMaxLabel, max);

            if (!max)
            {
                _upgNext.text = $"Siguiente: 20 RC → Lvl {lvl + 1}";
                _upgCapCurr.text = $"Capacidad: {lvl * 10}";
                _upgCapNext.text = $"Nueva capacidad: {(lvl + 1) * 10}";
            }
        }

        private void OnUpgradeTool()
        {
            if (_selectedTool == null) return;

            if (!GameSession.Instance.Data.UpgradeTool(_selectedTool))
                return;

            GameSession.Instance.Save();

            RefreshUpgradesUI();
            ShowUpgradeInfo(GameSession.Instance.Data.GetToolLevel(_selectedTool)); // panel derecha
        }

        private void RefreshUpgradesUI()
        {
            foreach (var tool in Tools)
            {
                int lvl = GameSession.Instance.Data.GetToolLevel(tool.Id);

                // Nivel
                var label = _root.Q<Label>($"tool-lvl-{tool.Id}");
                if (label != null)
                    label.text = $"Lvl. {lvl}";

                // Icono desde Inspector
                var iconBtn = _root.Q($"tool-icon-{tool.Id}");
                if (iconBtn != null)
                {
                    var entry = System.Array.Find(toolIcons, e => e.toolId == tool.Id);
                    if (entry != null && entry.icon != null)
                    {
                        iconBtn.style.backgroundImage = new StyleBackground(entry.icon);
                        iconBtn.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                    }
                }

                // Barras de nivel
                var bars = _root.Q($"bars-{tool.Id}")
                    ?.Query(className: "level-bar").ToList();

                if (bars == null) continue;

                for (int i = 0; i < bars.Count; i++)
                {
                    bars[i].EnableInClassList("level-bar--active",  i < lvl);
                    bars[i].EnableInClassList("level-bar--current", i == lvl - 1);
                }
            }
        }
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


        private void SetPanelText(string title, string desc)
        {
            _panelTitle.text = title;
            _panelDesc.text = desc;
        }

        private static void SetVisible(VisualElement el, bool v)
        {
            if (el != null)
                el.style.display = v ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    internal class ToolDef
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

    internal class IslandDef
    {
        public string Name;
        public string Desc;

        public IslandDef(string name, string desc)
        {
            Name = name;
            Desc = desc;
        }
    }

    [System.Serializable]
    public class ToolIconEntry
    {
        public string toolId;
        public Sprite icon;
    }
}