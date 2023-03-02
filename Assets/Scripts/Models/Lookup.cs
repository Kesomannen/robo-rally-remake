using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Lookup<T> : ScriptableObject where T : Lookup<T> {
    static LookupTable _table;

    static LookupTable Table {
        get {
            if (_table != null) return _table;

            _table = Resources.Load<LookupTable>(TablePath);
            if (_table == null) {
                Debug.LogError($"Lookup table not found at {TablePath}");
            }
            if (_table.Type != typeof(T)) {
                Debug.LogError($"Lookup table {TablePath} contains values of type {_table.Type}, but should contain values of type {typeof(T)}.", _table);
            }
            return _table;
        }
    }

    static string TablePath => $"Lookups/{typeof(T)}Lookup";

    public static T GetById(int id) {
        return (T) Table[id];
    }

    public static int GetLookupId(T item) {
        return Table.Values.IndexOf(item);
    }

    public static T GetRandom() {
        return (T) Table.Values.GetRandom();
    }
    
    public static IEnumerable<T> GetAll() {
        return Table.Values.Cast<T>();
    }

    public int GetLookupId() {
        return GetLookupId((T) this);
    }
}