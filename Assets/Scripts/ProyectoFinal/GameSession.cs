using UnityEngine;

namespace RootsOfLife
{
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

        public void OpenSlot(int slot)
        {
            ActiveSlot = slot;
            Data = SaveManager.Instance.LoadGame(slot);
        }

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
