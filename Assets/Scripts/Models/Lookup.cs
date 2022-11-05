using UnityEditor;
using UnityEngine;

public abstract class Lookup<T> : ScriptableObject where T : Lookup<T> {
    static LookupTable _table;

    string _tablePath => $"Assets/Lookups/{typeof(T)} Lookup.asset";

    public static T GetById(int id) {
        return (T) _table[id];
    }

    public static int GetLookupId(T item) {
        return _table.Values.IndexOf(item);
    }

    public int GetLookupId() {
        return GetLookupId((T) this);
    }

    protected virtual void OnEnable() {
        if (_table == null) {
            var obj = AssetDatabase.LoadAssetAtPath(_tablePath, typeof(LookupTable));
            if (obj == null) {
                _table = CreateInstance<LookupTable>();
                AssetDatabase.CreateAsset(_table, _tablePath);
                Debug.Log($"Created new lookup table at {_tablePath} for {typeof(T)}");
            } else {
                _table = (LookupTable) obj;
            }
        }
    }
}