using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RootsOfLife/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemDefinition> items = new List<ItemDefinition>();

    private Dictionary<string, ItemDefinition> _dict;

    public void Init()
    {
        _dict = new Dictionary<string, ItemDefinition>();
        if (items == null) return;
        foreach (var item in items)
        {
            if (item != null && !string.IsNullOrEmpty(item.id))
                _dict[item.id] = item;
        }
    }

    public ItemDefinition Get(string id)
    {
        if (_dict == null) Init();
        if (string.IsNullOrEmpty(id)) return null;
        return _dict.TryGetValue(id, out var item) ? item : null;
    }
}
