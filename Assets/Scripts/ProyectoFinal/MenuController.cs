using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace RootsOfLife
{

    [RequireComponent(typeof(UIDocument))]
    public class MenuController : MonoBehaviour
    {
        [Header("Escena de juego")]
        [SerializeField] private string gameSceneName = "GameScene";

        // pantallas del menu
        private VisualElement _screenMenu;
        private VisualElement _screenSaves;
        private VisualElement _modalOverlay;

        // menu main
        private Button _btnJugar;
        private Button _btnSalir;

        // partidas
        private Button _btnBackMenu;
        private VisualElement[] _slotCards = new VisualElement[6];

        //pop up
        private Button    _modalClose;
        private Label     _modalIcon;
        private Label     _modalTitle;
        private TextField _modalRenameField;
        private VisualElement _modalActions;
        private VisualElement _modalActionsRename;
        private Button    _modalBtnPlay;
        private Button    _modalBtnRename;
        private Button    _modalBtnDelete;
        private Button    _modalBtnConfirm;
        private Button    _modalBtnCancel;

        private int _selectedSlot = -1;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            BindElements(root);
            RegisterCallbacks();
            ShowScreen("menu");
        }

        private void BindElements(VisualElement root)
        {
            _screenMenu   = root.Q("screen-menu");
            _screenSaves  = root.Q("screen-saves");
            _modalOverlay = root.Q("modal-overlay");

            _btnJugar    = root.Q<Button>("btn-jugar");
            _btnSalir    = root.Q<Button>("btn-salir");
            _btnBackMenu = root.Q<Button>("btn-back-menu");

            for (int i = 0; i < 6; i++)
                _slotCards[i] = root.Q($"slot-{i}");

            _modalClose         = root.Q<Button>("modal-close");
            _modalIcon          = root.Q<Label>("modal-icon");
            _modalTitle         = root.Q<Label>("modal-title");
            _modalRenameField   = root.Q<TextField>("modal-rename-field");
            _modalActions       = root.Q("modal-actions");
            _modalActionsRename = root.Q("modal-actions-rename");
            _modalBtnPlay       = root.Q<Button>("modal-btn-play");
            _modalBtnRename     = root.Q<Button>("modal-btn-rename");
            _modalBtnDelete     = root.Q<Button>("modal-btn-delete");
            _modalBtnConfirm    = root.Q<Button>("modal-btn-confirm");
            _modalBtnCancel     = root.Q<Button>("modal-btn-cancel");
        }

        private void RegisterCallbacks()
        {
            _btnJugar?.RegisterCallback<ClickEvent>(_ => ShowScreen("saves"));
            _btnSalir?.RegisterCallback<ClickEvent>(_ => QuitGame());
            _btnBackMenu?.RegisterCallback<ClickEvent>(_ => ShowScreen("menu"));

            for (int i = 0; i < 6; i++)
            {
                int idx = i;
                _slotCards[i]?.RegisterCallback<ClickEvent>(_ => OnSlotClicked(idx));
            }

            _modalClose?.RegisterCallback<ClickEvent>(_ => CloseModal());
            _modalBtnPlay?.RegisterCallback<ClickEvent>(_ => OnModalPlay());
            _modalBtnRename?.RegisterCallback<ClickEvent>(_ => OnModalRenameStart());
            _modalBtnDelete?.RegisterCallback<ClickEvent>(_ => OnModalDelete());
            _modalBtnConfirm?.RegisterCallback<ClickEvent>(_ => OnModalRenameConfirm());
            _modalBtnCancel?.RegisterCallback<ClickEvent>(_ => OnModalRenameCancel());
        }

        private void ShowScreen(string screen)
        {
            SetVisible(_screenMenu,   screen == "menu");
            SetVisible(_screenSaves,  screen == "saves");
            SetVisible(_modalOverlay, false);

            if (screen == "saves")
                RefreshSlots();
        }

        private void RefreshSlots()
        {
            for (int i = 0; i < 6; i++)
            {
                var info = SaveManager.Instance.GetSlotInfo(i);
                var card = _slotCards[i];
                if (card == null) continue;

                bool empty = info.isEmpty;

                card.Q<Label>($"slot-{i}-icon").text = empty ? "➕" : "🌿";
                card.Q<Label>($"slot-{i}-name").text = empty ? "Vacío" : info.slotName;

                // Clases visuales
                if (empty)
                {
                    card.AddToClassList("slot-card--empty");
                    card.Q<Label>($"slot-{i}-icon")?.AddToClassList("slot-icon--empty");
                    card.Q<Label>($"slot-{i}-name")?.AddToClassList("slot-name--empty");
                }
                else
                {
                    card.RemoveFromClassList("slot-card--empty");
                    card.Q<Label>($"slot-{i}-icon")?.RemoveFromClassList("slot-icon--empty");
                    card.Q<Label>($"slot-{i}-name")?.RemoveFromClassList("slot-name--empty");
                }
            }
        }

        private void OnSlotClicked(int slot)
        {
            _selectedSlot = slot;
            var info = SaveManager.Instance.GetSlotInfo(slot);
            OpenModal(info);
        }

        private void OpenModal(SaveSlotInfo info)
        {
            _modalIcon.text  = info.isEmpty ? "➕" : "🌿";
            _modalTitle.text = info.isEmpty ? "Nueva partida" : info.slotName;

            bool hasData = !info.isEmpty;
            SetVisible(_modalBtnPlay,   hasData);
            SetVisible(_modalBtnRename, hasData);
            SetVisible(_modalBtnDelete, hasData);

            if (!hasData)
            {
                SetVisible(_modalBtnPlay, true);
                _modalBtnPlay.text = "▶ Crear partida";
            }
            else
            {
                _modalBtnPlay.text = "▶ Jugar";
            }

            SetVisible(_modalActions,       true);
            SetVisible(_modalActionsRename, false);
            SetVisible(_modalRenameField,   false);

            SetVisible(_modalOverlay, true);
        }

        private void CloseModal()
        {
            SetVisible(_modalOverlay, false);
            _selectedSlot = -1;
        }

        private void OnModalPlay()
        {
            if (_selectedSlot < 0) return;

            var info = SaveManager.Instance.GetSlotInfo(_selectedSlot);
            if (info.isEmpty)
                SaveManager.Instance.CreateNewSlot(_selectedSlot, $"Partida {_selectedSlot + 1}");

            GameSession.Instance.OpenSlot(_selectedSlot);
            SceneManager.LoadScene(gameSceneName);
        }

        private void OnModalRenameStart()
        {
            var info = SaveManager.Instance.GetSlotInfo(_selectedSlot);
            _modalRenameField.value = info.slotName;
            SetVisible(_modalRenameField,   true);
            SetVisible(_modalActions,       false);
            SetVisible(_modalActionsRename, true);
        }

        private void OnModalRenameConfirm()
        {
            string newName = _modalRenameField.value.Trim();
            if (!string.IsNullOrEmpty(newName))
                SaveManager.Instance.SetSlotName(_selectedSlot, newName);

            CloseModal();
            RefreshSlots();
        }

        private void OnModalRenameCancel()
        {
            SetVisible(_modalRenameField,   false);
            SetVisible(_modalActions,       true);
            SetVisible(_modalActionsRename, false);
        }

        private void OnModalDelete()
        {
            SaveManager.Instance.DeleteSlot(_selectedSlot);
            CloseModal();
            RefreshSlots();
        }

        private static void SetVisible(VisualElement el, bool visible)
        {
            if (el == null) return;
            el.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
