using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Lookup<T> : ScriptableObject where T : Lookup<T> {
    static LookupTable<T> _table;

    static LookupTable<T> Table {
        get {
            if (_table != null) return _table;

            _table = Resources.Load<LookupTable<T>>(TablePath);
            if (_table == null) {
                Debug.LogError($"Lookup table not found at {TablePath}");
            }
            return _table;
        }   
    }

    static string TablePath => $"Lookups/{typeof(T)}Lookup";

    public static T GetById(int id) {
        return Table[id];
    }

    static int GetLookupId(T item) {
        return Table.Values.IndexOf(item);
    }

    public static T GetRandom() {
        return Table.Values.GetRandom();
    }
    
    public static IEnumerable<T> GetAll() {
        return Table.Values;
    }

    public int GetLookupId() {
        return GetLookupId((T) this);
    }
}