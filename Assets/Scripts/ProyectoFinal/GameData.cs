using System;
using System.Collections.Generic;
using UnityEngine;

namespace RootsOfLife
{
    [Serializable]
    public class SaveSlotInfo
    {
        public bool isEmpty = true;
        public string slotName = "Vacío";
        public string lastPlayed = "";
    }

    [Serializable]
    public class ToolData
    {
        public string id;
        public int level = 1;
    }

    [Serializable]
    public class InventoryItemData
    {
        public string itemId;
        public int count;
    }

    [Serializable]
    public class SettingsData
    {
        public float musicVolume = 60f;
        public float sfxVolume = 75f;
        public float textSize = 14f;
    }

    [Serializable]
    public class GameSaveData
    {
        public List<ToolData> tools = new List<ToolData>();
        public List<InventoryItemData> inventory = new List<InventoryItemData>();
        public SettingsData settings = new SettingsData();

        public GameSaveData()
        {
            // Inicializar inventario con 30 slots vacíos
            for (int i = 0; i < 30; i++)
                inventory.Add(null);

            // Herramientas por defecto
            tools.Add(new ToolData { id = "regadera", level = 1 });
            tools.Add(new ToolData { id = "hacha",    level = 1 });
            tools.Add(new ToolData { id = "pico",     level = 1 });
            tools.Add(new ToolData { id = "azada",    level = 1 });
        }

        public ToolData GetTool(string id)
        {
            return tools.Find(t => t.id == id);
        }

        public int GetToolLevel(string id)
        {
            var t = GetTool(id);
            return t != null ? t.level : 1;
        }

        public bool UpgradeTool(string id)
        {
            var t = GetTool(id);
            if (t == null || t.level >= 5) return false;
            t.level++;
            return true;
        }
    }
}
