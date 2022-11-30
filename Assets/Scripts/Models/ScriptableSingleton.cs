using UnityEngine;

public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T> {
    public static T Instance {
        get{
            if (_instance != null) return _instance;
            _instance = Resources.Load<T>(typeof(T).Name);
            return _instance;
        }
    }

    static T _instance;
}