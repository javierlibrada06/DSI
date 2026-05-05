using UnityEngine;

namespace RootsOfLife
{
    /// <summary>
    /// Contiene la sesión de juego activa: qué slot está abierto y los datos en RAM.
    /// Persiste entre escenas con DontDestroyOnLoad.
    /// </summary>
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        public int ActiveSlot { get; private set; } = -1;
        public GameSaveData Data { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>Carga un slot y lo pone como activo.</summary>
        public void OpenSlot(int slot)
        {
            ActiveSlot = slot;
            Data = SaveManager.Instance.LoadGame(slot);
        }

        /// <summary>Guarda los datos actuales en el slot activo.</summary>
        public void Save()
        {
            if (ActiveSlot < 0)
            {
                Debug.LogWarning("[GameSession] No hay slot activo.");
                return;
            }
            SaveManager.Instance.SaveGame(ActiveSlot, Data);
        }
    }
}
