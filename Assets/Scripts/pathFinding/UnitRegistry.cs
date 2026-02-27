using UnityEngine;

[CreateAssetMenu(fileName= "UnitRegistry", menuName= "Game/UnitRegistry")]
public class UnitRegistry : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public UnitType unitType;
        public GameObject prefab;
    }
    
    public Entry[] entries;

    private System.Collections.Generic.Dictionary<UnitType, GameObject> _map;

    public void Build()
    {
        _map = new System.Collections.Generic.Dictionary<UnitType, GameObject>(entries.Length);

        foreach (var e in entries)
        {
            if(e.prefab == null) continue;
            if (_map.ContainsKey(e.unitType))
            {
                Debug.LogWarning($"Duplicate unit type {e.unitType}, will overwrite");
            }
            _map[e.unitType] = e.prefab;
        }
    }

    public GameObject GetUnit(UnitType unitType)
    {
        if(_map == null) Build();
        return _map.TryGetValue(unitType, out var obj) ? obj : null;
    }
}
