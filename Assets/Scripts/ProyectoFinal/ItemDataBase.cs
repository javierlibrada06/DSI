using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RootsOfLife/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemDefinition> items;

    private Dictionary<string, ItemDefinition> _dict;

    public void Init()
    {
        _dict = new Dictionary<string, ItemDefinition>();
        foreach (var item in items)
            _dict[item.id] = item;
    }

    public ItemDefinition Get(string id)
    {
        if (_dict == null) Init();
        return _dict.TryGetValue(id, out var item) ? item : null;
    }
}