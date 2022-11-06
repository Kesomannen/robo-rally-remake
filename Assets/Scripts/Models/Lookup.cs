using UnityEditor;
using UnityEngine;

public abstract class Lookup<T> : ScriptableObject where T : Lookup<T> {
    static LookupTable _table;

    public static LookupTable Table {
        get {
            if (_table == null) {
                _table = AssetDatabase.LoadAssetAtPath<LookupTable>(_tablePath);
                if (_table == null) {
                    _table = CreateInstance<LookupTable>();
                    AssetDatabase.CreateAsset(_table, _tablePath);
                    Debug.Log($"Created new lookup table at {_tablePath} for {typeof(T)}");
                }
            }
            return _table;
        }
    }

    static string _tablePath => $"Assets/Lookups/{typeof(T)}Lookup.asset";

    public static T GetById(int id) {
        Debug.Log(Table[id]);
        return (T) Table[id];
    }

    public static int GetLookupId(T item) {
        return Table.Values.IndexOf(item);
    }

    public int GetLookupId() {
        return GetLookupId((T) this);
    }
}