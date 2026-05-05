using UnityEngine;

[CreateAssetMenu(menuName = "RootsOfLife/Item")]
public class ItemDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;

    public int maxStack = 99; 
}
