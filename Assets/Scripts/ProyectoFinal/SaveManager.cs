using System;
using System.Collections.Generic;
using UnityEngine;

namespace RootsOfLife
{
    /// <summary>
    /// Gestiona guardado y carga de partidas (6 slots) usando PlayerPrefs + JSON.
    /// Singleton accesible desde cualquier sistema.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const int SLOT_COUNT = 6;
        private const string SLOT_META_KEY = "rol_slot_meta_{0}";
        private const string SLOT_DATA_KEY = "rol_slot_data_{0}";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ─── Metadatos de slot ────────────────────────────────────────────────

        public SaveSlotInfo GetSlotInfo(int slot)
        {
            ValidateSlot(slot);
            string json = PlayerPrefs.GetString(string.Format(SLOT_META_KEY, slot), "");
            if (string.IsNullOrEmpty(json))
                return new SaveSlotInfo();
            return JsonUtility.FromJson<SaveSlotInfo>(json) ?? new SaveSlotInfo();
        }

        public void SetSlotName(int slot, string name)
        {
            ValidateSlot(slot);
            var info = GetSlotInfo(slot);
            info.slotName = name;
            info.isEmpty = false;
            info.lastPlayed = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            SaveSlotMeta(slot, info);
        }

        public void DeleteSlot(int slot)
        {
            ValidateSlot(slot);
            PlayerPrefs.DeleteKey(string.Format(SLOT_META_KEY, slot));
            PlayerPrefs.DeleteKey(string.Format(SLOT_DATA_KEY, slot));
            PlayerPrefs.Save();
        }


        // ─── Datos de juego ───────────────────────────────────────────────────

        public GameSaveData LoadGame(int slot)
        {
            ValidateSlot(slot);
            string json = PlayerPrefs.GetString(string.Format(SLOT_DATA_KEY, slot), "");

            GameSaveData data;

            if (string.IsNullOrEmpty(json))
                data = new GameSaveData();
            else
                data = JsonUtility.FromJson<GameSaveData>(json) ?? new GameSaveData();

            data.EnsureInventorySize(); // 🔥 CLAVE

            return data;
        }

        public void SaveGame(int slot, GameSaveData data)
        {
            ValidateSlot(slot);
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(string.Format(SLOT_DATA_KEY, slot), json);

            // Actualizar metadatos
            var info = GetSlotInfo(slot);
            info.isEmpty = false;
            info.lastPlayed = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            SaveSlotMeta(slot, info);
            PlayerPrefs.Save();
        }

        public void CreateNewSlot(int slot, string name = "")
        {
            ValidateSlot(slot);
            var data = new GameSaveData();
            SaveGame(slot, data);
            if (!string.IsNullOrEmpty(name))
                SetSlotName(slot, name);
        }

        // ─── Helpers privados ─────────────────────────────────────────────────

        private void SaveSlotMeta(int slot, SaveSlotInfo info)
        {
            PlayerPrefs.SetString(string.Format(SLOT_META_KEY, slot), JsonUtility.ToJson(info));
            PlayerPrefs.Save();
        }

        private void ValidateSlot(int slot)
        {
            if (slot < 0 || slot >= SLOT_COUNT)
                throw new ArgumentOutOfRangeException(nameof(slot), $"Slot debe estar entre 0 y {SLOT_COUNT - 1}.");
        }
    }
}
