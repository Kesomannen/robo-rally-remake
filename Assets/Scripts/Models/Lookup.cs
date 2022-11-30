using UnityEngine;

public abstract class Lookup<T> : ScriptableObject where T : Lookup<T> {
    static LookupTable _table;

    static LookupTable Table {
        get {
            if (_table != null) return _table;

            _table = Resources.Load<LookupTable>(_tablePath);
            if (_table == null) {
                Debug.LogError($"Lookup table not found at {_tablePath}");
            }
            Debug.Log(_table);
            return _table;
        }
    }

    static string _tablePath => $"Lookups/{typeof(T)}Lookup";

    public static T GetById(int id) {
        return (T) Table[id];
    }

    public static int GetLookupId(T item) {
        return Table.Values.IndexOf(item);
    }

    public int GetLookupId() {
        return GetLookupId((T) this);
    }
}