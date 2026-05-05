using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace RootsOfLife
{
    /// <summary>
    /// Controlador principal de GameUI.uxml.
    /// Gestiona tabs, mapa, inventario, mejoras y ajustes.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class GameUIController : MonoBehaviour
    {
        [Header("Escena de menú")]
        [SerializeField] private string menuSceneName = "MenuScene";

        // ── Tabs ──────────────────────────────────────────────────────────────
        private readonly string[] _tabNames = { "mapa", "inventario", "enciclopedia", "mejoras", "ajustes" };
        private Dictionary<string, Button>        _tabs    = new();
        private Dictionary<string, VisualElement> _contents = new();
        private string _activeTab = "mapa";

        // ── Side Panel ────────────────────────────────────────────────────────
        private Label         _panelTitle;
        private Label         _panelDesc;
        private VisualElement _upgInfo;
        private Label         _upgLevel;
        private Label         _upgNext;
        private Label         _upgCapCurr;
        private Label         _upgCapNext;
        private Button        _btnUpgrade;
        private Label         _upgMaxLabel;
        private Button        _panelBack;

        // ── Mapa ──────────────────────────────────────────────────────────────
        private VisualElement _islandCentral;
        private VisualElement _islandOeste;
        private VisualElement _islandEste;
        private string _selectedIsland = "";

        // ── Inventario ────────────────────────────────────────────────────────
        private VisualElement _invGrid;

        // ── Mejoras ───────────────────────────────────────────────────────────
        private string _selectedTool = "";

        private static readonly List<ToolDef> Tools = new()
        {
            new ToolDef("regadera", "Regadera", "💧"),
            new ToolDef("hacha",    "Hacha",    "🪓"),
            new ToolDef("pico",     "Pico",     "⛏️"),
            new ToolDef("azada",    "Azada",    "🔨"),
        };

        private static readonly Dictionary<string, IslandDef> Islands = new()
        {
            ["central"] = new IslandDef("Isla Central",  "La isla principal del archipiélago. Aquí comenzó todo, y aquí yace el árbol más antiguo del mundo."),
            ["oeste"]   = new IslandDef("Isla Oeste",    "Una isla cubierta de bosques densos. Se dice que sus raíces llegan hasta el fondo del océano."),
            ["este"]    = new IslandDef("Isla Este",     "Pequeña pero rica en minerales. Los exploradores la llaman 'La Joya del Este'."),
        };

        // ── Unity ─────────────────────────────────────────────────────────────
        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            BindElements(root);
            RegisterCallbacks();
            SwitchTab("mapa");
        }

        // ─── Binding ──────────────────────────────────────────────────────────

        private void BindElements(VisualElement root)
        {
            foreach (var name in _tabNames)
            {
                _tabs[name]     = root.Q<Button>($"tab-{name}");
                _contents[name] = root.Q($"content-{name}");
            }

            _panelTitle  = root.Q<Label>("panel-title");
            _panelDesc   = root.Q<Label>("panel-desc");
            _upgInfo     = root.Q("upg-info");
            _upgLevel    = root.Q<Label>("upg-level");
            _upgNext     = root.Q<Label>("upg-next");
            _upgCapCurr  = root.Q<Label>("upg-cap-curr");
            _upgCapNext  = root.Q<Label>("upg-cap-next");
            _btnUpgrade  = root.Q<Button>("btn-upgrade");
            _upgMaxLabel = root.Q<Label>("upg-max-label");
            _panelBack   = root.Q<Button>("panel-back");

            _islandCentral = root.Q("island-central");
            _islandOeste   = root.Q("island-oeste");
            _islandEste    = root.Q("island-este");

            _invGrid = root.Q("inv-grid");
        }

        private void RegisterCallbacks()
        {
            foreach (var name in _tabNames)
            {
                string n = name;
                _tabs[n]?.RegisterCallback<ClickEvent>(_ => SwitchTab(n));
            }

            _islandCentral?.RegisterCallback<ClickEvent>(_ => SelectIsland("central"));
            _islandOeste?.RegisterCallback<ClickEvent>(_   => SelectIsland("oeste"));
            _islandEste?.RegisterCallback<ClickEvent>(_    => SelectIsland("este"));

            _panelBack?.RegisterCallback<ClickEvent>(_ => DeselectAll());
            _btnUpgrade?.RegisterCallback<ClickEvent>(_ => OnUpgradeTool());

            // Herramientas
            foreach (var tool in Tools)
            {
                string id = tool.Id;
                var btn = GetComponent<UIDocument>().rootVisualElement.Q<Button>($"tool-icon-{id}");
                btn?.RegisterCallback<ClickEvent>(_ => SelectTool(id));
            }

            // Ajustes — sliders
            var root = GetComponent<UIDocument>().rootVisualElement;
            RegisterSlider(root, "slider-music",    "val-music",
                v => GameSession.Instance.Data.settings.musicVolume = v);
            RegisterSlider(root, "slider-sfx",      "val-sfx",
                v => GameSession.Instance.Data.settings.sfxVolume = v);
            RegisterSlider(root, "slider-textsize",  "val-textsize",
                v => GameSession.Instance.Data.settings.textSize = v);
        }

        private void RegisterSlider(VisualElement root, string sliderId, string labelId, System.Action<float> onChanged)
        {
            var slider = root.Q<Slider>(sliderId);
            var label  = root.Q<Label>(labelId);
            if (slider == null) return;

            slider.RegisterValueChangedCallback(evt =>
            {
                float rounded = Mathf.Round(evt.newValue);
                if (label != null) label.text = rounded.ToString("0");
                onChanged?.Invoke(rounded);
                GameSession.Instance?.Save();
            });
        }

        // ─── Tabs ─────────────────────────────────────────────────────────────

        private void SwitchTab(string tab)
        {
            _activeTab     = tab;
            _selectedIsland = "";
            _selectedTool   = "";

            foreach (var name in _tabNames)
            {
                bool active = name == tab;
                SetTabActive(_tabs[name], active);
                SetVisible(_contents[name], active);
            }

            // Textos por defecto del panel
            var (title, desc) = tab switch
            {
                "mapa"         => ("Selección",   "En el mundo de Roots of Life, hay un montón de archipiélagos por explorar. ¿Podrás descubrir los secretos de todas sus islas?"),
                "inventario"   => ("Inventario",  "Tienes muchas herramientas. Cuanto más mejores su capacidad, más tiempo durarán."),
                "enciclopedia" => ("Enciclopedia","Descubre todos los seres vivos y recursos del mundo."),
                "mejoras"      => ("Mejoras",     "Mejora tus herramientas para que duren más y sean más eficaces."),
                "ajustes"      => ("Ajustes",     "Modifica los parámetros del juego a tu gusto."),
                _              => ("", "")
            };

            SetPanelText(title, desc);
            SetVisible(_upgInfo,  false);
            SetVisible(_panelBack, false);

            if (tab == "inventario") RefreshInventoryUI();
            if (tab == "mejoras")   RefreshUpgradesUI();
            if (tab == "ajustes")   RefreshSettingsUI();
        }

        // ─── Mapa ─────────────────────────────────────────────────────────────

        private void SelectIsland(string id)
        {
            _selectedIsland = id;
            ClearIslandSelection();

            VisualElement el = id switch
            {
                "central" => _islandCentral,
                "oeste"   => _islandOeste,
                "este"    => _islandEste,
                _         => null
            };
            el?.AddToClassList("island--selected");

            if (Islands.TryGetValue(id, out var island))
                SetPanelText(island.Name, island.Desc);

            SetVisible(_panelBack, true);
        }

        private void DeselectAll()
        {
            _selectedIsland = "";
            _selectedTool   = "";
            ClearIslandSelection();
            SwitchTab(_activeTab);
        }

        private void ClearIslandSelection()
        {
            _islandCentral?.RemoveFromClassList("island--selected");
            _islandOeste?.RemoveFromClassList("island--selected");
            _islandEste?.RemoveFromClassList("island--selected");
        }

        // ─── Inventario ───────────────────────────────────────────────────────

        private void RefreshInventoryUI()
        {
            if (_invGrid == null || GameSession.Instance == null) return;
            var inv = GameSession.Instance.Data.inventory;

            var slots = _invGrid.Query(className: "inv-slot").ToList();
            for (int i = 0; i < slots.Count && i < inv.Count; i++)
            {
                var slot = slots[i];
                slot.Clear();

                if (inv[i] != null)
                {
                    var lbl = new Label { text = "📦" };
                    var cnt = new Label { text = $"x{inv[i].count}" };
                    cnt.AddToClassList("item-count");
                    slot.Add(lbl);
                    slot.Add(cnt);
                    slot.RemoveFromClassList("inv-slot--empty");
                }
                else
                {
                    slot.AddToClassList("inv-slot--empty");
                }
            }
        }

        // ─── Mejoras ──────────────────────────────────────────────────────────

        private void RefreshUpgradesUI()
        {
            if (GameSession.Instance == null) return;
            foreach (var tool in Tools)
            {
                int lvl  = GameSession.Instance.Data.GetToolLevel(tool.Id);
                var root = GetComponent<UIDocument>().rootVisualElement;

                // Actualizar etiquetas de nombre y nivel
                root.Q<Label>($"tool-name-{tool.Id}")?.SetText(tool.Name);
                root.Q<Label>($"tool-lvl-{tool.Id}")?.SetText($"Lvl. {lvl}");

                // Actualizar barras de nivel
                var bars = root.Q($"bars-{tool.Id}")?.Query(className: "level-bar").ToList();
                if (bars == null) continue;
                for (int i = 0; i < bars.Count; i++)
                {
                    bool active  = i < lvl;
                    bool current = i == lvl - 1;
                    bars[i].EnableInClassList("level-bar--active",   active);
                    bars[i].EnableInClassList("level-bar--current",  current);
                }
            }
        }

        private void SelectTool(string id)
        {
            _selectedTool = id;

            // Reset selección visual
            var root = GetComponent<UIDocument>().rootVisualElement;
            foreach (var t in Tools)
                root.Q<Button>($"tool-icon-{t.Id}")?.RemoveFromClassList("tool-icon--selected");
            root.Q<Button>($"tool-icon-{id}")?.AddToClassList("tool-icon--selected");

            var tool = Tools.Find(t => t.Id == id);
            if (tool == null) return;

            int lvl = GameSession.Instance.Data.GetToolLevel(id);
            SetPanelText(tool.Name, "Mejora tu herramienta para obtener mayor capacidad y duración en el campo.");
            ShowUpgradeInfo(id, lvl);
        }

        private void ShowUpgradeInfo(string id, int lvl)
        {
            SetVisible(_upgInfo, true);
            _upgLevel.text = $"Nivel: Lvl.{lvl}";

            bool isMax = lvl >= 5;
            SetVisible(_upgNext,     !isMax);
            SetVisible(_upgCapCurr,  !isMax);
            SetVisible(_upgCapNext,  !isMax);
            SetVisible(_btnUpgrade,  !isMax);
            SetVisible(_upgMaxLabel,  isMax);

            if (!isMax)
            {
                _upgNext.text    = $"Siguiente mejora: 20 RC → Lvl. {lvl + 1}";
                _upgCapCurr.text = $"• Capacidad actual: {lvl * 10}";
                _upgCapNext.text = $"• Capacidad nueva: {(lvl + 1) * 10}";
                _btnUpgrade.text = $"⬆ Mejorar (20 RC)";
            }
        }

        private void OnUpgradeTool()
        {
            if (string.IsNullOrEmpty(_selectedTool)) return;
            bool upgraded = GameSession.Instance.Data.UpgradeTool(_selectedTool);
            if (upgraded)
            {
                GameSession.Instance.Save();
                RefreshUpgradesUI();
                int newLvl = GameSession.Instance.Data.GetToolLevel(_selectedTool);
                ShowUpgradeInfo(_selectedTool, newLvl);
            }
        }

        // ─── Ajustes ──────────────────────────────────────────────────────────

        private void RefreshSettingsUI()
        {
            if (GameSession.Instance == null) return;
            var s    = GameSession.Instance.Data.settings;
            var root = GetComponent<UIDocument>().rootVisualElement;

            SetSliderValue(root, "slider-music",   "val-music",    s.musicVolume);
            SetSliderValue(root, "slider-sfx",     "val-sfx",      s.sfxVolume);
            SetSliderValue(root, "slider-textsize", "val-textsize", s.textSize);
        }

        private static void SetSliderValue(VisualElement root, string sliderId, string labelId, float value)
        {
            var slider = root.Q<Slider>(sliderId);
            var label  = root.Q<Label>(labelId);
            if (slider != null) slider.SetValueWithoutNotify(value);
            if (label  != null) label.text = Mathf.Round(value).ToString("0");
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private void SetPanelText(string title, string desc)
        {
            if (_panelTitle != null) _panelTitle.text = title;
            if (_panelDesc  != null) _panelDesc.text  = desc;
        }

        private static void SetVisible(VisualElement el, bool visible)
        {
            if (el == null) return;
            el.EnableInClassList("unity-disabled", !visible);
        }

        private static void SetTabActive(Button btn, bool active)
        {
            if (btn == null) return;
            btn.EnableInClassList("tab--active", active);
        }
    }

    // ─── Tipos auxiliares internos ────────────────────────────────────────────

    internal class ToolDef
    {
        public string Id   { get; }
        public string Name { get; }
        public string Icon { get; }
        public ToolDef(string id, string name, string icon) { Id = id; Name = name; Icon = icon; }
    }

    internal class IslandDef
    {
        public string Name { get; }
        public string Desc { get; }
        public IslandDef(string name, string desc) { Name = name; Desc = desc; }
    }

    // Extensión para no llamar a text dos veces
    internal static class LabelExt
    {
        public static void SetText(this Label lbl, string text) { if (lbl != null) lbl.text = text; }
    }
}
